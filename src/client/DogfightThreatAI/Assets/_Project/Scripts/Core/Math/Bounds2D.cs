using System;

namespace DogfightThreatAI.Core.Math
{
    /// <summary>
    /// 二维矩形边界（战场范围）。
    ///
    /// 为什么放在 Core 而不是靠 Unity 的 Collider：
    ///   · 边界判定属于【模拟层】——服务器也必须算，否则客户端一改就能飞出战场
    ///   · 纯数据、无 Unity 依赖，能进 EditMode 测试、能进网络快照
    ///
    /// 以后要做"越界扣血 / 撞墙反弹 / 强制返航"，都改这里，不用碰 Unity 物理。
    /// </summary>
    [Serializable]
    public struct Bounds2D
    {
        public Vec2 Min;
        public Vec2 Max;

        public Bounds2D(Vec2 min, Vec2 max)
        {
            Min = new Vec2(MathF.Min(min.X, max.X), MathF.Min(min.Y, max.Y));
            Max = new Vec2(MathF.Max(min.X, max.X), MathF.Max(min.Y, max.Y));
        }

        /// <summary>以原点为中心、给定半尺寸的矩形。</summary>
        public static Bounds2D Centered(Vec2 halfSize)
            => new Bounds2D(halfSize * -1f, halfSize);

        public float Width  => Max.X - Min.X;
        public float Height => Max.Y - Min.Y;
        public Vec2  Center => (Min + Max) * 0.5f;

        public Vec2 Clamp(Vec2 p)
            => new Vec2(MathF.Max(Min.X, MathF.Min(p.X, Max.X)),
                        MathF.Max(Min.Y, MathF.Min(p.Y, Max.Y)));

        public bool Contains(Vec2 p)
            => p.X >= Min.X && p.X <= Max.X && p.Y >= Min.Y && p.Y <= Max.Y;

        public override string ToString() => $"[{Min} → {Max}]";
    }
}
