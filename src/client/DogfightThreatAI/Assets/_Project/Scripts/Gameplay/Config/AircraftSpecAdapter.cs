using DogfightThreatAI.Core.Config;

namespace DogfightThreatAI.Gameplay.Config
{
    /// <summary>
    /// 将 AircraftData 转换为 AircraftSpec。
    /// 模拟层不引用 ScriptableObject，Unity 类型到纯数据的转换集中在此。
    /// </summary>
    public static class AircraftSpecAdapter
    {
        public static AircraftSpec ToSpec(AircraftData data)
            => new AircraftSpec(data.MinSpeed, data.MaxSpeed, data.Acceleration,
                                data.TurnRateDegPerSec, data.Drag,
                                data.LateralGrip, data.DriftScrub);
    }
}
