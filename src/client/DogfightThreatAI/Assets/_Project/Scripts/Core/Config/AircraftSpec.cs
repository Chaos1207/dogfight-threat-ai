using System;

namespace DogfightThreatAI.Core.Config
{
    /// <summary>
    /// 飞机的纯配置数据。
    /// 该结构不依赖 Unity，由 AircraftData 经 AircraftSpecAdapter 转换得到。
    /// </summary>
    [Serializable]
    public readonly struct AircraftSpec
    {
        public readonly float MinSpeed;
        public readonly float MaxSpeed;
        public readonly float Acceleration;
        public readonly float TurnRateDegPerSec;
        public readonly float Drag;
        public readonly float LateralGrip;
        public readonly float DriftScrub;

        public AircraftSpec(float minSpeed, float maxSpeed, float acceleration,
                            float turnRateDegPerSec, float drag,
                            float lateralGrip, float driftScrub)
        {
            MinSpeed = minSpeed;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            TurnRateDegPerSec = turnRateDegPerSec;
            Drag = drag;
            LateralGrip = lateralGrip;
            DriftScrub = driftScrub;
        }
    }
}
