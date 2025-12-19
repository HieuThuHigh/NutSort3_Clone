using System.Collections;
using GameTool.Assistants.DesignPattern;
using GameToolSample.Scripts.FirebaseServices;
using UnityEngine;

namespace GameTool.APIs.Scripts
{
    public class NewVersionChecker : SingletonMonoBehaviour<NewVersionChecker>
    {
        [Header("COMPONENT")]
        [SerializeField] GameObject newVersionUpdatePopup;

        private void Start()
        {
            StartCoroutine(nameof(WaitCheckVersion));
        }

        public void CheckNewVersion()
        {
            if (API.NewVersionAvailable())
            {
                newVersionUpdatePopup.SetActive(true);
            }
        }

        IEnumerator WaitCheckVersion()
        {
            yield return new WaitUntil(() => FirebaseRemote.IsFirebaseGetDataCompleted);
            CheckNewVersion();
        }
    }
}
