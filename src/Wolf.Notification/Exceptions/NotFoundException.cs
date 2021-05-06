using System;

namespace Wolf.Notification.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(object insteadOfMessage) : base(insteadOfMessage)
        {
        }
    }
}