using DG.Tweening;
using GameTool.Audio;
using GameTool.Audio.Scripts;
using GameTool.Vibration;
using GameToolSample.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameTool.UI.Scripts.ItemUI
{
    public class ButtonScaler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Vector3 startScale;
        public Vector3 endScale;
        [SerializeField] bool useAudio = true;
        [SerializeField] bool useScaleEffect = true;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            if (useScaleEffect)
            {
                startScale = transform.localScale;
                endScale = new Vector3(startScale.x - 0.05f, startScale.y - 0.05f,startScale.z);
            }
            else
            {
                endScale = transform.localScale;
            }
        }
#endif
        public void OnPointerDown(PointerEventData eventData)
        {
            if (useScaleEffect)
            {
                transform.DOScale(endScale, 0.05f).SetEase(Ease.Linear).SetUpdate(true);
                VibrationManager.Instance.Vibrate();
            }
            if (useAudio)
            {
                if (AudioManager.IsInstanceValid())
                {
                    AudioManager.Instance.Shot(eSoundName.ButtonClick);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (useScaleEffect)
            {
                transform.DOScale(startScale, 0.1f).SetEase(Ease.Linear).SetUpdate(true);
            }
        }

    }
}
