using System;

namespace Wolf.Notification.Exceptions
{
    public class IncorrectFileException : BaseException
    {
        public IncorrectFileException(string message) : base(message)
        {
        }

        public IncorrectFileException(object insteadOfMessage) : base(insteadOfMessage)
        {
        }
    }
}