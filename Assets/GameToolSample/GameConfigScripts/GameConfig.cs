using System;
using GameTool.Assistants.DesignPattern;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.Scripts.UI.ResourcesItems;
using GameToolSample.UIFeature.BasicPopup;
using GameToolSample.UIFeature.BasicPopup.Scripts;
using GameToolSample.UIManager;
using UnityEngine;

namespace GameToolSample.GameConfigScripts
{
    public class GameConfig : SingletonMonoBehaviour<GameConfig>
    {
        [SerializeField] private ItemResourceData _itemResourceData;
        [SerializeField] private IAPConfig _iapConfig;
        public bool IsHome = false;
        public int CoinFreeAds = 1000;
        [Header("CONFIG")]
        [SerializeField] private int _totalLevel = 100;

        public ItemResourceData ItemResourceData => _itemResourceData;
        public IAPConfig IAPConfig => _iapConfig;
        public int TotalLevel => _totalLevel;
    }

    [Serializable]
    public class IAPConfig { }
}