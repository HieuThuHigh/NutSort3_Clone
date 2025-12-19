using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "MobileNotificationConfig", menuName = "ScriptableObject/MobileNotificationConfig")]
public class MobileNotificationConfig : ScriptableObject
{
    public List<NotificationDay> notificationDays = new List<NotificationDay>();

}
[Serializable]
public class NotificationDay
{
    public int day;
    public List<NotificationContent> NotificationContents;
}

[Serializable]
public class NotificationContent
{
    public string title;
    public string text;
    public string largeIcon;
    public string smallIcon;
    public double pushTime;
}
