using DogfightThreatAI.Core.Input;
using UnityEngine;

namespace DogfightThreatAI.Gameplay.InputSource
{
    /// <summary>
    /// 键盘输入源。将按键状态转换为 FlightInput，不包含飞行逻辑。
    /// </summary>
    public sealed class KeyboardInputSource : MonoBehaviour, IPlayerInputSource
    {
        [Tooltip("可选：WASD（默认）或 方向键")]
        [SerializeField] bool useArrowKeys;

        public FlightInput ReadInput(float deltaTime)
        {
            float throttle, steer;

            if (useArrowKeys)
            {
                throttle = Input.GetAxisRaw("Vertical");
                steer    = -Input.GetAxisRaw("Horizontal");
            }
            else
            {
                // 不依赖 InputManager 的轴名，避免"轴不存在"的坑
                throttle = (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f);
                steer    = (Input.GetKey(KeyCode.A) ? 1f : 0f) + (Input.GetKey(KeyCode.D) ? -1f : 0f);
            }

            return new FlightInput(throttle, steer);
        }
    }
}
