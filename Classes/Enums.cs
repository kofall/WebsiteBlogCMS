using System;

namespace WebsiteBlogCMS.Classes
{
    public class Enums
    {
        public enum MessageType
        {
            Error,
            Warning,
            Success
        }

        public static string Message(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Error:
                    return "ErrorMessage";
                case MessageType.Warning:
                    return "WarningMessage";
                case MessageType.Success:
                    return "SuccessMessage";
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }
}