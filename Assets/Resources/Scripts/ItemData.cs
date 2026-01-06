using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "ItemData",
    menuName = "Shop/ItemDataTable"
)]
public class ItemData : ScriptableObject
{
    public List<ItemInfo> items;
}

[System.Serializable]
public class ItemInfo
{
    public int id;
    public Sprite icon;
    public ItemType itemType;
    [Header("Buy Type")]
    public bool isGold;
    public bool isAds;

    [Header("UI Value")]
    public int goldPrice;

    public int TargetAds = 5;

}
public enum ItemType
{
    Background,
    Ring
}