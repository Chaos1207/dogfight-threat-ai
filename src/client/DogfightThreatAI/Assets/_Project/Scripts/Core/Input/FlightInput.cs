namespace DogfightThreatAI.Core.Input
{
    /// <summary>
    /// 【契约】一次"操作意图"。
    ///
    /// 这是整个项目最重要的数据契约之一：玩家、AI、网络远程输入，
    /// 产出的是同一个结构。所以 FlightModel 永远不需要知道"是谁在开飞机"。
    ///
    /// 注意：它是"意图"（想要多少油门、往哪转），不是"结果"（最终速度、位置）。
    /// 位置和速度只由 FlightModel 算出来——这就是服务器权威的基础。
    /// </summary>
    public struct FlightInput
    {
        /// <summary>油门：-1（减速）~ 1（加速）</summary>
        public float Throttle;

        /// <summary>转向：-1（左转）~ 1（右转）</summary>
        public float Steer;

        public FlightInput(float throttle, float steer)
        {
            Throttle = throttle;
            Steer = steer;
        }

        public static FlightInput Neutral => new FlightInput(0f, 0f);
    }
}
