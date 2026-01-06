using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameConfigScripts;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.UIManager;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : BaseUI
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button vibraBtn;
    [SerializeField] private Button rateBtn;
    [SerializeField] private Button soundBtn;

    [SerializeField] private Image vibraImage;
    [SerializeField] private Image soundImage;

    [SerializeField] private Sprite[] btnSprites;
    public GameObject continueObj;

    private void Start()
    {
        closeBtn.onClick.AddListener(CloseClick);
        rateBtn.onClick.AddListener(RateClick);
        soundBtn.onClick.AddListener(SoundClick);
        vibraBtn.onClick.AddListener(VibraClick);
    }

    private void OnEnable()
    {
        UpdateUI();
        CheckHomeButton();
    }

    void CheckHomeButton()
    {
        if (GameConfig.Instance.IsHome == true)
        {
            continueObj.gameObject.SetActive(false);
        }
    }
    private void VibraClick()
    {
        GameData.Instance.Vibrate = !GameData.Instance.Vibrate;
        vibraImage.sprite = GameData.Instance.Vibrate ? btnSprites[0] : btnSprites[1];
    }

    private void SoundClick()
    {
        GameData.Instance.SoundFX = !GameData.Instance.SoundFX;
        soundImage.sprite = GameData.Instance.SoundFX ? btnSprites[0] : btnSprites[1];
    }

    private void RateClick()
    {
        //CanvasManager.Instance.Pop(eUIName.Rate);
    }

    private void CloseClick()
    {
        CanvasManager.Instance.Pop(eUIName.SettingPopup);
    }

    private void UpdateUI()
    {
        soundImage.sprite = GameData.Instance.SoundFX ? btnSprites[0] : btnSprites[1];
        vibraImage.sprite = GameData.Instance.Vibrate ? btnSprites[0] : btnSprites[1];
    }

}