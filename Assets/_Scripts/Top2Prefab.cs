
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Top2Prefab : MonoBehaviour
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
