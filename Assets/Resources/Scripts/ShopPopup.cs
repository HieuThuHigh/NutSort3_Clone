using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameConfigScripts;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.UI.ResourcesItems;
using GameToolSample.UIFeature.BasicPopup.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopup : BaseUI
{
    [SerializeField] Button backButton;
    [SerializeField] Button coinAdsButton;
    [SerializeField] Button ringTabBtn;
    [SerializeField] Button bgTabBtn; 
    [SerializeField] Image bgImg;
    [SerializeField] Image iconRingTab;
    [SerializeField] Image iconBgTab;
    [SerializeField] Sprite[] ringTabStateSprs;
    [SerializeField] Sprite[] bgTabStateSprs;
    [SerializeField] Image frameRingTab;
    [SerializeField] Image frameBgTab;
    [SerializeField] Sprite[] tabStateSprs;
    int currentTab = 0;
    [SerializeField] GameObject ringTab;
    [SerializeField] GameObject bgTab;
    
    [SerializeField] Transform ringContent;
    [SerializeField] Transform bgContent;
    
    [SerializeField] ItemShopPrefab ringItemPrefab;
    [SerializeField] ItemShopPrefab bgItemPrefab;

    [SerializeField] ItemData ringItemData;
    [SerializeField] ItemData bgItemData;

    
    
    private void Start()
    {
        ringTabBtn.onClick.AddListener(RingTabClick);
        bgTabBtn.onClick.AddListener(BgTabClick);
        backButton.onClick.AddListener(BackClick);
        coinAdsButton.onClick.AddListener(CoinAdsClick);
        SpawnItems(ringItemData, ringItemPrefab, ringContent);
        SpawnItems(bgItemData, bgItemPrefab, bgContent);
        
        SwitchTab(0);
    }

    private void CoinAdsClick()
    {
        Debug.LogError("coin cong");
        GameData.Instance.AddCurrency(ItemResourceType.Coin, GameConfig.Instance.CoinFreeAds);
    }

    private void BackClick()
    {
        Pop();
        Debug.LogError("VAR");
        
    }

    void SpawnItems(ItemData data, ItemShopPrefab prefab, Transform targetContent)
    {
        // Xóa item cũ trong content đó
        foreach (Transform child in targetContent)
        {
            Destroy(child.gameObject);
        }

        // Sinh item mới
        foreach (var info in data.items)
        {
            ItemShopPrefab item =
                Instantiate(prefab, targetContent);

            item.Init(info);
        }
    }



    private void BgTabClick()
    {
        SwitchTab(1);
    }
    private void RingTabClick()
    {
        SwitchTab(0);
    }
    private void SwitchTab(int tab)
    {
        currentTab = tab;

        iconBgTab.sprite = currentTab == 1 ? bgTabStateSprs[0] : bgTabStateSprs[1];
        iconRingTab.sprite = currentTab == 0 ? ringTabStateSprs[0] : ringTabStateSprs[1];
        frameRingTab.sprite = currentTab == 0 ? tabStateSprs[0] : tabStateSprs[1];
        frameBgTab.sprite = currentTab == 1 ? tabStateSprs[0] : tabStateSprs[1];

        ringTab.SetActive(currentTab == 0);
        bgTab.SetActive(currentTab == 1);
        
        ringContent.gameObject.SetActive(currentTab == 0);
        bgContent.gameObject.SetActive(currentTab == 1);
    }


}
