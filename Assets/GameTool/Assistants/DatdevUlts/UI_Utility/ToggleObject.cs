using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleObject : MonoBehaviour
{
    [SerializeField] private Toggle _buttonToggle;
    [SerializeField] private List<GameObject> _listActive = new List<GameObject>();
    [SerializeField] private List<GameObject> _listDeactive = new List<GameObject>();

    public Toggle ButtonToggle => _buttonToggle;

    public List<GameObject> ListActive => _listActive;

    public List<GameObject> ListDeactive => _listDeactive;

    private void Awake()
    {
        _buttonToggle.onValueChanged.AddListener(UpdateData);
    }

    public void AddListener(UnityAction<bool> toggleEvent)
    {
        _buttonToggle.onValueChanged.AddListener(toggleEvent);
    }

    public void SetData(bool isOn)
    {
        _buttonToggle.isOn = isOn;
        UpdateData(isOn);
    }

    private void UpdateData(bool isOn)
    {
        _listActive.ForEach(o => o.SetActive(isOn) );
        _listDeactive.ForEach(o => o.SetActive(!isOn) );
    }
}