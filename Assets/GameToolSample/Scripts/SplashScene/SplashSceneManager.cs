using System.Collections;
using DG.Tweening;
using GameTool.APIs.Scripts;
using GameTool.APIs.Scripts.Ads;
using GameTool.Assistants.DesignPattern;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.FirebaseServices;
using GameToolSample.Scripts.LoadScene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameTool.Assistants
{
    public class SplashSceneManager : SingletonMonoBehaviour<SplashSceneManager>
    {
        float startTime = 0;
        bool isTimingStopped = false;
        float maxTimeWaitLoadSceneStart = 3f;
        [SerializeField] Image fillImage;
        [SerializeField] TextMeshProUGUI loadingText;
        private string textDefault = "Loading";
        private WaitForSeconds waitForHalfSeconds = new WaitForSeconds(0.5f);
        int indexText = 0;

        void Start()
        {
            StartCoroutine(LoadSceneStart());
        }

        protected override void Awake()
        {
            if (!GameData.Instance.BoughtItemIds.Contains(0))
            {
                GameData.Instance.BoughtItemIds.Add(0);
            }

            if (!GameData.Instance.BoughtItemIdsBG.Contains(0))
            {
                GameData.Instance.BoughtItemIdsBG.Add(0);
            }
            
        }

        IEnumerator LoadSceneStart()
        {
            fillImage.DOFillAmount(1, 3f).SetEase(Ease.InQuad);
            StartCoroutine(nameof(LoadingTextAnim));
            float currentTimeWaitLoadSceneStart = 0f;
            // while ((!global::GameToolSample.GameData.Scripts.GameData.allDataLoaded || !API.Scripts.API.apiStarted
            //            || !FirebaseRemote.IsFirebaseGetDataCompleted || !API.Scripts.API.adsConfigLoaded) && (currentTimeWaitLoadSceneStart < maxTimeWaitLoadSceneStart))
            // {
            //     currentTimeWaitLoadSceneStart += Time.unscaledDeltaTime;
            //     yield return null;
            // }
            yield return new WaitForSeconds(4f);
            SceneLoadManager.Instance.LoadSceneStart();
            StartCoroutine(ATT.CRRequestATTracking());
        }
        IEnumerator LoadingTextAnim()
        {
            if (indexText >= 4)
            {
                indexText = 0;
                loadingText.text = textDefault;
            }
            yield return waitForHalfSeconds;
            loadingText.text += ".";
            indexText++;
            StartCoroutine(nameof(LoadingTextAnim));
        }
    }
}