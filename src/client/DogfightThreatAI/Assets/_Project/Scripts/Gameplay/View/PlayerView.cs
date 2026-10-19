using DogfightThreatAI.Core.Math;
using DogfightThreatAI.Gameplay.Flight;
using UnityEngine;

namespace DogfightThreatAI.Gameplay
{
    /// <summary>
    /// 【表现层 / View】只干两件事：把机头画出来、把机身在两个模拟帧之间插值平滑。
    ///
    /// ★ 它不认识 FlightModel，也不算任何飞行数学。
    ///   它拿到的是一份"已经算好的状态"，只负责显示。
    ///   —— 这就是 Simulate / View 分离。以后做观战、做回放，都是换一个状态来源喂给它。
    ///
    /// ⚠️ 关于美术：这个类**不引用任何具体图片**。图从哪来？
    ///   由 PlayerController 把 AircraftData.BodySprite 传进 Render() 的第四个参数。
    ///   所以换美术、做选角色界面，都不用改这个文件。
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
            // ⚠️ 必须显式构造 Vector3：C# 不允许"用户自定义转换 + 再一层转换"自动叠加，
            //    Vec2→Vector2 是我们写的隐式转换，Vector2→Vector3 是 Unity 写的，两者不能串联。
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
