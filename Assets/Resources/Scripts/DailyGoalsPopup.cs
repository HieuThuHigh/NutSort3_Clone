using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameConfigScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyGoalsPopup : BaseUI
{
    
    [SerializeField] private TextMeshProUGUI targetTaskTxt1;
    [SerializeField] private TextMeshProUGUI targetTaskTxt2;
    [SerializeField] private TextMeshProUGUI targetTaskTxt3;

    [SerializeField] private Button closeBtn;

    private void Start()
    {
        closeBtn.onClick.AddListener(CloseEvent);
        UpdateTxtTask();
    }

    private void CloseEvent()
    {
        Pop();
    }

    void UpdateTxtTask()
    {
        targetTaskTxt1.text = "/" + GameConfig.Instance.targetTask1;
        targetTaskTxt2.text = "/" + GameConfig.Instance.targetTask2;
        targetTaskTxt3.text = "/" + GameConfig.Instance.targetTask3;
    }
}