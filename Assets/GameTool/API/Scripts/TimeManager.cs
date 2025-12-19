using System;
using System.Collections.Generic;
using System.Linq;
using GameTool.Assistants.DesignPattern;
using UnityEngine;

namespace GameTool.APIs.Scripts
{
    public class TimeManager : SingletonMonoBehaviour<TimeManager>
    {
        public List<float> _listTime;

        public void Add(float value)
        {
            _listTime.Add(value);
            CheckTimeScale();
        }

        public void Remove(float value)
        {
            int index = -1;
            try
            {
                index = _listTime.FindIndex(f => Mathf.Approximately(f, value));
            }
            catch (Exception e)
            {
                // ignored
            }

            try
            {
                _listTime.RemoveAt(index);
            }
            catch (Exception e)
            {
                // ignored
            }

            CheckTimeScale();
        }

        public void CheckTimeScale()
        {
            if (_listTime.Count > 0)
            {
                Time.timeScale = _listTime.Min();
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }
}