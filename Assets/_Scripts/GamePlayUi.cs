using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.UIManager;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUi : SingletonUI<GamePlayUi>
{
    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;
    public Button vipBtn;
    public Image GamePlayImg;
    [SerializeField] private Sprite[] backgroundSpritesGamePlay;
    private void Start()
    {
        homeButton.onClick.AddListener(HomeEvent);
        shopButton.onClick.AddListener(ShopEvent);
        vipBtn.onClick.AddListener(VipEvent);
    }
    
    private void OnEnable()
    {
        CheckBackGroudGamePlay();
    }

    private void VipEvent()
    {
        CanvasManager.Instance.Push(eUIName.VipPopup);
    }

    public void CheckBackGroudGamePlay()
    {
        int selectedBgId = GameData.Instance.SelectedShopBgID;

        if (GameData.Instance.BoughtItemIdsBG.Contains(selectedBgId))
        {
            if (selectedBgId < backgroundSpritesGamePlay.Length)
            {
                GamePlayImg.sprite = backgroundSpritesGamePlay[selectedBgId];
            }
        }
        else
        {
            GamePlayImg.sprite = backgroundSpritesGamePlay[0];
        }
    }


    private void ShopEvent()
    {
        CanvasManager.Instance.Push(eUIName.ShopPopup);
    }

    private void HomeEvent()
    {
        CanvasManager.Instance.Push(eUIName.Home);
    }
}