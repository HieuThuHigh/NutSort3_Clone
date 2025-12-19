using System.Collections.Generic;
using GameToolSample.Scripts.Enum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if USE_ADMOB_NATIVE_ADS
using System.Collections;
using System.Collections.Generic;
using _ProjectTemplate.Scripts.Ads;
using GameTool.Assistants.DesignPattern;
using GameToolSample.Scripts.Enum;
using GoogleMobileAds.Api;
using UnityEditor;
#endif

namespace GameTool.APIs.Scripts.Ads
{
    public class NativeAdsItem : MonoBehaviour
    {
        [SerializeField] AnalyticID.KeyAds key;
        [SerializeField] GameObject adNativePanel;
        [SerializeField] List<GameObject> listOffWithPanel = new List<GameObject>();
        [SerializeField] RawImage adIcon;
        [SerializeField] RawImage adImageContent;
        [SerializeField] RawImage adChoices;
        [SerializeField] Text adHeadline;
        [SerializeField] Text adCallToAction;
        [SerializeField] Text adAdvertiser;
        [SerializeField] TextMeshProUGUI tmpAdHeadline;
        [SerializeField] TextMeshProUGUI tmpAdCallToAction;
        [SerializeField] TextMeshProUGUI tmpAdAdvertiser;
        [SerializeField] private bool loadOnEnable = true;

        public AnalyticID.KeyAds Key
        {
            get => key;
            set => key = value;
        }
        
#if USE_ADMOB_NATIVE_ADS

        private NativeAd adNative;


        public NativeAd ADNative
        {
            get => adNative;
            set
            {
                adNative = value;
                if (adNative != null)
                {
                    loadOnEnable = false;
                }
            }
        }
#endif


#if USE_ADMOB_NATIVE_ADS
        void OnEnable()
        {
            API.Instance.HideBanner();
            
            this.RegisterListener(EventID.BuyRemoveAds, BuyRemoveAdsEventRegisterListener);

            adNativePanel.SetActive(false);
            foreach (var o in listOffWithPanel)
            {
                o.SetActive(false);
            }

            CheckShowNativeAds();
            Debug.Log("Native ad item enable.");
        }

        private void OnDisable()
        {
            loadOnEnable = true;
            this.RemoveListener(EventID.BuyRemoveAds, BuyRemoveAdsEventRegisterListener);

            StopCoroutine(nameof(SetNativeAds));
        }

        void BuyRemoveAdsEventRegisterListener(Component component, object[] obj = null)
        {
            CheckShowNativeAds();
        }

        IEnumerator SetNativeAds()
        {
            if (!API.Instance.CanShowNative)
            {
                if (!GetComponent<NativeFull>())
                {
                    gameObject.SetActive(false);
                }

                yield break;
            }

            yield return null;

            if (loadOnEnable || adNative == null)
            {
                NativeAdsAdmod.Instance.RequestNativeAd(key);

                yield return new WaitUntil(() => NativeAdsAdmod.Instance.nativeLoaded[key]);

                adNative = NativeAdsAdmod.Instance.adNative[key];
                SetNativeAdsContent();

                NativeAdsAdmod.Instance.RequestNativeAd(key);
            }
            else
            {
                var loaded = NativeAdsAdmod.Instance.nativeLoaded;
                SetNativeAdsContent();
                NativeAdsAdmod.Instance.nativeLoaded = loaded;
            }
        }

        void CheckShowNativeAds()
        {
            StopCoroutine(nameof(SetNativeAds));

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(nameof(SetNativeAds));
            }
        }

        void SetNativeAdsContent()
        {
            Debug.Log("Native ad item nativeLoaded.");

            if (adNativePanel)
            {
                adNativePanel.SetActive(true); //show ad panel
            }

            foreach (var o in listOffWithPanel)
            {
                o.SetActive(true);
            }

            NativeAdsAdmod.Instance.nativeLoaded[key] = false;
            Texture2D iconTexture = adNative.GetIconTexture();
            Texture2D iconAdChoices = adNative.GetAdChoicesLogoTexture();
            string headline = adNative.GetHeadlineText();
            string cta = adNative.GetCallToActionText();
            string advertiser = adNative.GetAdvertiserText();
            adIcon.texture = iconTexture;
            adChoices.texture = iconAdChoices;

            if (adHeadline)
            {
                adHeadline.text = headline;
            }
            else if (tmpAdHeadline)
            {
                tmpAdHeadline.text = headline;
            }

            if (adAdvertiser)
            {
                adAdvertiser.text = advertiser;
            }
            else if (tmpAdAdvertiser)
            {
                tmpAdAdvertiser.text = advertiser;
            }

            if (adCallToAction)
            {
                adCallToAction.text = cta;
            }
            else if (tmpAdCallToAction)
            {
                tmpAdCallToAction.text = cta;
            }

            if (adNative.GetImageTextures().Count > 0)
            {
                List<Texture2D> goList = adNative.GetImageTextures();
                adImageContent.texture = goList[0];
                List<GameObject> list = new List<GameObject>();
                list.Add(adImageContent.gameObject);
                adNative.RegisterImageGameObjects(list);
            }

            //register gameobjects
            adNative.RegisterIconImageGameObject(adIcon.gameObject);
            adNative.RegisterAdChoicesLogoGameObject(adChoices.gameObject);
            adNative.RegisterHeadlineTextGameObject(adHeadline.gameObject);
            adNative.RegisterCallToActionGameObject(adCallToAction.gameObject);
            adNative.RegisterAdvertiserTextGameObject(adAdvertiser.gameObject);
        }

        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private RectTransform _focusBox;
        [SerializeField] private bool _autoAdjustBoxCollider = true;

        private void OnDrawGizmos()
        {
            AdjustBoxCollider();
        }

        private void AdjustBoxCollider()
        {
            if (!_autoAdjustBoxCollider)
            {
                return;
            }

            if (_boxCollider && _focusBox)
            {
                if (_boxCollider.size != new Vector3(_focusBox.rect.width, _focusBox.rect.height, 10))
                {
                    _boxCollider.size = new Vector3(_focusBox.rect.width, _focusBox.rect.height, 10);
                    SetDirty();
                }

                if (_boxCollider.center != (Vector3)_focusBox.rect.center)
                {
                    _boxCollider.center = _focusBox.rect.center;
                    SetDirty();
                }
            }
        }

        private void SetDirty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }

        private void Update()
        {
            AdjustBoxCollider();
        }
#endif
    }
}