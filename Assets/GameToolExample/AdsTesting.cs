using System.Collections;
using DatdevUlts.Ults;
using GameTool.APIs.Scripts;
using GameTool.APIs.Scripts.Ads;
using GameTool.Assistants.DesignPattern;
using GameToolSample.Scripts.Enum;
using UnityEngine;
using UnityEngine.UI;

namespace GameToolExample
{
    public class AdsTesting : MonoBehaviour
    {
        [SerializeField] private Toggle _toggleNoAds;
        [SerializeField] private Toggle _toggleNoAdsFull;
        [SerializeField] private Toggle _toggleTestBackfill;
        [SerializeField] private GameObject _nativeAds;

        private void Awake()
        {
            _toggleNoAds.isOn = API.Instance.IsRemoveAds;
            _toggleNoAdsFull.isOn = API.Instance.IsRemoveAdsFull;
            _toggleTestBackfill.isOn = API.Instance.IsTestBackfill;

            _toggleNoAds.onValueChanged.AddListener(value =>
            {
                API.Instance.IsRemoveAds = value;
                if (value)
                {
                    this.PostEvent(EventID.BuyRemoveAds);
                }
            });

            _toggleNoAdsFull.onValueChanged.AddListener(value =>
            {
                API.Instance.IsRemoveAdsFull = value;
                if (value)
                {
                    this.PostEvent(EventID.BuyRemoveAds);
                }
            });

            _toggleTestBackfill.onValueChanged.AddListener(value => { API.Instance.IsTestBackfill = value; });

            _nativeAds.gameObject.SetActive(false);
            InvokeRepeating(nameof(ShowNative), 2f, 10f);
        }

        public void ShowNative()
        {
            _nativeAds.gameObject.SetActive(true);
        }

        public void ShowReward()
        {
            API.Instance.ShowReward(success => { Debug.Log("Adstesting: Success: " + success); },
                AnalyticID.LocationTracking.none, AnalyticID.KeyAds.none);
        }

        public void ShowInter()
        {
            API.Instance.ShowFull(success => { Debug.Log("Adstesting: Success: " + success); },
                AnalyticID.LocationTracking.none, AnalyticID.KeyAds.none);
        }

        public void ShowBanner()
        {
            API.Instance.ShowBanner(AnalyticID.KeyAds.none);
        }

        public void ShowCollBanner()
        {
            API.Instance.ShowBanner(normalBanner: false, key: AnalyticID.KeyAds.none);
        }

        public void HideBanner()
        {
            API.Instance.HideBanner();
        }
    }
}