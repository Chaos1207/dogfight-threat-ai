using System;
using DogfightThreatAI.Core.Math;

namespace DogfightThreatAI.Gameplay.Flight
{
    /// <summary>
    /// 【数据层】飞机某一时刻的完整状态。纯数据、可复制、可序列化、可回滚。
    ///
    /// 它是"状态"，不是"配置"：
    ///   - 配置（最大速度）→ AircraftData（ScriptableObject，只读）
    ///   - 状态（当前速度）→ 这里（每帧变，会进网络快照）
    ///
    /// 联机阶段这个结构体会被写进快照发给客户端，所以它必须简单、可复制、零引用。
    /// </summary>
    [Serializable]
    public struct FlightState
    {
        /// <summary>位置（世界坐标，单位）</summary>
        public Vec2 Position;

        /// <summary>速度向量（单位/秒）。大小即当前速度。</summary>
        public Vec2 Velocity;

        /// <summary>机头朝向（弧度）。**不做取模**，允许累积到很大——这样渲染插值不会在 ±180° 处跳变。</summary>
        public float Heading;

        public float Speed => Velocity.Magnitude;

        /// <summary>机头单位向量</summary>
        public Vec2 Forward => Vec2.FromAngle(Heading);

        public static FlightState At(Vec2 position, float heading = 0f)
            => new FlightState { Position = position, Velocity = Vec2.Zero, Heading = heading };
    }
}
