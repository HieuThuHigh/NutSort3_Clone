using System.Collections.Generic;
using UnityEngine;
using GameTool.UI.Scripts.CanvasPopup;

public class WeekingRatingPopup : BaseUI
{
    [SerializeField] Transform contentParent;
    [SerializeField] FrameDataPrefabs framePrefab;

    [Header("Star Random Range")]
    [SerializeField] int minStar = 1;
    [SerializeField] int maxStar = 10;

    void Start()
    {
        SpawnOtherPlayers();
    }

    void SpawnOtherPlayers()
    {
        List<(string userName, int star)> listUser =
            new List<(string, int)>()
            {
                ("Anh Hieu", 0),
                ("Bao", 0),
                ("Cuong", 0),
                ("Duc", 0),
                ("Huy", 0),
                ("Khanh", 0),
                ("Long", 0),
                ("Minh", 0),
                ("Nam", 0),
                ("Phong", 0),
            };

        for (int i = 0; i < listUser.Count; i++)
        {
            int randomStar = Random.Range(minStar, maxStar + 1);
            listUser[i] = (listUser[i].userName, randomStar);
        }

        listUser.Sort((a, b) => b.star.CompareTo(a.star));

        for (int i = 0; i < listUser.Count; i++)
        {
            int rank = i + 1;

            FrameDataPrefabs newUser = Instantiate(framePrefab, contentParent);
            newUser.SetData(listUser[i].userName, rank, listUser[i].star);
        }
    }
}