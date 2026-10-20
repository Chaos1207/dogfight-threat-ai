namespace DogfightThreatAI.Core.Input
{
    /// <summary>
    /// 飞行输入源接口。
    /// 当前由键盘实现，后续可增加 AI 与网络输入实现。
    /// PlayerController 仅依赖该接口，不依赖具体输入源。
    /// </summary>
    public interface IPlayerInputSource
    {
        /// <summary>读取本帧的操作意图。deltaTime 供需要积分的输入源使用。</summary>
        FlightInput ReadInput(float deltaTime);
    }
}
