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
    }
    public void Init(ItemInfo itemInfo)
    {
        data = itemInfo;
        icon.sprite = data.icon;
        
        buyGoldBtn.gameObject.SetActive(false);
        buyAdsBtn.gameObject.SetActive(false);

        if (data.isOwned)
        {
            lockIcon.gameObject.SetActive(false);
            return;
        }

        lockIcon.gameObject.SetActive(true);
        
        if (data.isGold)
        {
            buyGoldBtn.gameObject.SetActive(true);
            goldPriceTxt.text = data.goldPrice.ToString();
        }

        if (data.isAds)
        {
            if (data.adsWatched >= data.adsRequire)
            {
                data.isOwned = true;
                lockIcon.gameObject.SetActive(false);
            }
            else
            {
                buyAdsBtn.gameObject.SetActive(true);
                adsCountTxt.text = $"{data.adsWatched}/{data.adsRequire}";
            }
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
            data.isOwned = true;
            buyGoldBtn.gameObject.SetActive(false);
            lockIcon.gameObject.SetActive(false);
        }
    }
    private void ButtonBuyWAds()
    {
        if (data.adsWatched >= data.adsRequire) return;
        
        API.Instance.ShowReward(success =>
        {
            if (success)
            {
                data.adsWatched++;
                
                if (data.adsWatched >= data.adsRequire)
                {
                    data.isOwned = true;
                    buyAdsBtn.gameObject.SetActive(false);
                    lockIcon.gameObject.SetActive(false);
                }
                else
                {
                    adsCountTxt.text = $"{data.adsWatched}/{data.adsRequire}";
                }
            }
        },AnalyticID.LocationTracking.buyads);
    }
}