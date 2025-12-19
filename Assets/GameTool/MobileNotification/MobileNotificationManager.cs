using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_UNITY_NOTIFICATION
#if UNITY_ANDROID
using GameToolSample.GameDataScripts.Scripts;
using Unity.Notifications.Android;
using UnityEngine.Android;

#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
#endif

public class MobileNotificationManager : MonoBehaviour
{
#if USE_UNITY_NOTIFICATION
    [SerializeField] private MobileNotificationConfig mobileNotificationConfig;
    [SerializeField] private bool clearOnStart = true;
    [SerializeField] private int maxDayRetension = 365;

    private void Start()
    {
        StartCoroutine(InitNotificationSystem());
    }

    private IEnumerator InitNotificationSystem()
    {
#if UNITY_ANDROID
        // Android 13+ requires permission
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            yield return new WaitForSecondsRealtime(1f);
        }
#elif UNITY_IOS
        // Request permission on iOS
        using (var req =
 new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            }

            if (!req.Granted)
            {
                Debug.LogWarning("Notification permission not granted on iOS.");
                yield break;
            }
        }
#endif

        if (clearOnStart)
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        }

        yield return new WaitForSecondsRealtime(3);
        ScheduleNotificationsFromConfig();
    }

    private void ScheduleNotificationsFromConfig()
    {
        if (mobileNotificationConfig == null)
        {
            Debug.LogWarning("MobileNotificationConfig is not assigned.");
            return;
        }

        var dayRetension = GameData.Instance.DayRetension;
        for (var i = 0; i < maxDayRetension; i++)
        {
            var day = GameData.Instance.DayRetension + i;
            var datePush = DateTime.Now.Date.AddDays(i);
            NotificationDay notificationDay;
            if (datePush.DayOfWeek == DayOfWeek.Saturday)
            {
                notificationDay = mobileNotificationConfig.notificationDays[9];
            }
            else if (datePush.DayOfWeek == DayOfWeek.Sunday)
            {
                notificationDay = mobileNotificationConfig.notificationDays[10];
            }
            else if (day > 7)
            {
                notificationDay = mobileNotificationConfig.notificationDays[8];
            }
            else
            {
                notificationDay = mobileNotificationConfig.notificationDays[day];
            }

            var timeApart = notificationDay.day - dayRetension;
            TimeSpan timeApartAccept = TimeSpan.Zero;
            bool settedTimeApartAccept = false;

            for (var index = notificationDay.NotificationContents.Count - 1; index >= 0; index--)
            {
                var notificationContent = notificationDay.NotificationContents[index];
                if (notificationDay.checkTimeApart)
                {
                    if (timeApart < new TimeSpan(notificationContent.timeApart).Days)
                    {
                        continue;
                    }

                    if (settedTimeApartAccept)
                    {
                        if (new TimeSpan(notificationContent.timeApart) != timeApartAccept)
                        {
                            continue;
                        }
                    }

                    timeApartAccept = new TimeSpan(notificationContent.timeApart);

                    settedTimeApartAccept = true;
                }

                var fireTime = datePush
                    .AddTicks(notificationContent.pushTime);
                
                SendNotification(notificationContent.title, notificationContent.text, fireTime,
                    notificationContent.smallIcon, notificationContent.largeIcon);
            }
        }
    }

    /// <summary>
    /// Gửi thông báo tùy chỉnh.
    /// </summary>
    /// <param name="title">Tiêu đề thông báo.</param>
    /// <param name="text">Nội dung thông báo.</param>
    /// <param name="fireTime">Thời gian gửi thông báo.</param>
    /// <param name="smallIcon">Tên biểu tượng nhỏ (Android).</param>
    /// <param name="largeIcon">Tên biểu tượng lớn (Android).</param>
    /// <param name="repeatInterval">Khoảng thời gian lặp lại (nếu có).</param>
    public void SendNotification(string title, string text, DateTime fireTime, string smallIcon = null,
        string largeIcon = null, TimeSpan? repeatInterval = null)
    {
        if (fireTime < DateTime.Now.Add(TimeSpan.FromSeconds(30)))
        {
            return;
        }
        
#if UNITY_ANDROID
        // Đăng ký kênh thông báo nếu chưa có
        var channel = new AndroidNotificationChannel
        {
            Id = "default_channel",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Generic notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        var notification = new AndroidNotification
        {
            Title = title,
            Text = text,
            FireTime = fireTime,
            SmallIcon = smallIcon,
            LargeIcon = largeIcon,
            RepeatInterval = repeatInterval
        };

        AndroidNotificationCenter.SendNotification(notification, "default_channel");
#elif UNITY_IOS
        var iosNotification = new iOSNotification
        {
            Identifier = Guid.NewGuid().ToString(),
            Title = title,
            Body = text,
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = new iOSNotificationCalendarTrigger
            {
                Year = fireTime.Year,
                Month = fireTime.Month,
                Day = fireTime.Day,
                Hour = fireTime.Hour,
                Minute = fireTime.Minute,
                Second = fireTime.Second,
                Repeats = repeatInterval.HasValue
            }
        };

        iOSNotificationCenter.ScheduleNotification(iosNotification);
#endif

        Debug.Log($"[NOTIFICATION] Scheduled notification: {title}\n{text}\nat {fireTime}\nrepeat: {repeatInterval}");
    }
#endif
}