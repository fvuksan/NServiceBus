using NServiceBus;
using NServiceBus.Host;

namespace V1Subscriber
{
    public class V1SubscriberEndpoint : IConfigureThisEndpoint
    {
        public void Init(Configure configure)
        {
            configure
                .XmlSerializer()
                .MsmqTransport()
                    .IsTransactional(true)
                    .PurgeOnStartup(false)
                .UnicastBus()
                    .ImpersonateSender(false)
                    .LoadMessageHandlers();
        }
    }
}