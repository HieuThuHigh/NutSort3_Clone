using System;
using DatdevUlts.Ults;
using GameTool.APIs.Scripts;
using GameTool.Assistants.DesignPattern;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.LoadScene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameToolSample._menuhack
{
    public class MenuHack : MonoBehaviour
    {
        [SerializeField] private Button _btnOpenMenu;
        [SerializeField] private Button _btnHide;
        [SerializeField] private GameObject Content;
        [SerializeField] private TMP_InputField _inputCoin;
        [SerializeField] private Button _btnSetCoin;
        [SerializeField] private TMP_InputField _inputLevel;
        [SerializeField] private Button _btnLoadLevel;
        [SerializeField] private Toggle _toggleHideUI;

        private bool pauseUpdate;
        
        private void Awake()
        {
            _btnOpenMenu.onClick.AddListener(() =>
            {
                Content.gameObject.SetActive(!Content.gameObject.activeSelf);
            });
            
            _btnHide.onClick.AddListener(() =>
            {
                Content.gameObject.SetActive(!Content.gameObject.activeSelf);
            });
            
            _btnSetCoin.onClick.AddListener(() =>
            {
                GameData.Instance.Data.Coin = Convert.ToInt32(_inputCoin.text);
                GameData.Instance.CoinFake = Convert.ToInt32(_inputCoin.text);
                this.PostEvent(EventID.UpdateData);
            });
            
            _btnLoadLevel.onClick.AddListener(() =>
            {
                GameData.Instance.CurrentLevel = Convert.ToInt32(_inputLevel.text);
                SceneLoadManager.Instance.LoadSceneGamePlay();
            });

            API.Instance.IsRemoveAdsFull = true;
        }

        private void Update()
        {
            if (pauseUpdate)
            {
                return;
            }
            
            if (_toggleHideUI.isOn)
            {
                if (CanvasManager.IsInstanceValid() && CanvasManager.Instance.gameObject.activeSelf)
                {
                    pauseUpdate = true;
                    this.DelayedCall(0.3f, () =>
                    {
                        pauseUpdate = false;
                        CanvasManager.Instance.gameObject.SetActive(false);
                        var uis = FindObjectsOfType<HideUIHack>();
                        foreach (var ui in uis)
                        {
                            ui.Hide();
                        }
                    });
                }
            }
            else
            {
                if (CanvasManager.IsInstanceValid() && !CanvasManager.Instance.gameObject.activeSelf)
                {
                    pauseUpdate = true;
                    this.DelayedCall(0.3f, () =>
                    {
                        pauseUpdate = false;
                        CanvasManager.Instance.gameObject.SetActive(true);
                        var uis = FindObjectsOfType<HideUIHack>();
                        foreach (var ui in uis)
                        {
                            ui.Show();
                        }
                    });
                }
            }
        }
    }
}
