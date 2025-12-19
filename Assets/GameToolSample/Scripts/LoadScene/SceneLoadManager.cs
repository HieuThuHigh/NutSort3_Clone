using System;
using System.Collections;
using System.Collections.Generic;
using GameTool.APIs.Scripts;
using GameTool.Assistants.DesignPattern;
using GameTool.TransitionFX.Scripts;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using UnityEngine;
using UnityEngine.SceneManagement;

//using GameTool.ObjectPool;
#if USE_SPINE
//using GameTool.SpineAnimation;
#endif


namespace GameToolSample.Scripts.LoadScene
{
    public class SceneLoadManager : SingletonMonoBehaviour<SceneLoadManager>
    {
        [Header("COMPONENT")]
        [SerializeField] List<SceneInfo> listSceneInfo = new List<SceneInfo>();

        [Header("NAME SCENE")]
        public static string nameSceneSpl = "SPL";
        public static string nameSceneGamePlay = "GamePlay";
        public static string nameSceneHome = "Home";
        public static string nameSceneLevel = "Level_";

        //Active scene đã load tài nguyên nhưng bị ẩn, dùng khi cần active scene nào đó mà đã load trước
        public void SetActiveScene(string nameScene)
        {
            SceneInfo info = GetSceneInfo(nameScene);
            if (info != null)
            {
                SetConditionActiveScene(nameScene, true);
            }
        }

        /// <summary>
        /// Loadscene không đồng bộ, dùng để load trước scene
        /// </summary>
        /// <param name="_name">Tên scene</param>
        /// <param name="conditionActiveScene">Kiểm tra điều kiện có active scene luôn khi scene đã load xong hay không (false thì scene đã được load sẽ không active và ngược lại)</param>
        /// <param name="loadSceneMode"></param>
        /// <param name="useTransitionFX">Có sử dụng hiệu ứng chuyển cảnh hay không. Có thể dùng hàm này với điều kiện useTransitionFX = true để thay cho hàm SetActiveScene phía trên</param>
        /// <param name="delayTimeCallbackEndLoadScene"></param>
        public void LoadSceneWithName(string _name, bool conditionActiveScene = true, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool useTransitionFX = true, float delayTimeCallbackEndLoadScene = 0f)
        {
            if (!useTransitionFX)
            {
                SetConditionActiveScene(_name, conditionActiveScene);
                LoadAsyncScene(_name, loadSceneMode, delayTimeCallbackEndLoadScene);
            }
            else
            {
                ActiveTransitionLoadScene(() =>
                {
                    SceneInfo info = GetSceneInfo(_name);
                    if (info == null)
                    {
                        SetConditionActiveScene(_name, true);
                        info = GetSceneInfo(_name);
                        info.callbackEndLoadScene += () =>
                        {
                            DisableTransitionLoadScene();
                        };

                        LoadAsyncScene(_name, loadSceneMode, delayTimeCallbackEndLoadScene);
                    }
                    else
                    {
                        SetConditionActiveScene(_name, true);

                        info.callbackEndLoadScene += () =>
                        {
                            DisableTransitionLoadScene();
                        };
                    }
                });
            }
        }

        public void LoadCurrentScene(bool conditionActiveScene = true, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool useTransitionFX = true, float delayTimeCallbackEndLoadScene = 0f)
        {
            string _name = SceneManager.GetActiveScene().name;

            // SetConditionActiveScene(_name, conditionActiveScene);
            LoadSceneWithName(_name, conditionActiveScene, loadSceneMode, useTransitionFX, delayTimeCallbackEndLoadScene);
        }

        public void LoadSceneHome(bool conditionActiveScene = true, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool useTransitionFX = true, float delayTimeCallbackEndLoadScene = 0f)
        {
            LoadSceneWithName(nameSceneHome, conditionActiveScene, loadSceneMode, useTransitionFX, delayTimeCallbackEndLoadScene);
        }

        public void LoadSceneGamePlay(bool conditionActiveScene = true, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool useTransitionFX = true, float delayTimeCallbackEndLoadScene = 0f)
        {
            LoadSceneWithName(nameSceneGamePlay, conditionActiveScene, loadSceneMode, useTransitionFX, delayTimeCallbackEndLoadScene);
        }

        public void LoadSceneLevel(int level, bool conditionActiveScene = true, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool useTransitionFX = true, float delayTimeCallbackEndLoadScene = 0f)
        {
            LoadSceneWithName(nameSceneLevel + level, conditionActiveScene, loadSceneMode, useTransitionFX, delayTimeCallbackEndLoadScene);
        }

        public bool HaveScene(string _name)
        {
            return SceneManager.GetSceneByName(_name) is { };
        }

        //Các điều kiện xử lý trước khi active scene
        void SetBeforeActiveScene()
        {
            //Bỏ comment nếu có sử dụng

            //DOTween.Clear();

            //if (MixSkinManager.IsInstanceValid())
            //{
            //    MixSkinManager.Instance.StopCoroutine();
            //}

            //if (PoolingManager.IsInstanceValid())
            //{
            //    PoolingManager.Instance.DisableAllObject();
            //}

            if (GameData.IsInstanceValid())
            {
                GameData.Instance.SetDataFake();
            }

            this.PostEvent(EventID.ResetTime);
        }

        //Các điều kiện xử lý sau khi active scene
        void SetAfterActiveScene()
        {
            //this.PostEvent(EventID.UpdateCanvas);
        }

        //Dành riêng cho màn hình SPL
        #region CONFIG SPL
        /// <summary>
        /// Load trước cảnh khi bắt đầu scene SPL
        /// </summary>
        public void LoadSceneStart()
        {
            string nameScene = nameSceneHome;

            if (GameTool.APIs.Scripts.API.Instance.IsFirstOpenTime)
            {
                
            }

            LoadSceneWithName(nameScene);
        }

        /// <summary>
        /// Kích hoạt cảnh đã load trước ở SPL khi đã đủ điều kiện
        /// Chú ý xử lý điều kiện phải khớp với LoadSceneStart. Load scene tên là gì thì active scene cũng phải tên như vậy
        /// </summary>
        public void ActiveSceneStart()
        {
            string nameScene = nameSceneHome;

            if (GameTool.APIs.Scripts.API.Instance.IsFirstOpenTime)
            {
                nameScene = nameSceneLevel + GameData.Instance.CurrentLevel;
            }

            SetActiveScene(nameScene);
        }
        #endregion

        #region TRANSITION
        /// <summary>
        /// Kích hoạt hiệu ứng chuyển cảnh
        /// </summary>
        /// <param name="callBack">Gọi lại khi hiệu ứng chuyển cảnh bắt đầu</param>
        public void ActiveTransitionLoadScene(Action callBack = null)
        {
            if (LoadingTransitionFX.IsInstanceValid())
            {
                LoadingTransitionFX.Instance.ActiveLoading(callBack);
            }
        }

        /// <summary>
        /// Tắt hiệu ứng chuyển cảnh
        /// </summary>
        /// <param name="callBack">Gọi lại khi hiệu ứng chuyển cảnh kết thúc</param>
        public void DisableTransitionLoadScene(Action callBack = null)
        {
            if (LoadingTransitionFX.IsInstanceValid())
            {
                LoadingTransitionFX.Instance.DisableLoading(callBack);
            }
        }
        #endregion

        #region LOAD SCENE ASYNC
        private void LoadAsyncScene(string _name, LoadSceneMode loadSceneMode = LoadSceneMode.Single, float delayTimeCallbackEndLoadScene = 0f)
        {
            SceneInfo info = GetSceneInfo(_name);
            if (info.loadingOperation != null)
            {
                return;
            }

            StartCoroutine(WaitLoadAsyncScene(_name, loadSceneMode, delayTimeCallbackEndLoadScene));
        }

        private IEnumerator WaitLoadAsyncScene(string _name, LoadSceneMode loadSceneMode = LoadSceneMode.Single, float delayTimeCallbackEndLoadScene = 0f)
        {
            SceneInfo info = GetSceneInfo(_name);

            Debug.Log($"Scene {_name} is loading...");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_name, loadSceneMode);
            if (asyncLoad == null)
            {
                Debug.LogError($"Scene {_name} is Null");
                yield break;
            }

            info.loadingOperation = asyncLoad;

            asyncLoad.allowSceneActivation = false;

            info.callbackStartLoadScene?.Invoke();

            while (!asyncLoad.isDone)
            {
                if (asyncLoad.progress >= 0.9f && info.conditionActiveScene)
                {
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            yield return new WaitForSecondsRealtime(delayTimeCallbackEndLoadScene);
            info.callbackEndLoadScene?.Invoke();
            RemoveSceneInfo(_name);
        }
        #endregion

        #region UNLOAD SCENE ASYNC
        public void UnloadSceneWithName(string _name, Action callBack = null)
        {
            UnloadAsyncScene(_name, callBack);
        }

        private void UnloadAsyncScene(string _name, Action callBack = null)
        {
            // Kiểm tra xem cảnh có tồn tại và chưa được kích hoạt hay không
            Scene targetScene = SceneManager.GetSceneByName(_name);

            if (targetScene is { isLoaded: true, isDirty: false })
            {
                StartCoroutine(WaitUnloadScene(_name, callBack));
            }
            else
            {
                Debug.LogError($"Scene {_name} does not exist or has been activated");
            }
        }

        private IEnumerator WaitUnloadScene(string _name, Action callBack = null)
        {
            RemoveSceneInfo(_name);

            AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(_name);

            if (asyncLoad == null)
            {
                Debug.LogError($"Scene {_name} is Null");
                yield break;
            }

            while (!asyncLoad.isDone) yield return null;

            callBack?.Invoke();

            Debug.Log($"Scene {_name} is unloaded!!!");
        }
        #endregion

        #region SCENE INFO
        SceneInfo GetSceneInfo(string nameScene)
        {
            for (int i = 0; i < listSceneInfo.Count; i++)
            {
                SceneInfo info = listSceneInfo[i];

                if (info.sceneName.Contains(nameScene))
                {
                    return info;
                }
            }

            return null;
        }

        //Điều kiện active scene
        void SetConditionActiveScene(string nameScene, bool conditionActiveScene)
        {
            if (conditionActiveScene)
            {
                SetBeforeActiveScene();
            }

            for (int i = 0; i < listSceneInfo.Count; i++)
            {
                SceneInfo info = listSceneInfo[i];

                if (info.sceneName.Contains(nameScene))
                {
                    if (conditionActiveScene)
                    {
                        info.callbackEndLoadScene = SetAfterActiveScene;
                    }
                    info.conditionActiveScene = conditionActiveScene;
                    return;
                }
            }

            SceneInfo newInfo = new SceneInfo
            {
                sceneName = nameScene,
                conditionActiveScene = conditionActiveScene,
                callbackStartLoadScene = null,
                callbackEndLoadScene = null
            };

            if (conditionActiveScene)
            {
                newInfo.callbackEndLoadScene = SetAfterActiveScene;
            }

            listSceneInfo.Add(newInfo);
        }

        void RemoveSceneInfo(string nameScene)
        {
            for (int i = listSceneInfo.Count - 1; i >= 0; i--)
            {
                SceneInfo info = listSceneInfo[i];

                if (info.sceneName.Contains(nameScene))
                {
                    listSceneInfo.Remove(info);
                    return;
                }
            }
        }

        [Serializable]
        public class SceneInfo
        {
            public string sceneName = "";
            public bool conditionActiveScene;
            public AsyncOperation loadingOperation;
            public Action callbackStartLoadScene;
            public Action callbackEndLoadScene;
        }
        #endregion
    }
}