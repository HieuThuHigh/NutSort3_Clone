using System;
using DatdevUlts;
using DatdevUlts.Ults;
using GameTool.APIs.Scripts;
using GameTool.APIs.Scripts.Ads;
using UnityEngine;

namespace GameTool.APIs
{
    public class SafeAreaBanner : MonoBehaviour
    {
        [SerializeField] private bool _enable = true;
        private bool _preCanShow = true;
        
        private void OnEnable()
        {
            AdjustSafe();
        }

        private void AdjustSafe()
        {
            var rectTransform = transform.RectTransform();

            if (_enable && API.Instance.CanShowBanner)
            {
                ScreenDpUtils.SetSafeAreaAnchorsWithBanner(rectTransform);
            }
            else
            {
                ScreenDpUtils.SetSafeAreaAnchorsWithBanner(rectTransform, false);
            }
        }

        private void Update()
        {
            if (_preCanShow != API.Instance.CanShowBanner)
            {
                _preCanShow = API.Instance.CanShowBanner;
                AdjustSafe();
            }
        }
    }
}