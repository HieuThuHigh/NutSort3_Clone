using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS && USE_UNITY_NOTIFICATION
using Unity.Notifications.iOS;
#endif
using System;

public class MobileNotificationManagerIOS : MonoBehaviour
{
#if UNITY_IOS && USE_UNITY_NOTIFICATION
    public string notificationId = "test_notification";
    private int identifier;
    private iOSNotification notification;
    string[] titleArray = { "You Have A Gift", "I Miss You", "Many gifts are waiting for you", "Continue with the adventures ..." };
    void Start()
    {
        //if (GameData.Notification)
        //{
        //    PushNotification("Claim Free iPhone 11 Pro", "Get iphone 11 immediately", "", 120);
           
        //    PushNotification_RealHour("Daily Check-in", "It's time to check in and claim today's gifts", "", 8);
        //    int randomPos = UnityEngine.Random.Range(0, titleArray.Length);
        //    PushNotification_Span(titleArray[randomPos] + " for FREE!", "Have you claimed it yet? Hurry now!", "", 4);
        //}
        //else
        //{
        //    CancelNotification();
        //}

           

       

        iOSNotificationCenter.OnRemoteNotificationReceived += recievedNotification =>
         {
             Debug.Log("Recieved notification " + notification.Identifier + "!");
         };
        iOSNotification notificationIntentData = iOSNotificationCenter.GetLastRespondedNotification();
        if (notificationIntentData != null)
        {
            Debug.Log("App was opened with notification!");
        }

       // iOSNotificationCenter.RemoveDeliveredNotification(notification.Identifier);

    }
    

    //gửi thông báo sau time phút, chỉ gửi 1 lần khi gọi
    void PushNotification(string title, string body, string subTitle, int time)
    {
        iOSNotificationTimeIntervalTrigger timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, time, 0),
            Repeats = false
        };
        notification = new iOSNotification()
        {
            // You can specify a custom identifier which can be used to manage the notification later.
            // If you don't provide one, a unique string will be generated automatically.
            Identifier = "test_notification",
            Title = title,
            Body = body,
            Subtitle = subTitle,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };
        iOSNotificationCenter.ScheduleNotification(notification);

    }

    //Gửi thông báo sau time giờ, gửi nhiều lần
    void PushNotification_Span(string title, string body, string subTitle, int time)
    {
        iOSNotificationTimeIntervalTrigger timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(time, 0, 0),
            Repeats = true
        };
        notification = new iOSNotification()
        {
            // You can specify a custom identifier which can be used to manage the notification later.
            // If you don't provide one, a unique string will be generated automatically.
            Identifier = "test_notification",
            Title = title,
            Body = body,
            Subtitle = subTitle,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };
        iOSNotificationCenter.ScheduleNotification(notification);

    }

    //Gửi thông báo vào time giờ mỗi ngày 
    void PushNotification_RealHour(string title, string body, string subTitle, int time)
    {
        iOSNotificationCalendarTrigger calendarTrigger = new iOSNotificationCalendarTrigger()
        {
            Hour = time,
            Minute = 0,
            Repeats = true
        };
        notification = new iOSNotification()
        {
            // You can specify a custom identifier which can be used to manage the notification later.
            // If you don't provide one, a unique string will be generated automatically.
            Identifier = "test_notification",
            Title = title,
            Body = body,
            Subtitle = subTitle,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = calendarTrigger,
        };
        iOSNotificationCenter.ScheduleNotification(notification);

    }
    void CancelNotification()
    {
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
        iOSNotificationCenter.RemoveAllScheduledNotifications();
    }
    private void OnApplicationPause(bool pause)
    {
            PushNotification("Congrats!", "Check out all the achievements you have made>>>", "", 60*24);      
    }
#endif
}
