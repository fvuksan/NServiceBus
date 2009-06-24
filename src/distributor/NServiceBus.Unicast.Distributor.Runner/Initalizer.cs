using NServiceBus.Grid.MessageHandlers;
using NServiceBus.Serialization;
using NServiceBus.Unicast.Transport.Msmq;
using NServiceBus.ObjectBuilder;
using NServiceBus.Config;
using System.Threading;
using System;

namespace NServiceBus.Unicast.Distributor.Runner
{
    /// <summary>
    /// Initializes the distributor's elements.
    /// </summary>
    public static class Initalizer
    {
        /// <summary>
        /// Assumes that an <see cref="IMessageSerializer"/> was already configured in the given builder.
        /// </summary>
        /// <returns></returns>
        public static void Init(Func<Configure, Configure> setupSerialization)
        {
            MsmqTransport dataTransport = null;

            setupSerialization(NServiceBus.Configure.With()
                .SpringBuilder()
                .RunCustomAction(
                () =>
                {
                    dataTransport = new MsmqTransport
                                        {
                                            InputQueue =
                                                System.Configuration.ConfigurationManager.AppSettings["DataInputQueue"],
                                            NumberOfWorkerThreads =
                                                int.Parse(
                                                System.Configuration.ConfigurationManager.AppSettings[
                                                    "NumberOfWorkerThreads"]),
                                            ErrorQueue =
                                                System.Configuration.ConfigurationManager.AppSettings["ErrorQueue"],
                                            IsTransactional = true,
                                            PurgeOnStartup = false,
                                            SkipDeserialization = true
                                        };

                    Configure.TypeConfigurer.ConfigureComponent<MsmqWorkerAvailabilityManager.MsmqWorkerAvailabilityManager>(ComponentCallModelEnum.Singleton)
                        .ConfigureProperty(x => x.StorageQueue, System.Configuration.ConfigurationManager.AppSettings["StorageQueue"]);

                    Configure.TypeConfigurer.ConfigureComponent<Distributor>(ComponentCallModelEnum.Singleton);
                }
                ))
                .MsmqTransport()
                    .IsTransactional(true)
                    .PurgeOnStartup(false)
                .UnicastBus()
                    .LoadMessageHandlers(
                        First<GridInterceptingMessageHandler>
                            .Then<ReadyMessageHandler>()
                    )
                .CreateBus()
                .Start(() =>
                    {
                        var d = Configure.ObjectBuilder.Build<Distributor>();
                        d.MessageBusTransport = dataTransport;

                        d.Start();
                    }
                );
        }
    }
}
