using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
    [Header("Buy Type")] public bool isGold;

    public bool isAds;
    public ItemType ItemType;
    [Header("UI Value")] public int goldPrice;
    public int TargetAds;
}

public enum ItemType
{
    Background,
    Ring
}
