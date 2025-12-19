using UnityEngine;
using UnityEngine.UI;

namespace GameTool.UI.Scripts.ItemUI
{
    [RequireComponent(typeof(Button))]
    public class AndroidBackButton : MonoBehaviour
    {

        Button button;

        void Awake()
        {
            button = GetComponent<Button>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                button.onClick.Invoke();
        }
    }
}
