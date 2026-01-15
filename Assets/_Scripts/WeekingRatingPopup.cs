using System;
using System.Collections.Generic;
using GameTool.Assistants.DesignPattern;
using UnityEngine;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameConfigScripts;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using TMPro;
using UnityEngine.UI;

public class WeekingRatingPopup : BaseUI
{
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _rewardStar;

    [SerializeField] Transform contentParent;
    [SerializeField] Top1Prefab top1Prefab;
    [SerializeField] Top2Prefab top2Prefab;
    [SerializeField] Top3Prefab top3Prefab;
    [SerializeField] FrameDataPrefabs framePrefab;
    [SerializeField] private TextMeshProUGUI currentRankTxt;
    [SerializeField] private TextMeshProUGUI currentStarTxt;

    void Start()
    {
        _backButton.onClick.AddListener(BackEvent);
        _rewardStar.onClick.AddListener(RewardStar);
        currentStarTxt.text = GameData.Instance.CurrentStar.ToString();
        SpawnOtherPlayers();

        this.RegisterListener(EventID.UpdateData, UpdateStar);
    }
    private void UpdateStar(Component arg1, object[] arg2)
    {
        currentStarTxt.text = GameData.Instance.CurrentStar.ToString();
        SpawnOtherPlayers();
    }

    private void RewardStar()
    {
        GameData.Instance.CurrentStar += GameConfig.Instance.starRewardWeek;
        this.PostEvent(EventID.UpdateData);
    }

    private void BackEvent()
    {
        Pop();
    }


    void SpawnOtherPlayers()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        currentStarTxt.text = GameData.Instance.CurrentStar.ToString();
        List<(string userName, int star)> listUser = new List<(string, int )>()
        {
            ("Minh Hiếu 98", GameData.Instance.CurrentStar),
            ("Mary Johnson", 45),
            ("Robert Williams", 8),
            ("Patricia Brown", 29),
            ("John Jones", 3),
            ("Jennifer Garcia", 17),
            ("Michael Miller", 56),
            ("Linda Davis", 22),
            ("David Rodriguez", 9),
            ("Elizabeth Martinez", 34),
            ("William Hernandez", 7),
            ("Barbara Lopez", 88),
            ("Richard Gonzalez", 15),
            ("Susan Wilson", 41),
            ("Joseph Anderson", 10),
            ("Jessica Thomas", 63),
            ("Thomas Taylor", 27),
            ("Sarah Moore", 5),
            ("Charles Jackson", 19),
            ("Karen Martin", 81),
            ("Christopher Lee", 14),
            ("Lisa Perez", 30),
            ("Daniel Thompson", 2),
            ("Nancy White", 47),

            ("Matthew Harris", 11),
            ("Betty Sanchez", 25),
            ("Anthony Clark", 36),
            ("Margaret Ramirez", 72),
            ("Mark Lewis", 13),
            ("Sandra Robinson", 1),
            ("Donald Walker", 90),
            ("Ashley Young", 6),
            ("Steven Allen", 54),
            ("Kimberly King", 21),
            ("Paul Wright", 38),
            ("Emily Scott", 4),
            ("Andrew Torres", 77),
            ("Donna Nguyen", 16),
            ("Joshua Hill", 49),

            ("Michelle Flores", 23),
            ("Kenneth Green", 68),
            ("Dorothy Adams", 18),
            ("Kevin Nelson", 33),
            ("Carol Baker", 50),
            ("Brian Hall", 12),
            ("Amanda Rivera", 95),
            ("George Campbell", 7),
            ("Melissa Mitchell", 26),
            ("Edward Carter", 42),
            ("Deborah Roberts", 59),
            ("Ronald Gomez", 8),
            ("Stephanie Phillips", 31),
            ("Timothy Evans", 15),
            ("Rebecca Turner", 20),
            ("Jason Diaz", 37),
            ("Sharon Parker", 61),
            ("Jeffrey Cruz", 9),
            ("Laura Edwards", 44),

            ("Ryan Collins", 28),
            ("Cynthia Reyes", 52),
            ("Jacob Stewart", 3),
            ("Kathleen Morris", 17),
            ("Gary Morales", 84),
            ("Amy Murphy", 11),
            ("Nicholas Cook", 39),
            ("Shirley Rogers", 66),
            ("Eric Gutierrez", 5),
            ("Angela Ortiz", 24),
            ("Helen Cooper", 73),
            ("Stephen Peterson", 46),
            ("Anna Bailey", 13),
            ("Larry Reed", 29),
            ("Brenda Kelly", 55),
            ("Justin Howard", 2),
            ("Pamela Ramos", 32),
            ("Scott Kim", 80),
            ("Nicole Foster", 21),
            ("Brandon Ward", 43),
            ("Emma Cox", 67),
            ("Benjamin Diaz", 6),
            ("Samantha Richardson", 14),
            ("Samuel Wood", 35),
            ("Katherine Watson", 91),
            ("Gregory Brooks", 19),
            ("Christine Bennett", 48),
            ("Alexander Gray", 7),
            ("Debra James", 25),
            ("Patrick Reyes", 53),
            ("Rachel Cruz", 12),
            ("Frank Hughes", 70),
            ("Catherine Price", 4),
            ("Raymond Myers", 38),
            ("Carolyn Long", 62),
            ("Jack Foster", 16),
            ("Janet Sanders", 27),
            ("Dennis Ross", 9),
            ("Ruth Morales", 51),
            ("Ruth Ross", 89),
            ("Jerry Powell", 22),
            ("Olivia Sullivan", 100)
        };
        listUser.Sort((a, b) => b.star.CompareTo(a.star));


        for (int i = 0; i < listUser.Count; i++)
        {
            string myName = "Minh Hiếu 98";

            var newUser = listUser[i];
            int rank = i + 1;
            if (newUser.userName == myName)
            {
                currentRankTxt.text = rank.ToString();
            }

            if (rank == 1)
            {
                top1Prefab.SetData(newUser.userName, 1, newUser.star);
            }

            else if (rank == 2)
            {
                top2Prefab.SetData(newUser.userName, 2, newUser.star);
            }

            else if (rank == 3)
            {
                top3Prefab.SetData(newUser.userName, 3, newUser.star);
            }
            else
            {
                FrameDataPrefabs userNotTop = Instantiate(framePrefab, contentParent);
                userNotTop.SetData(newUser.userName, rank, newUser.star);
            }
        }
    }
}