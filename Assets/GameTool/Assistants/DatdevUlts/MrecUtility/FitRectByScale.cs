using DatdevUlts.Ults;
using UnityEngine;

namespace DatdevUlts
{
    public class FitRectByScale : MonoBehaviour
    {
        [SerializeField] private Vector2 _sizeDefault;
        [SerializeField] private RectTransform _parent;
        [SerializeField] private RectTransform _scaler;
        [SerializeField] private float _maxScale = 1;
        [SerializeField] private float _minScale;
        [SerializeField] private FitType _fitType;
        [SerializeField] private AdjustPosAfterFit _adjustPosAfter;

        private void Start()
        {
            Fit();
            this.DelayedCall(0.01f, Fit, true);
        }

        private void OnEnable()
        {
            this.DelayedCall(0.01f, Fit, true);
        }

        [ContextMenu("Fit")]
        public void Fit()
        {
            _scaler.localPosition = _parent.localPosition;

            if (_fitType == FitType.None)
            {
                return;
            }

            var sizeParent = _parent.rect.size;
            var scale = _maxScale;

            if (_fitType == FitType.Width)
            {
                scale = sizeParent.x / _sizeDefault.x;
            }
            else if (_fitType == FitType.Height)
            {
                scale = sizeParent.y / _sizeDefault.y;
            }
            else if (_fitType == FitType.Fit)
            {
                scale = Mathf.Min(sizeParent.x / _sizeDefault.x, sizeParent.y / _sizeDefault.y);
            }
            else if (_fitType == FitType.Expand)
            {
                scale = Mathf.Max(sizeParent.x / _sizeDefault.x, sizeParent.y / _sizeDefault.y);
            }

            if (scale > _maxScale)
            {
                scale = _maxScale;
            }

            if (scale < _minScale)
            {
                scale = _minScale;
            }

            _scaler.localScale = new Vector3(scale, scale, scale);
            
            _adjustPosAfter?.Adjust();
        }
    }

    public enum FitType
    {
        None,
        Width,
        Height,
        Fit,
        Expand,
    }
}