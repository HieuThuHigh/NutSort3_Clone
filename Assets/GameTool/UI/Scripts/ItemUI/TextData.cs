using System;
using GameTool.Assistants.DesignPattern;
using GameTool.Assistants.Helper;
using GameToolSample.Scripts.Enum;
using TMPro;
using UnityEngine;

namespace GameTool.UI.Scripts.ItemUI
{
    public class TextData : MonoBehaviour
    {
        enum TextType
        {
            Coin,
            Diamond,
            Level
        }

        [SerializeField] private TextType textType = TextType.Coin;

        [SerializeField] private TextMeshProUGUI txtOut;
        // Start is called before the first frame update

        private void OnEnable()
        {
            if (!txtOut)
            {
                txtOut = GetComponent<TextMeshProUGUI>();
            }

            UpdateText();

            this.RegisterListener(EventID.UpdateData, UpdateDataListener);
        }

        private void OnDisable()
        {
            this.RemoveListener(EventID.UpdateData, UpdateDataListener);
        }

        void UpdateDataListener(Component o, object[] sender = null)
        {
            UpdateText();
        }

        void UpdateText()
        {
            switch (textType)
            {
                case TextType.Coin:
                    txtOut.text = AbbrevationUtility.AbbreviateNumber(GameToolSample.GameDataScripts.Scripts.GameData.Instance.CoinFake);
                    break;
                case TextType.Diamond:
                    txtOut.text = AbbrevationUtility.AbbreviateNumber(GameToolSample.GameDataScripts.Scripts.GameData.Instance.DiamondFake);
                    break;
                case TextType.Level:
                    txtOut.text = String.Format("Level: {0}", GameToolSample.GameDataScripts.Scripts.GameData.Instance.CurrentLevel);
                    break;
            }
        }
    }
}
