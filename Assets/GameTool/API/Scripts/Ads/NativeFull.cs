using System;
using System.Collections;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.FirebaseServices;
using GameToolSample.UIManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameTool.APIs.Scripts.Ads
{
    public class NativeFull : MonoBehaviour
    {
        public static void ShowNativeFull(AnalyticID.KeyAds key, Action callback)
        {
            var popup = CanvasManager.Instance.Push(eUIName.NativeFull);
            popup.GetComponent<NativeFull>().Show(key, callback);
        }
        
        [SerializeField] private TextMeshProUGUI _txtTime;
        [SerializeField] private Image _imgClose;
        [SerializeField] private Button _btnClose;
        [SerializeField] private NativeAdsItem _item;
        private int _currentTime;
        private bool _canClose;
        private Action _action;

        private void Awake()
        {
            _btnClose.onClick.AddListener(() =>
            {
                if (_currentTime <= 0 && _canClose)
                {
                    Close();
                }
            });
        }

        private void OnEnable()
        {
            _txtTime.gameObject.SetActive(true);
            _imgClose.gameObject.SetActive(false);
            _canClose = false;
            TimeManager.Instance.Add(0);

            StartCoroutine(nameof(CheckClose));
        }

        public void Show(AnalyticID.KeyAds key, Action action = null)
        {
            _item.Key = key;
            
            _canClose = true;
            _action = action;

            if (!API.Instance.CanShowNative)
            {
                Close();
                return;
            }

            Close();
        }

        private IEnumerator CheckClose()
        {
            _currentTime = 3;

#if UNITY_EDITOR
            _currentTime = 1;
#endif

            var time = _currentTime;

            for (int i = 0; i < time; i++)
            {
                _txtTime.text = _currentTime.ToString();
                yield return new WaitForSecondsRealtime(1f);
                _currentTime--;
            }

            if (!_canClose)
            {
                Debug.LogError("DETECT SHOW() IS NOT CALLED");
            }

            _txtTime.gameObject.SetActive(false);
            _imgClose.gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            _action?.Invoke();
        }

        private void OnDisable()
        {
            TimeManager.Instance.Remove(0);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _canClose)
            {
                Close();
            }
        }
    }
}