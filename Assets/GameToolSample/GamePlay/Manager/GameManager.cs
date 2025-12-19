#if !Minify
using System;
using System.Collections.Generic;
using DatdevUlts.Ults;

#if USE_FALCON
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
#endif

using GameTool.APIs.Analytics.Analytics;
using GameTool.Assistants.DesignPattern;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using GameToolSample.UIManager;
using UnityEngine;
using static GameToolSample.Scripts.Enum.AnalyticID;

namespace GameToolSample.GamePlay.Manager
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        [Header("COMPONENT")] [SerializeField] private GameModeName _gameMode = GameModeName.none;
        [SerializeField] private GamePlayState _gameplayStatus = GamePlayState.none;
        private float _timePlay;
        private float _timeStart;

        public GameModeName GameMode
        {
            get => _gameMode;
            set => _gameMode = value;
        }

        public GamePlayState GameplayStatus
        {
            get => _gameplayStatus;
            set => _gameplayStatus = value;
        }


        [Header("START")] [Tooltip("-1 is disable")] [SerializeField]
        private int _autoStartDelay = -1;

        public int AutoStartDelay
        {
            get => _autoStartDelay;
            set => _autoStartDelay = value;
        }

        [Header("VICTORY")] [Tooltip("-1 is disable")] [SerializeField]
        private float _delayShowPopupVictory;

        [SerializeField] private eUIName _winPopup = eUIName.VictoryPopup;

        public float DelayShowPopupVictory
        {
            get => _delayShowPopupVictory;
            set => _delayShowPopupVictory = value;
        }


        [Header("LOSE")] [Tooltip("-1 is disable")] [SerializeField]
        private float _delayShowPopupLose;

        [SerializeField] private eUIName _losePopup = eUIName.LosePopup;
        private float _delayShowPopupLoseAfterDenyRevive;

        public float DelayShowPopupLose
        {
            get => _delayShowPopupLose;
            set => _delayShowPopupLose = value;
        }

        public float DelayShowPopupLoseAfterDenyRevive
        {
            get => _delayShowPopupLoseAfterDenyRevive;
            set => _delayShowPopupLoseAfterDenyRevive = value;
        }


        [Header("REVIVE")] [SerializeField] private bool _canRevive = true;
        [SerializeField] private eUIName _revivePopup = eUIName.RevivePopup;

        [Tooltip("-1 is infinity")] [SerializeField]
        private int _amountRevive = -1;

        [Tooltip("-1 is disable")] [SerializeField]
        private float _delayShowPopupRevive;


        public bool CanRevive
        {
            get => _canRevive && _amountRevive != 0;
            set => _canRevive = value;
        }

        public int AmountRevive
        {
            get => _amountRevive;
            set => _amountRevive = value;
        }

        public float DelayShowPopupRevive
        {
            get => _delayShowPopupRevive;
            set => _delayShowPopupRevive = value;
        }

        private int _currentLevel;
        private LevelPlayInfoData _levelPlayData;

        public int CurrentLevel
        {
            get => _currentLevel;
            set => _currentLevel = value;
        }

        public LevelPlayInfoData LevelPlayData
        {
            get => _levelPlayData;
            set => _levelPlayData = value;
        }


        protected override void Awake()
        {
            base.Awake();
            _currentLevel = GameData.Instance.CurrentLevel;
            _levelPlayData = GameData.Instance.GetLevelPlayInfoData(new LevelPlayInfoKey(){gameMode = _gameMode, level = _currentLevel});
            
            TrackingManager.Instance.TrackFirebaseEvent($"game_{_gameMode}_prestart_{_currentLevel}");

            if (_autoStartDelay >= 0)
            {
                this.DelayedCall(_autoStartDelay, PlayGame);
            }
        }

        public bool IsGamePlayStatus(GamePlayState gamePlayState)
        {
            return _gameplayStatus == gamePlayState;
        }

        #region STATUS INGAME

        public virtual void PlayGame()
        {
            _gameplayStatus = GamePlayState.playing;
            _timePlay = 0;
            _timeStart = Time.time;
            TrackingPlaygame();
        }

        public void CalculateTimePlay()
        {
            _timePlay = Time.time - _timeStart;
        }

        public virtual void Victory()
        {
            _gameplayStatus = GamePlayState.victory;
            GameData.Instance.CurrentLevel++;
            CalculateTimePlay();
            ShowVictoryPopup();
            TrackingVictory();
        }

        protected virtual void ShowVictoryPopup()
        {
            if (DelayShowPopupVictory >= 0)
            {
                this.DelayedCall(DelayShowPopupVictory, () => { CanvasManager.Instance.Push(_winPopup); });
            }
        }

        public virtual void Lose()
        {
            if (CanRevive)
            {
                CheckRevive();
            }
            else
            {
                _gameplayStatus = GamePlayState.lose;
                CalculateTimePlay();
                ShowLosePopup();
                TrackingLose();
            }
        }

        protected virtual void ShowLosePopup()
        {
            if (DelayShowPopupLose >= 0)
            {
                this.DelayedCall(DelayShowPopupLose, () => { CanvasManager.Instance.Push(_losePopup); });
            }
        }

        public virtual void CheckRevive()
        {
            if (_gameplayStatus == GamePlayState.revive) return;
            _gameplayStatus = GamePlayState.revive;
            ShowRevivePopup();
            TrackingLoseCanRevive();
        }

        public virtual void Revive()
        {
            _gameplayStatus = GamePlayState.playing;
            _amountRevive--;
            this.PostEvent(EventID.PlayerRevive);
            TrackingRevive();
        }

        protected virtual void ShowRevivePopup()
        {
            if (DelayShowPopupRevive >= 0)
            {
                this.DelayedCall(DelayShowPopupRevive, () => { CanvasManager.Instance.Push(_revivePopup); });
            }
        }

        public virtual void SkipLevel(LocationTracking location = LocationTracking.gameplay)
        {
            CalculateTimePlay();
            GameData.Instance.CurrentLevel++;

            TrackingSkip(location);
        }

        public virtual void ReplayLevel(LocationTracking location = LocationTracking.gameplay)
        {
            CalculateTimePlay();
            TrackingReplay(location);
        }

        public void DenyRevive()
        {
            CanRevive = false;
            DelayShowPopupLose = DelayShowPopupLoseAfterDenyRevive;
            Lose();
        }

        #endregion

        #region TRACKING

        protected virtual void TrackingPlaygame()
        {
            TrackingManager.Instance.TrackFirebaseEvent($"game_{_gameMode}_start_{_currentLevel}");

#if USE_FALCON
            new FFilteredFunnelLog($"Level_drop_{_gameMode}", $"start", 0).Send();
            new FFilteredFunnelLog($"Level_drop_{_gameMode}", $"game_start_{_currentLevel}", (_currentLevel - 1) * 2 + 1).Send();
#endif
            
            if (_levelPlayData.playCount <= 0)
            {
                TrackingManager.Instance.TrackLevelFirstPlay(_currentLevel);
            }
            else
            {
                TrackingManager.Instance.TrackLevelStartCostCenter(_currentLevel);
            }

            TrackingManager.Instance.TrackGamePlay(GamePlayEvent.game_start, _currentLevel, _gameMode,
                new Dictionary<string, object>()
                {
                    {
                        GamePlayParam.location.ToString(), LocationTracking.gameplay
                    },
                    {
                        GamePlayParam.state.ToString(), GamePlayState.playing
                    }
                });

            _levelPlayData.playCount++;

            GameData.Instance.SetLevelPlayInfoData(_levelPlayData);
        }

        protected virtual void TrackingVictory()
        {
            TrackingManager.Instance.TrackFirebaseEvent($"game_{_gameMode}_victory_{_currentLevel}");
            
#if USE_FALCON
            new FFunnelLog($"Level_drop_{_gameMode}", $"game_victory_{_currentLevel}", (_currentLevel - 1) * 2 + 2).Send();
#endif
            
            if (_levelPlayData.victoryCount <= 0)
            {
#if USE_FALCON
                new FLevelLog(_currentLevel, "none", LevelStatus.Pass, TimeSpan.FromSeconds(_timePlay)).Send();
#endif
                TrackingManager.Instance.TrackLevelFirstVictory(_currentLevel);
            }
            else
            {
#if USE_FALCON
                new FLevelLog(_currentLevel, "none", LevelStatus.ReplayPass, TimeSpan.FromSeconds(_timePlay)).Send();
#endif
                TrackingManager.Instance.TrackLevelEndCostCenter(_currentLevel, isSuccess: true);
            }

            TrackingManager.Instance.TrackGamePlay(GamePlayEvent.game_end, _currentLevel, _gameMode,
                new Dictionary<string, object>()
                {
                    {
                        GamePlayParam.location.ToString(), LocationTracking.gameplay
                    },
                    {
                        GamePlayParam.state.ToString(), GamePlayState.victory
                    }
                });

            _levelPlayData.victoryCount++;

            GameData.Instance.SetLevelPlayInfoData(_levelPlayData);
        }

        protected virtual void TrackingLose()
        {
            TrackingManager.Instance.TrackFirebaseEvent($"game_{_gameMode}_lose_{_currentLevel}");
            
            if (_levelPlayData.loseCount <= 0)
            {
#if USE_FALCON
                new FLevelLog(_currentLevel, "none", LevelStatus.Fail, TimeSpan.FromSeconds(_timePlay)).Send();
#endif
                TrackingManager.Instance.TrackLevelFirstLose(_currentLevel);
            }
            else
            {
#if USE_FALCON
                new FLevelLog(_currentLevel, "none", LevelStatus.ReplayFail, TimeSpan.FromSeconds(_timePlay)).Send();
#endif
                TrackingManager.Instance.TrackLevelEndCostCenter(_currentLevel);
            }

            TrackingManager.Instance.TrackGamePlay(GamePlayEvent.game_end, _currentLevel, _gameMode,
                new Dictionary<string, object>()
                {
                    {
                        GamePlayParam.location.ToString(), LocationTracking.gameplay
                    },
                    {
                        GamePlayParam.state.ToString(), GamePlayState.lose
                    }
                });

            _levelPlayData.loseCount++;

            GameData.Instance.SetLevelPlayInfoData(_levelPlayData);
        }

        protected virtual void TrackingLoseCanRevive()
        {
            TrackingManager.Instance.TrackFirebaseEvent($"game_{_gameMode}_losecanrevive_{_currentLevel}");
            
            TrackingManager.Instance.TrackGamePlay(GamePlayEvent.game_playing, _currentLevel, _gameMode,
                new Dictionary<string, object>()
                {
                    {
                        GamePlayParam.location.ToString(), LocationTracking.gameplay
                    },
                    {
                        GamePlayParam.state.ToString(), GamePlayState.die
                    }
                });

            _levelPlayData.deadCount++;

            GameData.Instance.SetLevelPlayInfoData(_levelPlayData);
        }

        protected virtual void TrackingRevive()
        {
            TrackingManager.Instance.TrackFirebaseEvent($"game_{_gameMode}_revive_{_currentLevel}");
            
            TrackingManager.Instance.TrackGamePlay(GamePlayEvent.game_playing, _currentLevel, _gameMode,
                new Dictionary<string, object>()
                {
                    {
                        GamePlayParam.location.ToString(), LocationTracking.gameplay
                    },
                    {
                        GamePlayParam.state.ToString(), GamePlayState.revive
                    }
                });
        }

        protected virtual void TrackingSkip(LocationTracking location = LocationTracking.gameplay)
        {
#if USE_FALCON
            new FLevelLog(_currentLevel, "none", LevelStatus.Skip, TimeSpan.FromSeconds(_timePlay)).Send();
#endif
            
            TrackingManager.Instance.TrackFirebaseEvent($"game_{_gameMode}_skip_{_currentLevel}");
            
            TrackingManager.Instance.TrackLevelEndCostCenter(_currentLevel, isSuccess: true);

            TrackingManager.Instance.TrackGamePlay(GamePlayEvent.game_end, _currentLevel, _gameMode,
                new Dictionary<string, object>()
                {
                    {
                        GamePlayParam.location.ToString(), LocationTracking.gameplay
                    },
                    {
                        GamePlayParam.state.ToString(), GamePlayState.skip
                    }
                });

            _levelPlayData.skipCount++;

            GameData.Instance.SetLevelPlayInfoData(_levelPlayData);
        }

        protected virtual void TrackingReplay(LocationTracking location = LocationTracking.gameplay)
        {
            TrackingManager.Instance.TrackFirebaseEvent($"game_{_gameMode}_replay_{_currentLevel}");

            TrackingManager.Instance.TrackLevelEndCostCenter(_currentLevel);

            TrackingManager.Instance.TrackGamePlay(GamePlayEvent.game_end, _currentLevel, _gameMode,
                new Dictionary<string, object>()
                {
                    {
                        GamePlayParam.location.ToString(), LocationTracking.gameplay
                    },
                    {
                        GamePlayParam.state.ToString(), GamePlayState.replay
                    }
                });

            _levelPlayData.replayCount++;

            GameData.Instance.SetLevelPlayInfoData(_levelPlayData);
        }

        #endregion
    }
}

#endif