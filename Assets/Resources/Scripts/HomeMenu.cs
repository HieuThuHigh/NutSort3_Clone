using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.Assistants.DesignPattern;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameConfigScripts;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using GameToolSample.UIManager;
using TMPro;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeMenu : BaseUI
{
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button shopBtn;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button dailyBtn;
    [SerializeField] private TextMeshProUGUI currentLevelTxt;
    [SerializeField] private TextMeshProUGUI[] levelTxtList;

    void Start()
    {
        settingBtn.onClick.AddListener(SettingEvent);
        shopBtn.onClick.AddListener(ShopEvent);
        playBtn.onClick.AddListener(PlayEvent);
        dailyBtn.onClick.AddListener(DailyEvent);
        currentLevelTxt.text = GameData.Instance.CurrentLevel.ToString();
        SetUpLevelText();
        this.RegisterListener(EventID.UpdateData, OnUpdateData);
    }


    void UpdateLevelDisplay()
    {
        currentLevelTxt.text = GameData.Instance.CurrentLevel.ToString();
        SetUpLevelText();
    }
    private void OnUpdateData(Component sender, object[] param)
    {
        UpdateLevelDisplay();
    }

    private void OnDestroy()
    {
        this.RemoveListener(EventID.UpdateData, OnUpdateData);
    }
    private void TangEvent()
    {
        GameData.Instance.CurrentLevel += 1;
        this.PostEvent(EventID.UpdateData);
    }

    void SetUpLevelText()
    {
        int currentLevel = GameData.Instance.CurrentLevel;

        for (int i = 0; i < levelTxtList.Length; i++)
        {
            levelTxtList[i].text = (currentLevel + i + 1).ToString();
        }
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
        Pop();
        CanvasManager.Instance.Push(eUIName.GamePlayUI);
    }
    private void DailyEvent()
    {
        CanvasManager.Instance.Push(eUIName.DailyGoalsPopup);
    }

}