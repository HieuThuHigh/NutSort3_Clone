using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameTool.APIs.Scripts;
using GameTool.Assistants.DesignPattern;
using GameTool.Assistants.Helper;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameConfigScripts;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.UI.ResourcesItems;
using GameToolSample.UIFeature.BasicPopup.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopup : SingletonUI<ShopPopup>
{
    [SerializeField] Button backButton;
    [SerializeField] Button coinAdsButton;
    [SerializeField] GameObject coinAdsButtonWait;
    [SerializeField] TextMeshProUGUI timeCoinAdsTxt;
    [SerializeField] Button ringTabBtn;
    [SerializeField] Button bgTabBtn;
    public Image bgImg;
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
    [SerializeField] private Image ringImg;
    [SerializeField] private Sprite[] backgroundSprites;
    private void Start()
    {
        ringTabBtn.onClick.AddListener(RingTabClick);
        bgTabBtn.onClick.AddListener(BgTabClick);
        backButton.onClick.AddListener(BackClick);
        coinAdsButton.onClick.AddListener(CoinAdsClick);
        SpawnItems(ringItemData, ringItemPrefab, ringContent);
        SpawnItems(bgItemData, bgItemPrefab, bgContent);
        LoadSelectedItems();
        SwitchTab(0);
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(GameData.Instance.TimeWatchAds))
        {
            coinAdsButton.gameObject.SetActive(true);
            coinAdsButtonWait.SetActive(false);
        }
        else
        {
            coinAdsButton.gameObject.SetActive(false);
            coinAdsButtonWait.SetActive(true);
            timeCoinAdsTxt.gameObject.SetActive(true);
            StartCoroutine(CanShowAds());

        }
    }

    private void OnDisable()
    {
        LoadSelectedItems();
    }

    private void CoinAdsClick()
    {
        GameData.Instance.AddCurrency(ItemResourceType.Coin, GameConfig.Instance.CoinFreeAds);
        
        GameData.Instance.TimeWatchAds = DateTime.Now.ToString();
        this.PostEvent(EventID.UpdateData);
        
        coinAdsButton.gameObject.SetActive(false);
        coinAdsButtonWait.SetActive(true);
        timeCoinAdsTxt.gameObject.SetActive(true);
        StartCoroutine(CanShowAds());

    }

    IEnumerator CanShowAds()
    {
        int cooldown = 2 * 60;

        while (true)
        {
            if (string.IsNullOrEmpty(GameData.Instance.TimeWatchAds))
            {
                coinAdsButton.gameObject.SetActive(true);
                coinAdsButtonWait.SetActive(false);
                timeCoinAdsTxt.text = "";
                yield break;
            }
            
            DateTime timeStart = DateTime.Parse(GameData.Instance.TimeWatchAds);

            TimeSpan timePassed = DateTime.Now - timeStart;
            int remain = cooldown - (int)timePassed.TotalSeconds;

            if (remain <= 0)
            {
                coinAdsButton.gameObject.SetActive(true);
                coinAdsButtonWait.SetActive(false);
                timeCoinAdsTxt.text = "";
                yield break;
            }

            int minutes = remain / 60;
            int seconds = remain % 60;
            timeCoinAdsTxt.text = $"{minutes:D2}:{seconds:D2}";

            yield return new WaitForSeconds(1);
        }
    }
    public void OnItemSelected(ItemInfo itemInfo)
    {
        if (itemInfo.itemType == ItemType.Background)
        {
            GameData.Instance.SelectedShopBgId = itemInfo.id;

            if (itemInfo.id < backgroundSprites.Length)
            {
                bgImg.sprite = backgroundSprites[itemInfo.id];
                GamePlayUi.Instance.GamePlayImg.sprite = backgroundSprites[itemInfo.id];
            }
        }
        else if (itemInfo.itemType == ItemType.Ring)
        {
            GameData.Instance.SelectedShopRingId = itemInfo.id;
            ringImg.sprite = itemInfo.icon;
        }
        this.PostEvent(EventID.UpdateData);
    }

    public void OnItemPreview(ItemInfo itemInfo)
    {
        if (itemInfo.itemType == ItemType.Background)
        {
            if (itemInfo.id < backgroundSprites.Length)
            {
                bgImg.sprite = backgroundSprites[itemInfo.id];
            }
        }
        else if (itemInfo.itemType == ItemType.Ring)
        {
            ringImg.sprite = itemInfo.icon;
        }
    }

    void LoadSelectedItems()
    {
        int selectedBgId = GameData.Instance.SelectedShopBgId;
        if (selectedBgId > 0 && selectedBgId < backgroundSprites.Length)
        {
            bgImg.sprite = backgroundSprites[selectedBgId];
        }

        int selectedRingId = GameData.Instance.SelectedShopRingId;
        if (selectedRingId > 0)
        {
            var ringInfo = FindItemInData(ringItemData, selectedRingId);
            if (ringInfo != null)
                ringImg.sprite = ringInfo.icon;
        }
    }

    private ItemInfo FindItemInData(ItemData data, int id)
    {
        foreach (var item in data.items)
        {
            if (item.id == id)
                return item;
        }

        return null;
    }


    private void BackClick()
    {
        LoadSelectedItems();
        Pop();
    }

    void SpawnItems(ItemData data, ItemShopPrefab prefab, Transform targetContent)
    {
        foreach (Transform child in targetContent)
        {
            Destroy(child.gameObject);
        }

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