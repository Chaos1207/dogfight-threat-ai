namespace DogfightThreatAI.Core.Input
{
    /// <summary>
    /// 【接口】输入源。之所以要接口，是因为现在就能说出三种实现：
    ///   1. KeyboardInputSource —— 本地玩家（现在）
    ///   2. AiInputSource       —— AI 决策（10 月）
    ///   3. RemoteInputSource   —— 服务器/其他客户端发来的输入（11 月）
    ///
    /// PlayerController 只认这个接口，所以加第 2、3 种实现时，它一行都不用改。
    /// 这就是"用胶水代码连接，而不是让代码互相认识"。
    /// </summary>
    public interface IPlayerInputSource
    {
        /// <summary>读取本帧的操作意图。deltaTime 供需要积分的输入源使用。</summary>
        FlightInput ReadInput(float deltaTime);
    }
}
