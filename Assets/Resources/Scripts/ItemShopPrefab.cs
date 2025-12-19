using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemShopPrefab : MonoBehaviour
{
    [Header("Basic UI")]
    [SerializeField] Image icon;

    [Header("Gold Button")]
    [SerializeField] Button buyGoldBtn;
    [SerializeField] TMP_Text goldPriceTxt;

    [Header("Ads Button")]
    [SerializeField] Button buyAdsBtn;
    [SerializeField] TMP_Text adsCountTxt;

    ItemInfo data;

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
            buyAdsBtn.gameObject.SetActive(true);
            adsCountTxt.text = data.adsRequire.ToString();
        }
    }
}