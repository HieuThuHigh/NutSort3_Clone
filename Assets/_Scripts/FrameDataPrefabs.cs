using System;
using TMPro;
using UnityEngine;

public class FrameDataPrefabs : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI starTxt;
    [SerializeField] private TextMeshProUGUI rankTxt;

    public void SetData(string name, int rank, int star)
    {
        rankTxt.text = rank.ToString();
        nameTxt.text = name;
        starTxt.text = star.ToString();
    }
}

[Serializable]
public class UserData
{
    public string userName;
    public int star;
}