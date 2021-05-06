namespace Wolf.MessageQueue
{
    public class QueueOptions
    {
        public string ConnectionString { get; set; }
        public int ConnectRetry { get; set; }
        public int ReconnectDeltaBackOffMilliseconds { get; set; }
        public string ChannelName { get; set; }

        public int RedeliveryMaxAttempts { get; set; }
        public int RedeliveryFailDelayMilliseconds { get; set; } //redelivery delay is calculated by formula RedeliveryFailDelayMilliseconds * Math.Pow(RedeliveryExponentBase,  attempts);
        public int RedeliveryExponentBase { get; set; } //redelivery delay is calculated by formula RedeliveryFailDelayMilliseconds * Math.Pow(RedeliveryExponentBase,  attempts);
    }
}