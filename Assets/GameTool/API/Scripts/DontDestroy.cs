using GameTool.Assistants.DesignPattern;

namespace GameTool.APIs.Scripts
{
    public class DontDestroy:SingletonMonoBehaviour<DontDestroy>
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
        }
    }
}
