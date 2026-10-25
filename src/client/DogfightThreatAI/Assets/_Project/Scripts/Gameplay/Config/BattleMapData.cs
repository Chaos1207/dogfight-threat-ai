using UnityEngine;

namespace DogfightThreatAI.Gameplay.Config
{
    /// <summary>地图内容资产。布局按从左至右、从上至下存储，每字节引用一个地形精灵。</summary>
    [CreateAssetMenu(menuName = "DogfightThreatAI/Battle Map", fileName = "Map_New")]
    public sealed class BattleMapData : ScriptableObject
    {
        [SerializeField] string displayName;
        [SerializeField, Min(1)] int width = 800;
        [SerializeField, Min(1)] int height = 600;
        [SerializeField, Min(.01f)] float tileWorldSize = .5f;
        [SerializeField] TextAsset layout;
        [SerializeField] Sprite[] palette;

        public string DisplayName => displayName;
        public int Width => width;
        public int Height => height;
        public float TileWorldSize => tileWorldSize;
        public Vector2 WorldSize => new Vector2(width, height) * tileWorldSize;
        public TextAsset Layout => layout;
        public Sprite[] Palette => palette;

        public bool IsValid => width > 0 && height > 0 && tileWorldSize > 0f &&
            layout != null && layout.bytes.Length == (long)width * height &&
            palette != null && palette.Length > 0 && palette.Length <= 256;
    }
}
