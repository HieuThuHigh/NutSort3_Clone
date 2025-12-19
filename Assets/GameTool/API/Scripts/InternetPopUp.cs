using UnityEngine;
using UnityEngine.UI;


namespace GameTool.APIs.Scripts
{
    public class InternetPopUp : MonoBehaviour
    {
#if !Minify
        [SerializeField] private Button retryButton;

        private void Start()
        {
            retryButton.onClick.AddListener(RetryClick);
        }

        private void OnEnable()
        {
            TimeManager.Instance.Add(0);
        }

        private void RetryClick()
        {
            if (!InternetChecker.Instance.HasTurnOffInternet())
            {
                TimeManager.Instance.Remove(0);
                gameObject.SetActive(false);
            }
        }

#endif
    }
}