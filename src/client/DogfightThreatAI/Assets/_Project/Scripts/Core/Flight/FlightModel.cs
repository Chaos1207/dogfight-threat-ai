using System;
using DogfightThreatAI.Core.Config;
using DogfightThreatAI.Core.Input;
using DogfightThreatAI.Core.Math;

namespace DogfightThreatAI.Gameplay.Flight
{
    /// <summary>
    /// 【模拟层 / Simulate】飞机的运动学积分器。整个项目最核心的一个类。
    ///
    /// ★ 三条纪律（破一条，11 月就要返工）：
    ///   1. 不继承 MonoBehaviour —— 它不是组件，是纯逻辑
    ///   2. 不引用 UnityEngine —— 连 Vector2 都不用（用 Core.Math.Vec2）
    ///   3. 不碰 transform / Input / Time / Sprite —— 那些是"外面的事"
    ///
    /// 满足这三条之后，这个类可以：
    ///   ✅ 原样复制进 LogicServer（纯 .NET 进程）跑服务器权威模拟
    ///   ✅ 被 AI 连续调用 60 次做"推演"（威胁规避路径规划需要）
    ///   ✅ 在 EditMode 测试里一秒跑一万次，做批量胜率统计
    ///
    /// ── 运动模型（漂移版）────────────────────────────────
    /// 每步把速度分解成两个轴：
    ///     前进分量（沿机头）  → 由油门控制，加速/减速有惯性
    ///     横向分量（垂直机头） → 被「横向抓地力 lateralGrip」指数衰减
    ///
    /// 机头一转，原来的速度在新坐标系里就多出横向分量 → 这就是甩尾的来源。
    /// grip 大 → 横向速度很快被吃掉 → 贴地、像钉在轨道上
    /// grip 小 → 横向速度保留久 → 甩得厉害（赛车漂移）
    /// </summary>
    public sealed class FlightModel
    {
        readonly float _minSpeed;
        readonly float _maxSpeed;
        readonly float _acceleration;
        readonly float _turnRateRad;
        readonly float _drag;
        readonly float _lateralGrip;
        readonly float _driftScrub;
        readonly Bounds2D _arena;
        readonly bool _hasArena;

        /// <param name="minSpeed">失速下界（单位/秒）</param>
        /// <param name="maxSpeed">极速上界（单位/秒）</param>
        /// <param name="acceleration">前进速度变化率（单位/秒²）</param>
        /// <param name="turnRateDegPerSec">最大转向速率（度/秒）</param>
        /// <param name="drag">速度阻尼（0 = 无）</param>
        /// <param name="lateralGrip">横向抓地力（1/秒）。越大越贴地，越小越甩尾</param>
        /// <param name="driftScrub">漂移掉速系数（0~1）。侧滑越厉害，前进极速越低</param>
        /// <param name="arena">战场边界；null = 无限大（调试用）</param>
        public FlightModel(float minSpeed, float maxSpeed, float acceleration,
                           float turnRateDegPerSec, float drag,
                           float lateralGrip = 6f, float driftScrub = 0.35f,
                           Bounds2D? arena = null)
        {
            _minSpeed     = minSpeed;
            _maxSpeed     = maxSpeed < minSpeed ? minSpeed : maxSpeed;
            _acceleration = acceleration;
            _turnRateRad  = turnRateDegPerSec * 0.01745329252f;   // 度 → 弧度（不依赖 Mathf）
            _drag         = drag;
            _lateralGrip  = lateralGrip < 0f ? 0f : lateralGrip;
            _driftScrub   = Clamp(driftScrub, 0f, 0.95f);
            _hasArena     = arena.HasValue;
            _arena        = arena.GetValueOrDefault();
        }

        /// <summary>从 ScriptableObject 构造。模拟层本身不认 Unity 类型。</summary>
        public static FlightModel From(AircraftData data, Bounds2D? arena = null)
            => new FlightModel(data.MinSpeed, data.MaxSpeed, data.Acceleration,
                               data.TurnRateDegPerSec, data.Drag,
                               data.LateralGrip, data.DriftScrub, arena);

        /// <summary>
        /// 推进一个固定时间步。调用方必须传【固定】dt（如 Time.fixedDeltaTime），
        /// 否则服务器与客户端的模拟结果会发散。
        /// </summary>
        public void Step(ref FlightState state, in FlightInput input, float dt)
        {
            if (dt <= 0f) return;

            // 0) 输入夹紧：防止脏输入（网络包被篡改 / 读取错误）破坏物理
            float throttle = Clamp(input.Throttle, -1f, 1f);
            float steer    = Clamp(input.Steer,    -1f, 1f);

            // 1) 转机头 —— 有限角速度，所以"掉头需要时间"
            state.Heading += steer * _turnRateRad * dt;

            // 2) 把速度分解到「机头方向」和「垂直机头方向」
            //    这是漂移的数学基础：机头一转，旧速度在新坐标系里就多出横向分量。
            Vec2 forward = state.Forward;
            Vec2 right   = new Vec2(forward.Y, -forward.X);      // 与机头垂直
            float forwardSpeed = Vec2.Dot(state.Velocity, forward);
            float lateralSpeed = Vec2.Dot(state.Velocity, right);

            // 3) 前进分量：油门控制
            //    W（>0）：加速到极速；至少给一个起步速度
            //    S（<0）：刹到 0
            //    无输入（=0）：保持当前前进速度 —— 惯性滑行，**绝不会自己加速**
            float targetForwardSpeed;
            if (throttle > 0f)
                targetForwardSpeed = Clamp(throttle * _maxSpeed, _minSpeed, _maxSpeed);
            else if (throttle < 0f)
                targetForwardSpeed = 0f;
            else
                targetForwardSpeed = forwardSpeed;

            // 3b) 漂移掉速：侧滑越厉害，前进极速越低（模拟机翼侧滑的阻力）
            //     不做这个的话，漂移是"免费"的，玩家会一直拖着侧滑飞
            float totalSpeed = state.Speed;
            if (_driftScrub > 0f && totalSpeed > 1e-3f)
            {
                float slipRatio = MathF.Abs(lateralSpeed) / totalSpeed;   // 0 = 纯前飞，1 = 纯侧滑
                targetForwardSpeed *= 1f - _driftScrub * slipRatio;
            }

            // 3c) 前进速度以有限加速度逼近目标 —— 惯性在这一步
            forwardSpeed = MoveTowardsScalar(forwardSpeed, targetForwardSpeed, _acceleration * dt);

            // 4) 横向分量被「抓地力」吃掉 —— 漂移与收敛都在这一步
            //    grip 越大 → exp 衰减越快 → 立刻咬住航线
            //    grip 越小 → 横向速度保留越久 → 甩得越远
            lateralSpeed *= MathF.Exp(-_lateralGrip * dt);

            // 5) 阻尼（对总速度的简单近似）
            if (_drag > 0f)
            {
                float damp = 1f / (1f + _drag * dt);
                forwardSpeed *= damp;
                lateralSpeed *= damp;
            }

            // 6) 重新合成速度并位移
            state.Velocity = forward * forwardSpeed + right * lateralSpeed;
            state.Position += state.Velocity * dt;

            // 7) 战场边界：越界就贴边，并把"朝外"的速度分量吃掉
            //    （只夹住位置而不处理速度的话，飞机会贴着墙一直抖）
            if (_hasArena)
            {
                Vec2 clamped = _arena.Clamp(state.Position);
                if (!clamped.Equals(state.Position))
                {
                    Vec2 outward = state.Position - clamped;          // 朝墙外的方向
                    float intoWall = Vec2.Dot(state.Velocity, outward.Normalized);
                    if (intoWall > 0f) state.Velocity -= outward.Normalized * intoWall;
                    state.Position = clamped;
                }
            }
        }

        /// <summary>标量版的"向目标靠拢"，不依赖 Mathf（保持模拟层零 Unity 依赖）。</summary>
        static float MoveTowardsScalar(float current, float target, float maxDelta)
        {
            float d = target - current;
            if (MathF.Abs(d) <= maxDelta) return target;
            return current + MathF.Sign(d) * maxDelta;
        }

        static float Clamp(float v, float lo, float hi)
            => v < lo ? lo : (v > hi ? hi : v);
    }
}
