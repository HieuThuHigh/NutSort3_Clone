using GameTool.Assistants.DesignPattern;
using Lofelt.NiceVibrations;

namespace GameTool.Vibration
{
    public class VibrationManager : SingletonMonoBehaviour<VibrationManager>
    {
        private void Start()
        {
#if !Minify
             // Khởi tạo NiceVibrations
             HapticController.Init();
#endif
        }

        public void Vibrate(int millisecond)
        {
            // Thay thế bằng NiceVibrations - rung đều với thời lượng tùy chỉnh
            float duration = millisecond / 1000f; // Chuyển đổi từ millisecond sang giây
            HapticPatterns.PlayConstant(0.7f, 0.8f, duration); // amplitude: 0.7, frequency: 0.8
        }

        public void Vibrate(HapticType hapticType = HapticType.Sort)
        {
#if !Minify
             if (GameToolSample.GameDataScripts.Scripts.GameData.Instance.Vibrate)
             {
#if !UNITY_EDITOR
                 switch (hapticType)
                 {
                     case HapticType.None:
                         {
                             break;
                         }
                     case HapticType.Sort:
                         {
                             // Thay đổi thành NiceVibrations
                             HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
                             break;
                         }
                     case HapticType.Medium:
                         {
                             // Thay đổi thành NiceVibrations
                             HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
                             break;
                         }
                     case HapticType.Hard:
                         {
                             // Thay đổi thành NiceVibrations
                             HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
                             break;
                         }
                 }
#endif
             }
#endif 
         }
        }

        public enum HapticType
        {
            None,
            Sort,
            Medium,
            Hard
        }
    }