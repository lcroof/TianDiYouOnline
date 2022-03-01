using System;
using System.Collections.Generic;
using System.Text;

namespace DataPublic
{
    public enum MessageLevel
    {
        Hint,
        Alarm,
        Error,
        Exception,
        SpcAlarm,
        ChangePN,
        AlarmPic
    }

    public enum RequestCode
    {
        Room,
        Chat,
        Card
    }

    public enum ActionCode
    {
        EnterRoom,
        LeaveRoom,
        CreateRoom,
        Chat,
        ShowCard,
        ConfirmCard,
        AddCard,
        RemoveCard
    }
}
