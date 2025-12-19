using System.Collections.Generic;
using GameTool.Assistants.DesignPattern;
using GameToolSample.Scripts.Enum;
using UnityEngine;

namespace GameToolSample.Scripts.Toast
{
    public class ToastMgr : SingletonMonoBehaviour<ToastMgr>
    {
        [Header("COMPONENT")]
        [SerializeField] RectTransform posSpawnToast;
        [SerializeField] ToastDisplayItemController toastDisplayItemPrefab;
        List<ToastDisplayItemController> listToastDisplayItem = new List<ToastDisplayItemController>();

        private void OnEnable()
        {
            this.RegisterListener(EventID.ShowToast, ShowToastEventRegisterListener);
        }

        private void OnDisable()
        {
            this.RemoveListener(EventID.ShowToast, ShowToastEventRegisterListener);
        }

        void ShowToastEventRegisterListener(Component component, object[] obj = null)
        {
            if (obj != null && obj.Length > 0)
            {
                string[] listString = new string[obj.Length];

                for (int i = 0; i < obj.Length; i++)
                {
                    listString[i] = (string)obj[i];
                }

                Show(listString);          
            }
        }

        public void Show(params string[] mess)
        {
            ToastDisplayItemController item = GetToastDisplayItem();
            item.gameObject.SetActive(true);
            item.ShowToast(mess);
        }

        ToastDisplayItemController GetToastDisplayItem()
        {
            for(int i = 0; i < listToastDisplayItem.Count; i++)
            {
                if (!listToastDisplayItem[i].gameObject.activeInHierarchy)
                {
                    return listToastDisplayItem[i];
                }
            }

            ToastDisplayItemController control = Instantiate(toastDisplayItemPrefab, posSpawnToast);
            listToastDisplayItem.Add(control);
            return control;
        }
    }
}
