using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameTool.Assistants.DesignPattern;
using UnityEngine;
using UnityEngine.Networking;

namespace DatdevUlts.DateTimeScripts
{
    /// <summary>
    /// Network time - chạy không đồng bộ
    /// </summary>
    public class DateTimeManager : SingletonMonoBehaviour<DateTimeManager>
    {
        public enum TypeRequest
        {
            [Tooltip("Sử dụng thời gian local của máy")]
            Local,

            [Tooltip("Sử dụng phương thức NTP")] NTP,

            [Tooltip("Sử dụng phương thức Https")] Https,

            [Tooltip("Mix giữa phương thức NTP và Https")]
            NetworkMixed,
        }

        [SerializeField] private TypeRequest _typeRequestValue;

        [Space] [Tooltip("Nếu quá số lần MaxRequestTimes, thì dùng thời gian local")] [SerializeField]
        private bool _useLocalDateTimeOnNetError;

        [SerializeField] private int _maxRequestTimes = 5;
        private int _currentRequestTimes;

        [SerializeField] private List<string> https = new List<string>()
        {
            "https://www.google.com/", "https://www.yahoo.com", "https://www.msdn.com"
        };

        [SerializeField] private List<string> ntpLinks = new List<string>()
        {
            "time-a-g.nist.gov", "pool.ntp.org", "time.windows.com", "time.google.com", "time.cloudflare.com",
            "time.apple.com"
        };

        [Tooltip("Thời gian tối đa để request, tính bằng Milisecond")]
        [SerializeField] private int _timeOutPerRequest = 500;

        /// <summary>
        /// Event bắn mỗi giây
        /// </summary>
        public event Action OneSecondUpdateEvent;

        public event Action OnLostConnectTimeEvent;
        public event Action OnReConnectedTimeEvent;
        public event Action OnReConnectingTimeEvent;
        public event Action OnConnectTimeFailedEvent;


        private int indexHttps;
        private int indexNtp;

        private CallbackUlts _callbackProtector = new CallbackUlts();

        public TypeRequest TypeRequestValue => _typeRequestValue;

        public bool UseLocalDateTimeOnNetError => _useLocalDateTimeOnNetError;

        protected override void Awake()
        {
            base.Awake();
            ReConnectTime();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Debug.Log($"TimeManager: app focus - {hasFocus}");

            LoadedTime = false;
            OnLostConnectTimeEvent?.Invoke();

            if (hasFocus)
            {
                RequestTime();
            }
        }

        public void ReConnectTime()
        {
            LoadedTime = false;
            OnLostConnectTimeEvent?.Invoke();

            RequestTime();
        }

        /// <summary>
        /// Hàm này tự động được gọi lúc Awake hoặc ApplicationFocus
        /// </summary>
        private void RequestTime()
        {
            if (_typeRequestValue != TypeRequest.Local)
            {
                if (_typeRequestValue == TypeRequest.NTP || _typeRequestValue == TypeRequest.NetworkMixed)
                {
                    StartRequestNTP();
                }
                else
                {
                    StartRequestHttps();
                }
            }
        }

        private void StartRequestHttps()
        {
            CancleAllTaskBefore();

            var keyProtect = _callbackProtector.CurrentKeyProtect;
            RequestTimeHttps(keyProtect);
        }


        private void StartRequestNTP()
        {
            CancleAllTaskBefore();

            var protectKey = _callbackProtector.CurrentKeyProtect;
            RequestTimeNTP(protectKey);
        }


        private void CancleAllTaskBefore()
        {
            _callbackProtector.CancelAllCallbackNonProtect();
        }


        private float _time;
        private bool _loadedTime;

        private DateTime _nowNetWorkTime;

        public DateTime Now
        {
            get
            {
                if (_typeRequestValue != TypeRequest.Local)
                {
                    return _nowNetWorkTime;
                }

                return DateTime.Now;
            }
        }

        public DateTime Today
        {
            get
            {
                var now = Now;
                return new DateTime(now.Year, now.Month, now.Day);
            }
        }

        /// <summary>
        /// True nếu thành công request, trả về true
        /// </summary>
        public bool LoadedTime
        {
            get => _loadedTime;
            private set => _loadedTime = value;
        }


        private void RequestTimeHttps(int keyProtect)
        {
            SetupReconnect();
            DateTime netDateTime = DateTime.Now;
            bool getted = true;


            var http = https[indexHttps];
            var webRequest = UnityWebRequest.Get(http);
            webRequest.timeout = _timeOutPerRequest;
            var task = webRequest.SendWebRequest();
            task.completed += _ =>
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var todaysDates = webRequest.GetResponseHeader("date");
                        netDateTime = DateTime.ParseExact(todaysDates, "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                            DateTimeFormatInfo.CurrentInfo);
                    }
                    catch (Exception e)
                    {
                        getted = false;
                        Debug.Log($"TimeManager - Request failed - link: {http} - message: {e}");
                    }
                }
                else
                {
                    getted = false;
                    Debug.Log($"TimeManager - Request failed - link: {http} - Web result: {webRequest.result}");
                }
                
                _callbackProtector.CallNonProtect(() => OnDoneRequest(true, getted, netDateTime, http), keyProtect);
            };
        }


        private void RequestTimeNTP(int protectKey)
        {
            SetupReconnect();
            var getted = true;
            DateTime netDateTime = DateTime.Now;

            string ntpServer = ntpLinks[indexNtp];

            Task.Run(() =>
            {
                try
                {
                    var ntpData = new byte[48];
                    ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

                    var addresses = Dns.GetHostEntry(ntpServer).AddressList;
                    var ipEndPoint = new IPEndPoint(addresses[0], 123);
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    socket.Connect(ipEndPoint);
                    socket.ReceiveTimeout = _timeOutPerRequest;
                    socket.Send(ntpData);
                    socket.Receive(ntpData);
                    socket.Close();

                    ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 |
                                    ntpData[43];
                    ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 |
                                      ntpData[47];

                    var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                    netDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
                }
                catch (Exception ex)
                {
                    Debug.Log($"TimeManager - Request failed - link: {ntpServer} - message: {ex}");
                    getted = false;
                }

                _callbackProtector.CallNonProtect(()=>OnDoneRequest(false, getted, netDateTime, ntpServer), protectKey);
            });
        }

        private void OnDoneRequest(bool isHttp, bool getted, DateTime netDateTime, string link)
        {
            if (isHttp)
            {
                indexHttps++;
                if (indexHttps >= https.Count)
                {
                    indexHttps = 0;
                }


                if (!getted)
                {
                    if (_currentRequestTimes < _maxRequestTimes)
                    {
                        if (_typeRequestValue == TypeRequest.NetworkMixed)
                        {
                            StartRequestNTP();
                        }
                        else
                        {
                            StartRequestHttps();
                        }

                        _currentRequestTimes++;
                        return;
                    }

                    if (!_useLocalDateTimeOnNetError)
                    {
                        OnConnectTimeFailedEvent?.Invoke();
                        return;
                    }
                }


                indexHttps = 0;
            }
            else
            {
                indexNtp++;
                if (indexNtp >= ntpLinks.Count)
                {
                    indexNtp = 0;
                }


                // Nếu không get được thì để frame sau chạy lại
                if (!getted)
                {
                    if (_currentRequestTimes < _maxRequestTimes)
                    {
                        if (_typeRequestValue == TypeRequest.NetworkMixed)
                        {
                            StartRequestHttps();
                        }
                        else
                        {
                            StartRequestNTP();
                        }

                        _currentRequestTimes++;
                        return;
                    }

                    if (!_useLocalDateTimeOnNetError)
                    {
                        OnConnectTimeFailedEvent?.Invoke();
                        return;
                    }
                }
                
                indexNtp = 0;
            }
            
            Debug.Log(
                $"TimeManager - DateTime requested: {netDateTime.ToString(DateTimeFormatInfo.CurrentInfo)} - link: {link}");
            SetTime(netDateTime);
        }


        private void SetupReconnect()
        {
            OnReConnectingTimeEvent?.Invoke();
        }


        private void SetTime(DateTime netDateTime)
        {
            _nowNetWorkTime = netDateTime;
            LoadedTime = true;
            OnReConnectedTimeEvent?.Invoke();
        }


        private void Update()
        {
            var unscaledDeltaTime = Time.unscaledDeltaTime;
            _nowNetWorkTime = _nowNetWorkTime.AddSeconds(unscaledDeltaTime);

            if (OneSecondUpdateEvent != null)
            {
                _time += unscaledDeltaTime;
                if (_time >= 1)
                {
                    _time -= 1;
                    OneSecondUpdateEvent.Invoke();
                }
            }
        }
    }
}