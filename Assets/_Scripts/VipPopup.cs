using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameDataScripts.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class VipPopup : BaseUI
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button subBtn;
    [SerializeField] private Button termsBtn;
    [SerializeField] private Button privacyBtn;
    
    private void Start()
    {
        closeBtn.onClick.AddListener(CloseEvent);
        subBtn.onClick.AddListener(SubEvent);
        termsBtn.onClick.AddListener(TermEvent);
        privacyBtn.onClick.AddListener(PrivacyEvent);
    }

    private void PrivacyEvent()
    {
        
    }

    private void TermEvent()
    {
        
    }

    private void SubEvent()
    {
        Pop();
        GamePlayUi.Instance.vipBtn.gameObject.SetActive(false);
        GameData.Instance.IsBuyVipAccess = true;
    }

    private void CloseEvent()
    {
        Pop();
    }
}
