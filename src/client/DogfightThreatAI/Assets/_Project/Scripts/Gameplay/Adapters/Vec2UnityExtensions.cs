using DogfightThreatAI.Core.Math;
using UnityEngine;

namespace DogfightThreatAI.Gameplay.Adapters
{
    /// <summary>
    /// Vec2 与 UnityEngine.Vector2 的转换扩展。
    /// Core 不引用 UnityEngine，相关转换放在 Gameplay 层。
    /// </summary>
    public static class Vec2UnityExtensions
    {
        public static Vector2 ToUnity(this Vec2 v) => new Vector2(v.X, v.Y);
        public static Vec2 ToCore(this Vector2 v) => new Vec2(v.x, v.y);
    }
}
