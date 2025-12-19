
using System;
using GameTool.Assistants.DesignPattern;
using UnityEngine;

namespace GameTool.APIs.Scripts
{
    public class ATTPopup : SingletonMonoBehaviour<ATTPopup>
    {
#if !Minify
        [SerializeField] GameObject attPreObject;
        Action nextAction;
        public static bool ShowedATT
        {
            get { return PlayerPrefs.GetInt("ShowedATT", 0) == 1; }
            set { PlayerPrefs.SetInt("ShowedATT", value ? 1 : 0); }
        }
        public void NextClick()
        {
            attPreObject.gameObject.SetActive(false);
            nextAction?.Invoke();

        }
        public bool CheckATTReadly(Action callBack = null)
        {
            if (ATT.IsOSReady() && !ShowedATT)
            {
                nextAction += callBack;
                attPreObject.gameObject.SetActive(true);

                return true;
            }
            else
                return false;
        }
#endif
    }
}
