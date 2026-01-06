using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameConfigScripts;
using GameToolSample.UIManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeMenu : BaseUI
{
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button shopBtn;
    [SerializeField] private Button playBtn;

    void Start()
    {
        settingBtn.onClick.AddListener(SettingEvent);
        shopBtn.onClick.AddListener(ShopEvent);
        playBtn.onClick.AddListener(PlayEvent);
    }

    private void OnEnable()
    {
        GameConfig.Instance.IsHome = true;
    }

    private void OnDisable()
    {
        GameConfig.Instance.IsHome = false;
    }

    private void ShopEvent()
    {
        CanvasManager.Instance.Push(eUIName.ShopPopup);
    }

    private void SettingEvent()
    {
        CanvasManager.Instance.Push(eUIName.SettingPopup);
    }

    private void PlayEvent()
    {
        CanvasManager.Instance.Push(eUIName.GamePlayUI);
    }
}