using System;
using GameTool.APIs.Scripts;
using GameTool.Assistants.DesignPattern;
using GameToolSample.GameConfigScripts;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.UI.ResourcesItems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemShopPrefab : MonoBehaviour
{
    [SerializeField] private Button choseBtn;
    [Header("Basic UI")] [SerializeField] Image icon;
    [SerializeField] Image lockIcon;

    [Header("Gold Button")] [SerializeField]
    Button buyGoldBtn;

    [SerializeField] TMP_Text goldPriceTxt;

    [Header("Ads Button")] [SerializeField]
    Button buyAdsBtn;

    [SerializeField] TMP_Text adsCountTxt;

    ItemInfo data;

    private void Start()
    {
        buyGoldBtn.onClick.AddListener(ButtonBuyWGold);
        buyAdsBtn.onClick.AddListener(ButtonBuyWAds);
        CheckBuyAds();
    }

    void CheckBuyAds()
    {
        ItemShopState itemShopState = GameData.Instance.GetItemShopState(data.id);

        if (itemShopState.CountWatched >= data.TargetAds)
        {
            lockIcon.gameObject.SetActive(false);
            buyAdsBtn.gameObject.SetActive(false);
        }
    }

    public void Init(ItemInfo itemInfo)
    {
        data = itemInfo;
        icon.sprite = data.icon;

        buyGoldBtn.gameObject.SetActive(false);
        buyAdsBtn.gameObject.SetActive(false);

        if (data.isGold)
        {
            buyGoldBtn.gameObject.SetActive(true);
            goldPriceTxt.text = data.goldPrice.ToString();
        }

        if (data.isAds)
        {
            ItemShopState itemShopState = GameData.Instance.GetItemShopState(data.id);
            buyAdsBtn.gameObject.SetActive(true);
            adsCountTxt.text = $"{itemShopState.CountWatched}/{data.TargetAds}";
        }
    }

    private void ButtonBuyWGold()
    {
        int price = data.goldPrice;
        if (GameData.Instance.Coin >= price)
        {
            GameData.Instance.SpendItem(new CurrencyInfo(ItemResourceType.Coin, price),
                true, AnalyticID.LocationTracking.buygold);
            this.PostEvent(EventID.UpdateData);
            buyGoldBtn.gameObject.SetActive(false);
            lockIcon.gameObject.SetActive(false);
        }
    }

    private void ButtonBuyWAds()
    {
        API.Instance.ShowReward(success =>
        {
            if (success)
            {
                ItemShopState itemShopState = GameData.Instance.GetItemShopState(data.id);
                itemShopState.IsAds = true;
                itemShopState.CountWatched++;
                if (itemShopState.CountWatched >= data.TargetAds)
                {
                    itemShopState.IsUnlock = true;
                    Debug.LogError(itemShopState.CountWatched);
                    buyAdsBtn.gameObject.SetActive(false);
                    lockIcon.gameObject.SetActive(false);
                }
                else
                {
                    buyAdsBtn.gameObject.SetActive(true);
                    adsCountTxt.text = $"{itemShopState.CountWatched}/{data.TargetAds}";
                }

                GameData.Instance.SetItemShopState(itemShopState);
            }
        }, AnalyticID.LocationTracking.buyads);
    }
}