using GameTool.APIs.Analytics.Analytics;
using GameToolSample.Scripts.Enum;
using UnityEngine;
using UnityEngine.UI;

namespace GameTool.APIs.Analytics
{
    public class ScreenAnalytics : MonoBehaviour
    {
    #if !Minify
        // Start is called before the first frame update
        [SerializeField] AnalyticID.ScreenID screenID;
        [SerializeField] AnalyticID.ButtonID buttonID;

        [Header("Require Button For Button Tracking")]
        [SerializeField]
        TrackingType trackingType = TrackingType.Button;

        private void Start()
        {
            if (trackingType == TrackingType.Button)
            {
                if (TryGetComponent(out Button button))
                {
                    button.onClick.AddListener(() =>
                    {
                        TrackingManager.Instance.TrackButtonGameScreen(screenID, buttonID);
                    });
                }

                if (TryGetComponent(out Toggle toggle))
                {
                    toggle.onValueChanged.AddListener(delegate
                    {
                        TrackingManager.Instance.TrackButtonGameScreen(screenID, buttonID);
                    });
                }
            }
        }

        private void OnEnable()
        {
            if (trackingType == TrackingType.Screen)
            {
                TrackingManager.Instance.TrackButtonGameScreen(screenID, AnalyticID.ButtonID.open);
            }
        }

        private void OnDisable()
        {
            if (trackingType == TrackingType.Screen)
            {
                TrackingManager.Instance.TrackButtonGameScreen(screenID, AnalyticID.ButtonID.close);
            }
        }

        enum TrackingType
        {
            Screen,
            Button
        }
    #endif
    }
}