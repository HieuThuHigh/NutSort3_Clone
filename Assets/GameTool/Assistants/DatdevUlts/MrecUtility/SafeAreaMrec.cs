using DatdevUlts.Ults;
using GameTool.APIs.Scripts;
using GameTool.APIs.Scripts.Ads;
using UnityEngine;

namespace DatdevUlts
{
    public class SafeAreaMrec : MonoBehaviour
    {
        [SerializeField] private bool _enable = true;
        [SerializeField] private float _safeTopInPixels;
        private void OnEnable()
        {
            var rectTransform = transform.RectTransform();
            
            if (_enable && API.Instance.CanShowMrec)
            {
                ScreenDpUtils.SetSafeAreaAnchorsWithMrec(rectTransform);
                var max = rectTransform.offsetMax;
                max.y = -_safeTopInPixels;
                rectTransform.offsetMax = max;
            }
            else
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
            
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
        }
    }
}