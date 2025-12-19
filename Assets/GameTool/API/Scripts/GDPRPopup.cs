using GameToolSample.GameDataScripts.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameTool.APIs.Scripts
{
    public class GDPRPopup : MonoBehaviour
    {
#if !Minify
        public void OpenLink(string link)
        {
            Application.OpenURL(link);
        }

        public void ButtonGDPR(bool isAccept)
        {
            GameData.Instance.GDPRShowed = true;
            GameData.Instance.GDPR = isAccept;
            gameObject.SetActive(false);
            SceneManager.LoadScene("HomeScene");
            //API.Instance.InitAds();
        }
#endif
    }
}
