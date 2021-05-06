using System;
using Newtonsoft.Json;

namespace Wolf.Notification.Exceptions
{
    public class IncorrectModelException : BaseException
    {
        public IncorrectModelException(string message) : base(message)
        {
        }

        public IncorrectModelException(object insteadOfMessage) : base(insteadOfMessage)
        {
        }
    }
}