using DogfightThreatAI.Core.Config;
using DogfightThreatAI.Core.Input;
using DogfightThreatAI.Core.Math;
using DogfightThreatAI.Gameplay.Flight;
using UnityEngine;

namespace DogfightThreatAI.Gameplay
{
    /// <summary>
    /// 【胶水层】把三样东西接起来：
    ///     输入源（Keyboard / AI / 网络） ──► FlightModel（纯 C# 模拟） ──► Rigidbody2D（落地）
    ///     以及把当前状态交给 PlayerView 去画。
    ///
    /// 它自己**不含任何飞行数学**，也**不含任何绘制代码**。
    /// 胶水越薄越好——它一变胖，就说明有逻辑跑错地方了。
    ///
    /// 单机阶段：本地读输入 → 本地模拟 → 写位置。
    /// 联机阶段：这里会变成"只上报输入"，模拟搬到 LogicServer，位置来自服务器快照。
    ///           到时候改的是 FixedUpdate 里的两行，FlightModel 一行都不用动。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("数据（先设计数据，再搭系统）")]
        [Tooltip("这架飞机的档案：手感数值 + 美术引用。换飞机 = 换这个 asset")]
        [SerializeField] AircraftData ship;

        [Header("接线")]
        [Tooltip("输入源。单机阶段挂 KeyboardInputSource；以后可换成 AI 或网络输入源")]
        [SerializeField] MonoBehaviour inputSource;
        [SerializeField] Rigidbody2D body;
        [SerializeField] PlayerView view;

        [Header("战场边界（固定镜头方案用）")]
        [Tooltip("以世界原点为中心的半尺寸。\n" +
                 "Orthographic Size = 5、16:9 时，屏幕可见半高 5、半宽约 8.9。\n" +
                 "填 (8.5, 4.5) 左右可以让飞机刚好在屏幕内活动。\n" +
                 "填 (0,0) 或负数 = 不限制（无限世界）")]
        [SerializeField] Vector2 arenaHalfSize = new Vector2(8.5f, 4.5f);

        FlightModel _model;
        FlightState _state;

        // 上一个固定步的状态，只给渲染插值用（不进 FlightModel，保持模拟层纯粹）
        FlightState _previous;
        bool _hasPrevious;

        public AircraftData Ship => ship;

        void Reset() => body = GetComponent<Rigidbody2D>();

        void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (view == null) view = GetComponentInChildren<PlayerView>();

            if (ship == null)
            {
                Debug.LogError($"{name}: 没有指定 Ship Definition", this);
                enabled = false;
                return;
            }
            if (inputSource == null || !(inputSource is IPlayerInputSource))
            {
                Debug.LogError($"{name}: inputSource 必须挂一个实现了 IPlayerInputSource 的组件", this);
                enabled = false;
                return;
            }

            // 战场边界：半尺寸为正才启用，否则视为无限世界
            Bounds2D? arena = null;
            if (arenaHalfSize.x > 0f && arenaHalfSize.y > 0f)
                arena = Bounds2D.Centered(new Vec2(arenaHalfSize.x, arenaHalfSize.y));

            _model = FlightModel.From(ship, arena);

            // 初始机头朝向 = 美术上机头的朝向。
            // Kenney 的舰船图朝上（偏移 90°），所以初始就朝 +Y 飞，视觉和物理一致；
            // 如果换成朝右的图，只需改 AircraftData 的 Visual Forward Offset，不用改代码。
            _state = FlightState.At(new Vec2(transform.position.x, transform.position.y),
                                    ship.VisualForwardOffsetDeg * Mathf.Deg2Rad);

            if (view != null) view.SetVisualForward(ship.VisualForwardOffsetDeg);
        }

        void FixedUpdate()
        {
            var source = (IPlayerInputSource)inputSource;
            var input  = source.ReadInput(Time.fixedDeltaTime);

            if (_hasPrevious) _previous = _state;   // 先存旧状态，再步进
            _model.Step(ref _state, input, Time.fixedDeltaTime);
            _hasPrevious = true;

            // 位置以物理引擎为落地方式（Kinematic 刚体不会自作主张改位置）
            body.MovePosition(_state.Position);
        }

        void Update()
        {
            if (view == null) return;

            float alpha = Time.fixedDeltaTime > 0f
                ? Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime)
                : 0f;
            view.Render(_state, _hasPrevious ? _previous : _state, alpha, ship.BodySprite);
        }
    }
}
