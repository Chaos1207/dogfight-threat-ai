using DogfightThreatAI.Core.Math;
using DogfightThreatAI.Core.World;
using UnityEngine;

namespace DogfightThreatAI.Gameplay.World
{
    /// <summary>
    /// 【调试可视化】把"隐形的逻辑网格"用 Gizmos 画出来。
    ///
    /// 这是理解与调试 AI 的唯一实用手段：
    ///   · 平时游戏里什么都看不到（玩家不该看到格子）
    ///   · 在 Scene 视图里选中这个物体，就能看到覆盖整张地图的格子线
    ///   · 以后 ThreatGrid 接上来，这里会变成"威胁热力图"（每格按威胁值染色）——答辩的招牌画面
    ///
    /// ⚠️ Gizmos 只在编辑器/开发版里绘制，正式发布包里没有开销。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GridDebugView : MonoBehaviour
    {
        [Header("网格配置（必须和逻辑网格一致）")]
        [Tooltip("世界尺寸。例如 400 × 300")]
        [SerializeField] Vector2 worldSize = new Vector2(400f, 300f);

        [Tooltip("单格边长。例如 4")]
        [SerializeField] float cellSize = 4f;

        [Header("绘制")]
        [Tooltip("Play 模式下也绘制（调试 AI 时建议保持开启）")]
        [SerializeField] bool drawInPlayMode = true;

        [Tooltip("每隔几条画一条粗线（例如 4 条 = 每 16 单位一条主线）")]
        [SerializeField, Min(1)] int majorEvery = 4;

        [SerializeField] Color lineColor = new Color(1f, 1f, 1f, 0.10f);
        [SerializeField] Color majorLineColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] Color borderColor = new Color(0.3f, 1f, 0.6f, 0.9f);

        Grid2D _grid;

        /// <summary>供其他系统读取（ThreatGrid 会复用同一份配置）。</summary>
        public Grid2D Grid => _grid;

        void OnValidate() => Rebuild();

        void Awake() => Rebuild();

        void Rebuild()
        {
            if (cellSize <= 0f) cellSize = 0.01f;
            _grid = Grid2D.Centered(new Vec2(worldSize.x, worldSize.y), cellSize);
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying && !drawInPlayMode) return;
            if (cellSize <= 0f) return;

            // OnDrawGizmos 可能在 Awake 之前被调用，所以这里也重建一次
            if (_grid.CellSize <= 0f || Mathf.Abs(_grid.CellSize - cellSize) > 1e-4f) Rebuild();
            if (_grid.CellCount <= 0) return;

            float halfW = _grid.Bounds.Width * 0.5f;
            float halfH = _grid.Bounds.Height * 0.5f;

            // 竖线
            for (int x = 0; x <= _grid.Width; x++)
            {
                float px = -halfW + x * cellSize;
                Gizmos.color = (x % majorEvery == 0) ? majorLineColor : lineColor;
                Gizmos.DrawLine(new Vector3(px, -halfH, 0f), new Vector3(px, halfH, 0f));
            }

            // 横线
            for (int y = 0; y <= _grid.Height; y++)
            {
                float py = -halfH + y * cellSize;
                Gizmos.color = (y % majorEvery == 0) ? majorLineColor : lineColor;
                Gizmos.DrawLine(new Vector3(-halfW, py, 0f), new Vector3(halfW, py, 0f));
            }

            // 战场边界（粗一圈，方便确认飞机不会飞出去）
            Gizmos.color = borderColor;
            var c0 = new Vector3(-halfW, -halfH, 0f);
            var c1 = new Vector3(halfW, -halfH, 0f);
            var c2 = new Vector3(halfW, halfH, 0f);
            var c3 = new Vector3(-halfW, halfH, 0f);
            Gizmos.DrawLine(c0, c1);
            Gizmos.DrawLine(c1, c2);
            Gizmos.DrawLine(c2, c3);
            Gizmos.DrawLine(c3, c0);
        }
    }
}
