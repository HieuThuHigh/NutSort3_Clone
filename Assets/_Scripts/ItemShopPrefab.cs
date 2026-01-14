using System.Linq;
using GameTool.Assistants.DesignPattern;
using GameTool.GameDataScripts;
using GameToolSample.GameConfigScripts;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.Toast;
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
    [SerializeField] private ItemInfo data;

    private void Start()
    {
        buyGoldBtn.onClick.AddListener(BuyWithGoldEvent);
        buyAdsBtn.onClick.AddListener(BuyWithAdsEvent);
        choseBtn.onClick.AddListener(ChoseEvent);
        CheckStateItem();
    }

    void ChoseEvent()
    {
        ShopPopup.Instance.OnItemSelected(data);
    }

    void CheckStateItem()
    {
        if (data == null) return;

        if (data.ItemType == ItemType.Ring)
        {
            if (GameData.Instance.Data.BoughtItemIds.Contains(data.id))
            {
                lockIcon.gameObject.SetActive(false);
                buyGoldBtn.gameObject.SetActive(false);
                buyAdsBtn.gameObject.SetActive(false);
                choseBtn.gameObject.SetActive(true);
            }
        }
        else
        {
            if (GameData.Instance.Data.BoughtItemIdsBG.Contains(data.id))
            {
                lockIcon.gameObject.SetActive(false);
                buyGoldBtn.gameObject.SetActive(false);
                buyAdsBtn.gameObject.SetActive(false);
                choseBtn.gameObject.SetActive(true);
            }
        }
    }


    private void BuyWithGoldEvent()
    {
        if (GameData.Instance.Coin >= data.goldPrice)
        {
            GameData.Instance.SpendItem(new CurrencyInfo(ItemResourceType.Coin, data.goldPrice),
                true, AnalyticID.LocationTracking.buygold);
            if (data.ItemType == ItemType.Ring)
            {
                GameData.Instance.BoughtItemIds.Add(data.id);
                SaveGameData.SaveData(eData.BoughtItemIds, GameData.Instance.Data.BoughtItemIds);
            }
            else
            {
                GameData.Instance.BoughtItemIdsBG.Add(data.id);
                SaveGameData.SaveData(eData.BoughtItemIdsBG, GameData.Instance.Data.BoughtItemIdsBG);
            }

            // GameData.Instance.BoughtItemIds.Add(data.id);
            // GameData.Instance.BoughtItemIds = GameData.Instance.BoughtItemIds;s
            lockIcon.gameObject.SetActive(false);
            buyGoldBtn.gameObject.SetActive(false);
            choseBtn.gameObject.SetActive(true);
        }
        else
        {
            ToastMgr.Instance.Show(new string[] { "Not Enough Coins" });
        }

        this.PostEvent(EventID.UpdateData);
    }

    private void BuyWithAdsEvent()
    {
        var itemRing = GameData.Instance._stateRingList.Find(itemaaa => itemaaa.Id == data.id);
        if (data.ItemType == ItemType.Ring)
        {
            if (itemRing == null)
            {
                itemRing = new StateRingList() { Id = data.id };
                GameData.Instance._stateRingList.Add(itemRing);
            }

            itemRing.RingWatchAdsCount++;
            adsCountTxt.text = itemRing.RingWatchAdsCount + "/" + data.TargetAds;
            if (itemRing.RingWatchAdsCount >= data.TargetAds)
            {
                lockIcon.gameObject.SetActive(false);
                buyAdsBtn.gameObject.SetActive(false);
                GameData.Instance.BoughtItemIds.Add(data.id);
                SaveGameData.SaveData(eData.BoughtItemIds, GameData.Instance.Data.BoughtItemIds);
            }

            SaveGameData.SaveData(eData._stateRingList, GameData.Instance._stateRingList);
        }

        var itemBackGround = GameData.Instance.StateBgLists.Find(itemBG => itemBG.Id == data.id);
        if (data.ItemType == ItemType.Background)
        {
            if (itemBackGround == null)
            {
                itemBackGround = new StateBGList() { Id = data.id };
                GameData.Instance.StateBgLists.Add(itemBackGround);
            }

            itemBackGround.BGWatchAdsCount++;
            adsCountTxt.text = itemBackGround.BGWatchAdsCount + "/" + data.TargetAds;
            if (itemBackGround.BGWatchAdsCount >= data.TargetAds)
            {
                lockIcon.gameObject.SetActive(false);
                buyAdsBtn.gameObject.SetActive(false);
                GameData.Instance.BoughtItemIdsBG.Add(data.id);
                SaveGameData.SaveData(eData.BoughtItemIdsBG, GameData.Instance.Data.BoughtItemIdsBG);
            }

            SaveGameData.SaveData(eData.StateBgLists, GameData.Instance.StateBgLists);
        }
    }

    public void SetUpData(ItemInfo dataInfo)
    {
        data = dataInfo;
        icon.sprite = data.icon;


        if (data.isAds)
        {
            buyAdsBtn.gameObject.SetActive(true);
            buyGoldBtn.gameObject.SetActive(false);
            if (data.ItemType == ItemType.Background)
            {
                var itemBackGround = GameData.Instance.StateBgLists.Find(itemBG => itemBG.Id == data.id);
                int currentAds = 0;
                if (itemBackGround != null)
                {
                    currentAds = itemBackGround.BGWatchAdsCount;
                }

                adsCountTxt.text = currentAds + "/" + data.TargetAds;
            }
            else
            {
                var itemRing = GameData.Instance._stateRingList.Find(itemBG => itemBG.Id == data.id);
                int currentAds = 0;
                if (itemRing != null)
                {
                    currentAds = itemRing.RingWatchAdsCount;
                }

                adsCountTxt.text = currentAds + "/" + data.TargetAds;
            }
        }

        if (data.isGold)
        {
            buyGoldBtn.gameObject.SetActive(true);
            buyAdsBtn.gameObject.SetActive(false);
            goldPriceTxt.text = data.goldPrice.ToString();
        }
    }
}