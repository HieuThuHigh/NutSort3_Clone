using UnityEngine;

namespace DatdevUlts
{
    public class AdjustMrecMinY : AdjustPosAfterFit
    {
        [SerializeField] private RectTransform _frame;
        [SerializeField] private float _minY = 109;
        public override void Adjust()
        {
            base.Adjust();
            if (_frame.anchoredPosition.y < _minY)
            {
                var vector2 = _frame.anchoredPosition;
                vector2.y = _minY;
                _frame.anchoredPosition = vector2;
            }
        }
    }
}