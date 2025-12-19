using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.UIManager;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUi : BaseUI
{
    [SerializeField] private Button homeButton;

    private void Start()
    {
        homeButton.onClick.AddListener(HomeEvent);
    }

    private void HomeEvent()
    {
        CanvasManager.Instance.Push(eUIName.Home);
    }
}
