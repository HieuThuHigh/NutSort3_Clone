using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.Assistants.DesignPattern;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameConfigScripts;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.UI.ResourcesItems;
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
    [SerializeField] Image iconRingTab;
    [SerializeField] Image iconBgTab;
    [SerializeField] Sprite[] ringTabStateSprs;
    [SerializeField] Sprite[] bgTabStateSprs;
    [SerializeField] Image frameRingTab;
    [SerializeField] Image frameBgTab;
    [SerializeField] Sprite[] tabStateSprs;

    [SerializeField] GameObject ringTab;
    [SerializeField] GameObject bgTab;

    [SerializeField] Transform ringContent;
    [SerializeField] Transform bgContent;
    [SerializeField] ItemShopPrefab ringItemPrefab;
    [SerializeField] ItemShopPrefab bgItemPrefab;
    [SerializeField] ItemData ringItemData;
    [SerializeField] ItemData bgItemData;

    [SerializeField] private Image ringImg;
    [SerializeField] private Image bgImg;
    public Sprite[] backgroundSprites;
    [SerializeField] private Sprite[] ringSprites;

    Coroutine countRoutine;

    private void Start()
    {
        ringTabBtn.onClick.AddListener(RingTabClick);
        bgTabBtn.onClick.AddListener(BgTabClick);
        backButton.onClick.AddListener(BackClick);
        coinAdsButton.onClick.AddListener(CoinAdsClick);

        SpawnItem(ringItemData, ringItemPrefab, ringContent);
        SpawnItem(bgItemData, bgItemPrefab, bgContent);

        CheckItemState();
        SwitchTab(0);

        
    }

    private void OnEnable()
    {
        if (GameData.Instance.TargetTime > DateTime.Now.Ticks)
            StartCountDown();
        else
            ShowReadyState();
    }

    private void CoinAdsClick()
    {
        GameData.Instance.AddCurrency(ItemResourceType.Coin, GameConfig.Instance.CoinFreeAds);

        GameData.Instance.TargetTime = DateTime.Now
            .AddSeconds(GameConfig.Instance.timeRewardAds)
            .Ticks;

        StartCountDown();
    }

    void StartCountDown()
    {
        if (countRoutine != null)
            StopCoroutine(countRoutine);

        countRoutine = StartCoroutine(IE_CountDown());
    }

    IEnumerator IE_CountDown()
    {
        coinAdsButton.gameObject.SetActive(false);
        coinAdsButtonWait.gameObject.SetActive(true);

        while (true)
        {
            long remain = GameData.Instance.TargetTime - DateTime.Now.Ticks;

            if (remain <= 0)
            {
                timeCoinAdsTxt.text = "00:00";
                ShowReadyState();
                yield break;
            }

            long totalSeconds = remain / TimeSpan.TicksPerSecond;
            long minutes = totalSeconds / 60;
            long seconds = totalSeconds % 60;

            timeCoinAdsTxt.text = minutes.ToString("00") + ":" + seconds.ToString("00");

            yield return new WaitForSeconds(1f);
        }
    }

    void ShowReadyState()
    {
        coinAdsButton.gameObject.SetActive(true);
        coinAdsButtonWait.gameObject.SetActive(false);
        timeCoinAdsTxt.text = "";
    }

    void SpawnItem(ItemData item, ItemShopPrefab prefab, Transform parent)
    {
        foreach (var info in item.items)
        {
            ItemShopPrefab it = Instantiate(prefab, parent);
            it.SetUpData(info);
        }
    }

    private void BackClick()
    {
        CheckItemState();
        if (GamePlayUi.Instance != null)
            GamePlayUi.Instance.CheckBackGroudGamePlay();

        Pop();
    }

    private void BgTabClick()
    {
        SwitchTab(1);
        bgTab.SetActive(true);
        ringTab.SetActive(false);
    }

    private void RingTabClick()
    {
        bgTab.SetActive(false);
        ringTab.SetActive(true);
        SwitchTab(0);
    }

    private void SwitchTab(int tab)
    {
        iconBgTab.sprite = tab == 1 ? bgTabStateSprs[0] : bgTabStateSprs[1];
        iconRingTab.sprite = tab == 0 ? ringTabStateSprs[0] : ringTabStateSprs[1];
        frameRingTab.sprite = tab == 0 ? tabStateSprs[0] : tabStateSprs[1];
        frameBgTab.sprite = tab == 1 ? tabStateSprs[0] : tabStateSprs[1];

        ringContent.gameObject.SetActive(tab == 0);
        bgContent.gameObject.SetActive(tab == 1);
    }

    void CheckItemState()
    {
        int bgId = GameData.Instance.SelectedShopBgID;
        if (bgId < backgroundSprites.Length)
        {
            bgImg.sprite = backgroundSprites[bgId];
            if (GamePlayUi.Instance != null)
                GamePlayUi.Instance.GamePlayImg.sprite = backgroundSprites[bgId];
        }

        int ringId = GameData.Instance.SelectedShopRingID;
        if (ringId < ringSprites.Length)
            ringImg.sprite = ringSprites[ringId];
    }

    public void OnItemSelected(ItemInfo itemInfo)
    {
        if (itemInfo.ItemType == ItemType.Background)
        {
            bgImg.sprite = backgroundSprites[itemInfo.id];
            if (GameData.Instance.BoughtItemIdsBG.Contains(itemInfo.id))
                GameData.Instance.SelectedShopBgID = itemInfo.id;
        }
        else
        {
            ringImg.sprite = ringSprites[itemInfo.id];
            if (GameData.Instance.BoughtItemIds.Contains(itemInfo.id))
                GameData.Instance.SelectedShopRingID = itemInfo.id;
        }
    }
}
