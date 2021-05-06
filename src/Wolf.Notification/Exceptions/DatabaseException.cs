using System;

namespace Wolf.Notification.Exceptions
{
    public class DatabaseException : BaseException
    {
        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(object insteadOfMessage) : base(insteadOfMessage)
        {
        }
    }
}