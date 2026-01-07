using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.UIManager;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUi : SingletonUI<GamePlayUi>
{
    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;
    public Image GamePlayImg;

    private void Start()
    {
        homeButton.onClick.AddListener(HomeEvent);
        shopButton.onClick.AddListener(ShopEvent);
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
