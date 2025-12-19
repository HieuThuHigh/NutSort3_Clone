#if !Minify
using System.Collections.Generic;

namespace GameTool.APIs.Analytics
{
    namespace Analytics
    {
        public interface IAnalytic
        {
            bool InitSuccess { get; set; }

            void Init(params string[] args);

            void TrackEvent(string eventName);
            void TrackEvent(string eventName, Dictionary<string, object> parameters);

            void ApplicationOnPause(bool Paused);
        }
    }
}

#endif