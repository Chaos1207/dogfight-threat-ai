using DogfightThreatAI.Core.Math;
using DogfightThreatAI.Core.Flight;
using UnityEngine;

namespace DogfightThreatAI.Gameplay
{
    /// <summary>
    /// 飞机的表现组件。
    /// 负责位置插值、机头旋转和精灵切换，不参与飞行计算。
    /// 该组件需要挂载在飞机的 Art 子物体上，避免与 Rigidbody2D 争夺 Transform。
    /// 具体精灵由 PlayerController 从 AircraftData.BodySprite 传入。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;

        [Tooltip("这张精灵的机头指向（朝上 90 / 朝右 0）。由 PlayerController 从 AircraftData 传入")]
        [SerializeField] float visualForwardOffsetDeg = 90f;

        Sprite _appliedSprite;

        /// <summary>由 PlayerController 在 Awake 里根据 AircraftData 设定。</summary>
        public void SetVisualForward(float degrees) => visualForwardOffsetDeg = degrees;

        void Reset() => spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        /// <param name="current">当前固定步状态</param>
        /// <param name="previous">上一个固定步状态</param>
        /// <param name="alpha">插值系数 0~1</param>
        /// <param name="sprite">要显示的精灵（来自 Ship Definition，可空 = 不改）</param>
        public void Render(in FlightState current, in FlightState previous, float alpha, Sprite sprite)
        {
            // 1) 位置插值：固定 50Hz 模拟 → 平滑到显示帧率
            Vec2 position = previous.Position + (current.Position - previous.Position) * alpha;
            // 显式构造 Vector3。Core 不依赖 UnityEngine，
            // Vec2 的转换位于 Gameplay/Adapters。
            transform.position = new Vector3(position.X, position.Y, transform.position.z);

            // 2) 朝向：直接给"机头"，不是给"速度方向"
            //    （高速时两者几乎一致；失速/倒滑时能明显看出"机头朝前、身子在飘"）
            // 2) 朝向：机头角度减去"美术机头偏移"
            //    （精灵朝上 = 偏移 90°，所以 heading 90° 时旋转为 0，图保持竖直朝上）
            transform.rotation = Quaternion.Euler(0f, 0f, current.Heading * Mathf.Rad2Deg - visualForwardOffsetDeg);

            // 3) 美术：只在换了飞机时才赋值
            if (sprite != null && !ReferenceEquals(sprite, _appliedSprite) && spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                _appliedSprite = sprite;
            }
        }

        /// <summary>观战/回放时把自己隐藏（不要销毁，状态还要用）。</summary>
        public void SetVisible(bool visible)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = visible;
        }
    }
}
