
using System;
using System.Collections;
using DatdevUlts.Ults;
using UnityEngine;
using UnityEngine.UI;

namespace GameTool.APIs.Scripts.Ads
{
    public class AdsFake : MonoBehaviour
    {
    #if !Minify
        [SerializeField] Text titleAds;
        [SerializeField] Text timeText;
        [SerializeField] Button closeButton;
        Action<bool> closeAction;
        int timeToClose = 5;
        private void Start()
        {
            if (closeButton)
            {
                closeButton.onClick.AddListener(CloseButton);
            }
        }
        public void Init(string title, Action<bool> onClosed, int waitingTime)
        {
            transform.SetAsLastSibling();
            closeAction = onClosed;
            titleAds.text = title;
            if (!gameObject.name.Contains("Banner"))
            {
                AudioListener.volume = 0;
                TimeManager.Instance.Add(0);
            }
            if (waitingTime > 0)
            {
                timeToClose = waitingTime;

                if (closeButton)
                    closeButton.gameObject.SetActive(false);
                if (timeText)
                    timeText.fontSize = 150;
                StartCoroutine(nameof(TimeCountDown));
            }
            else
            {
                if (closeButton)
                    closeButton.gameObject.SetActive(true);
                if (timeText)
                {
                    timeText.fontSize = 50;
                    timeText.text = "SUCCESSFULLY";
                }
            }
        }
        IEnumerator TimeCountDown()
        {
            if (timeText)
                timeText.text = timeToClose.ToString();

            yield return new WaitForSecondsRealtime(1);

            timeToClose--;
            if (timeToClose <= 0)
            {
                closeButton.gameObject.SetActive(true);
                if (timeText)
                {
                    timeText.fontSize = 50;
                    timeText.text = "SUCCESSFULLY";
                }
            }
            else
            {
                StartCoroutine(nameof(TimeCountDown));
            }

        }
        void CloseButton()
        {
            API.Instance.ActiveLoading(false);
            AudioListener.volume = 1;
            gameObject.SetActive(false);
            closeAction(true);
            TimeManager.Instance.Remove(0);
        }
#endif
    }
}
