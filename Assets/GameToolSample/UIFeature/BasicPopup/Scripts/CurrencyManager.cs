using DatdevUlts.Ults;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.APIs;
using GameToolSample.UIManager;
using UnityEngine;
using UnityEngine.UI;


namespace GameToolSample.UIFeature.BasicPopup.Scripts
{
    public class CurrencyManager : SingletonList<CurrencyManager>
    {
        [Header("COMPONENT")] [SerializeField] public Button coinButton;
        [SerializeField] public Button diamondButton;
        [SerializeField] public Button heartButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button _btnAvatar;
        [SerializeField] private RectTransform posCoin;
        [SerializeField] private RectTransform posDiamond;

        public Button CoinButton => coinButton;

        public Button DiamondButton => diamondButton;

        public Button HeartButton => heartButton;

        public Button SettingButton => settingButton;

        public Button BtnAvatar => _btnAvatar;

        public RectTransform PosCoin => posCoin;

        public RectTransform PosDiamond => posDiamond;

        private void Start()
        {
            if (coinButton)
                coinButton.onClick.AddListener(ClickCoin);

            if (diamondButton)
            {
                diamondButton.onClick.AddListener(ClickDiamond);
            }

            if (heartButton)
            {
                heartButton.onClick.AddListener(ClickHeart);
            }

            if (settingButton)
            {
                settingButton.onClick.AddListener(OpenSettingPopup);
            }
        }

        private void ClickHeart()
        {
        }

        public void ClickCoin()
        {
        }

        public void ClickDiamond()
        {
        }

        public void OpenSettingPopup()
        {
        }
    }
}