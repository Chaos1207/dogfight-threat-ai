using UnityEngine;

namespace DogfightThreatAI.Core.Config
{
    /// <summary>
    /// 【数据层】一架飞机的"档案"。
    ///
    /// 这个类是"先设计数据、再搭系统"的落点：飞机的一切可调项都在这里，
    /// 系统（FlightModel / PlayerController / PlayerView）只负责读它。
    ///
    /// 【为什么它同时是"选角色界面的一条记录"】
    /// 它同时承担两件事：显示名 + 美术 + 手感数值。
    /// 加一架新飞机 = 新建一个 .asset + 拖两张图，不改任何代码。
    ///
    /// ⚠️ 纪律：这里只放【配置】（不会在运行时改变的值）。
    ///    当前血量、冷却、位置这类【状态】绝不能放进来——ScriptableObject 是全工程共享的一份，
    ///    写进去会导致"所有同类飞机一起掉血"。
    /// </summary>
    [CreateAssetMenu(menuName = "DogfightThreatAI/Ship Definition", fileName = "Ship_New")]
    public sealed class AircraftData : ScriptableObject
    {
        // ────────── 显示信息（选角色界面用） ──────────
        [Header("显示")]
        [Tooltip("选角色界面显示的名字")]
        [SerializeField] string displayName = "未命名机型";
        [TextArea(2, 4)]
        [SerializeField] string description = "";

        // ────────── 美术引用（换美术只改这里） ──────────
        [Header("美术")]
        [Tooltip("机身精灵。注意：Sprite 的 Pivot 必须设为 Center，否则旋转时会绕偏")]
        [SerializeField] Sprite bodySprite;
        [Tooltip("选角色界面用的小图。留空则用 bodySprite")]
        [SerializeField] Sprite icon;
        [Tooltip("尾焰/推进器效果。暂时可留空")]
        [SerializeField] GameObject thrusterPrefab;

        // ────────── 飞行参数（手感只调这里） ──────────
        [Header("速度（单位/秒）")]
        [Tooltip("失速下界：低于它飞机飞不快（以后可以做失速下坠）")]
        [SerializeField] float minSpeed = 2f;
        [SerializeField] float maxSpeed = 8f;
        [Tooltip("速度向目标逼近的速率，越大越灵敏、越小越'重'")]
        [SerializeField] float acceleration = 12f;

        [Header("转向")]
        [Tooltip("最大转向速率（度/秒）。不能瞬间掉头就是靠这个")]
        [SerializeField] float turnRateDegPerSec = 180f;

        [Header("惯性")]
        [Tooltip("速度阻尼。0 = 无阻尼；越大越'粘'")]
        [SerializeField] float drag = 0f;

        [Header("漂移（横向抓地力）")]
        [Tooltip("横向抓地力（1/秒）：越大越贴地、像钉在轨道上；越小甩尾越厉害。\n" +
                 "20 ≈ 几乎不甩（转圈飞）；6 ≈ 中等漂移；2~3 ≈ 赛车式大甩尾")]
        [SerializeField] float lateralGrip = 6f;

        [Tooltip("漂移掉速系数（0~1）：侧滑越厉害，前进极速越低。0 = 漂移不掉速")]
        [SerializeField, Range(0f, 1f)] float driftScrub = 0.35f;

        [Header("美术朝向")]
        [Tooltip("这张精灵的机头指向：朝上填 90，朝右填 0，朝下填 -90。\n" +
                 "用来让「视觉机头」和「物理机头」对齐。Kenney 的舰船图是朝上的，所以默认 90。")]
        [SerializeField] float visualForwardOffsetDeg = 90f;

        // ────────── 只读访问器 ──────────
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite BodySprite  => bodySprite != null ? bodySprite : icon;
        public Sprite Icon        => icon != null ? icon : bodySprite;
        public GameObject ThrusterPrefab => thrusterPrefab;

        public float MinSpeed            => minSpeed;
        public float MaxSpeed            => maxSpeed;
        public float Acceleration        => acceleration;
        public float TurnRateDegPerSec   => turnRateDegPerSec;
        public float Drag                => drag;
        public float LateralGrip         => lateralGrip;
        public float DriftScrub          => driftScrub;
        public float VisualForwardOffsetDeg => visualForwardOffsetDeg;

        void OnValidate()
        {
            if (maxSpeed < minSpeed) maxSpeed = minSpeed;
            if (minSpeed < 0f)       minSpeed = 0f;
            if (acceleration < 0f)   acceleration = 0f;
            if (turnRateDegPerSec < 0f) turnRateDegPerSec = 0f;
            if (drag < 0f)           drag = 0f;
            if (lateralGrip < 0f)    lateralGrip = 0f;
            driftScrub = Mathf.Clamp01(driftScrub);
        }
    }
}
