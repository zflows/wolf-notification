using System;

namespace Wolf.Notification.Exceptions
{
    public abstract class BaseException : Exception
    {
        public object InsteadOfMessage { get; set; }

        public BaseException(string message) : base(message)
        {

        }

        public BaseException(object insteadOfMessage)
        {
            this.InsteadOfMessage = insteadOfMessage;
        }
    }
}