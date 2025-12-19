using System;
using UnityEngine;

namespace DatdevUlts.AnimationUtils
{
    public class AnimatorEvent : MonoBehaviour
    {
        /// <summary>
        /// arg1: Tên sự kiện
        /// </summary>
        public event Action<string> TrackEvent;
        
        public void InvokePostEvent(string eventName)
        {
            TrackEvent?.Invoke(eventName);
        }

        public void InvokeRegistEvent(Action<string> callBack)
        {
            TrackEvent += callBack;
        }

        public void InvokeRemoveEvent(Action<string> callBack)
        {
            TrackEvent -= callBack;
        }
    }
}