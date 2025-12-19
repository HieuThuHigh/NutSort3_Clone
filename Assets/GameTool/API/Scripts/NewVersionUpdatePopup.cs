using GameToolSample.Scripts.FirebaseServices;
using UnityEngine;
using UnityEngine.UI;

namespace GameTool.APIs.Scripts
{
    public class NewVersionUpdatePopup : MonoBehaviour
    {
        [Header("COMPONENT")]
        [SerializeField] Button updateBtn;
        [SerializeField] Button laterBtn;
        bool forceUpdate;

        // Start is called before the first frame update
        void Start()
        {
            forceUpdate = FirebaseRemote.Instance.GetStoreConfig().ForceUpdate;

            updateBtn.onClick.AddListener(UpdateBtnClick);
            laterBtn.onClick.AddListener(LaterBtnClick);

            laterBtn.gameObject.SetActive(!forceUpdate);
        }

        private void OnEnable()
        {
            TimeManager.Instance.Add(0);
        }

        private void OnDisable()
        {
            TimeManager.Instance.Remove(0);
        }

        void UpdateBtnClick()
        {
            Application.OpenURL(API.StoreLinkGame());
        
            if (!forceUpdate)
            {
                gameObject.SetActive(false);
            }
        }

        void LaterBtnClick()
        {
            gameObject.SetActive(false);
        }
    }
}
