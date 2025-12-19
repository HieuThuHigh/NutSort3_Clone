using GameTool.Assistants.DesignPattern;
using UnityEngine;

namespace GameTool.APIs.Scripts
{
    public class InternetChecker : SingletonMonoBehaviour<InternetChecker>
    {
        [SerializeField] private bool _check = true;

        [SerializeField] private GameObject noInternetPopup;

        private void Start()
        {
            if (HasTurnOffInternet())
            {
                noInternetPopup.gameObject.SetActive(true);
            }
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus)
            {
                if (HasTurnOffInternet())
                {
                    noInternetPopup.gameObject.SetActive(true);
                }
            }
        }

        public bool HasTurnOffInternet()
        {
            if (!_check)
            {
                return false;
            }

            return SHasTurnOffInternet();
        }

        public static bool SHasTurnOffInternet()
        {
#if UNITY_EDITOR
            return API.Instance.IsNotNetwork;
#else
            return Application.internetReachability == NetworkReachability.NotReachable;
#endif
            
        }
    }
}