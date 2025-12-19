#if !Minify
using UnityEngine;

#if USE_ADVERTY
        using Adverty;
#endif
#endif

namespace GameTool.APIs.Scripts.Ads
{
    public class AdvertyManager : MonoBehaviour
    {
#if !Minify
        // Start is called before the first frame update
        void Start()
        {
#if USE_ADVERTY
            UserData userData = new UserData(AgeSegment.Unknown, Gender.Unknown);
            Adverty.AdvertySDK.Init(userData);
#endif
        }
#endif
    }
}
