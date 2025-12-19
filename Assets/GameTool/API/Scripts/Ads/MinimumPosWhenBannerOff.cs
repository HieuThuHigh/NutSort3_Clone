using DatdevUlts.Ults;
using UnityEngine;

namespace GameTool.APIs.Scripts.Ads
{
    public class MinimumPosWhenBannerOff : MonoBehaviour
    {
        [SerializeField] private float _minimumPosY;
        private bool _preCanShow = true;
        
        private void OnEnable()
        {
            AdjustSafe();
        }
        
        private void Update()
        {
            if (_preCanShow != API.Instance.CanShowBanner)
            {
                _preCanShow = API.Instance.CanShowBanner;
                AdjustSafe();
            }
        }

        public void AdjustSafe()
        {
            if (!API.Instance.CanShowBanner)
            {
                var vector2 = transform.RectTransform().anchoredPosition;
                vector2.y = _minimumPosY;
                transform.RectTransform().anchoredPosition = vector2;
            }
        }
    }
}