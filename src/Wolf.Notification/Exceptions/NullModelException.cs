using System;

namespace Wolf.Notification.Exceptions
{
    public class NullModelException : BaseException
    {
        public NullModelException(string message) : base(message)
        {
        }

        public NullModelException(object insteadOfMessage) : base(insteadOfMessage)
        {
        }
    }
}