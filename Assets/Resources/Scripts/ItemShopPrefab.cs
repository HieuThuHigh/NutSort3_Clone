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
        choseBtn.onClick.AddListener(ChoseItemEvent);
        CheckItemState();
    }

    private void ChoseItemEvent()
    {
        ItemShopState itemShopState = GameData.Instance.GetItemShopState(data.id);
        if (itemShopState.IsBoughtWithCoin || itemShopState.CountWatched >= data.TargetAds)
        {

            ShopPopup.Instance.OnItemSelected(data);
        }
        else
        {
            ShopPopup.Instance.OnItemPreview(data);
        }
    }

    void CheckItemState()
    {
        ItemShopState itemShopState = GameData.Instance.GetItemShopState(data.id);

        if (itemShopState.IsBoughtWithCoin || itemShopState.CountWatched >= data.TargetAds)
        {
            lockIcon.gameObject.SetActive(false);
            buyAdsBtn.gameObject.SetActive(false);
            buyGoldBtn.gameObject.SetActive(false);
            choseBtn.gameObject.SetActive(true);
        }
    }
    public void Init(ItemInfo itemInfo)
    {
        data = itemInfo;
        icon.sprite = data.icon;

        buyGoldBtn.gameObject.SetActive(false);
        buyAdsBtn.gameObject.SetActive(false);

        ItemShopState itemShopState = GameData.Instance.GetItemShopState(data.id);
    
        if (itemShopState.IsBoughtWithCoin || itemShopState.IsUnlock)
        {
            lockIcon.gameObject.SetActive(false);
            return;
        }

        if (data.isGold)
        {
            buyGoldBtn.gameObject.SetActive(true);
            goldPriceTxt.text = data.goldPrice.ToString();
        }

        if (data.isAds)
        {
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
        
            ItemShopState itemShopState = GameData.Instance.GetItemShopState(data.id);
            itemShopState.IsBoughtWithCoin = true;
            itemShopState.IsUnlock = true;
            GameData.Instance.SetItemShopState(itemShopState);
        
            ShopPopup.Instance.OnItemSelected(data);
            
            this.PostEvent(EventID.UpdateData);
            buyGoldBtn.gameObject.SetActive(false);
            lockIcon.gameObject.SetActive(false);
            CheckItemState();
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
                     CheckItemState();
                     ShopPopup.Instance.OnItemSelected(data);
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