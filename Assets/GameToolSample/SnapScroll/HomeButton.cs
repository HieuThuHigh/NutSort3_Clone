using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SnapScroll
{
    public class HomeButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _buttonTxt;
        [SerializeField] private GameObject _buttonIconOn;
        [SerializeField] private GameObject _buttonIconOff;
        [SerializeField] private Button _btnClick;
        [SerializeField] private Vector2 _sizeDeltaOn;
        [SerializeField] private Vector2 _sizeDeltaOff;
        [SerializeField] private Vector2 _anchorPosOn;
        [SerializeField] private Vector2 _anchorPosOff;
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private List<Transform> _listLine = new List<Transform>();
        [SerializeField] private List<Transform> _listPos = new List<Transform>();

        private void Awake()
        {
            for (int i = 0; i < _listLine.Count; i++)
            {
                _listLine[i].transform.SetParent(transform.parent);
                _listLine[i].transform.SetSiblingIndex(0);
            }
        }

        public void UpdateLine()
        {
            for (int i = 0; i < _listLine.Count; i++)
            {
                _listLine[i].position = _listPos[i].position;
            }
        }

        public LayoutElement LayoutElement => _layoutElement;

        public Vector2 SizeDeltaOn => _sizeDeltaOn;

        public Vector2 SizeDeltaOff => _sizeDeltaOff;

        public Vector2 AnchorPosOn => _anchorPosOn;

        public Vector2 AnchorPosOff => _anchorPosOff;

        public Button BtnClick => _btnClick;

        public TextMeshProUGUI ButtonTxt => _buttonTxt;

        public GameObject ButtonIconOn => _buttonIconOn;

        public GameObject ButtonIconOff => _buttonIconOff;
    }
}