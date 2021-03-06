﻿namespace Core6.Headers.Writers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using NServiceBus;
    using NServiceBus.MessageMutator;
    using NUnit.Framework;
    using Operations.Msmq;

    [TestFixture]
    public class HeaderWriterReturn
    {
        static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);
        string endpointName = "HeaderWriterReturnV6";

        [SetUp]
        [TearDown]
        public void Setup()
        {
            QueueDeletion.DeleteQueuesForEndpoint(endpointName);
        }

        [Test]
        public async Task Write()
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            var callbackTypes = typeof(RequestResponseExtensions).Assembly.GetTypes();
            var typesToScan = TypeScanner.NestedTypes<HeaderWriterReturn>(callbackTypes);
            endpointConfiguration.SetTypesToScan(typesToScan);
            endpointConfiguration.ScaleOut().InstanceDiscriminator("A");
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.RegisterComponents(c => c.ConfigureComponent<Mutator>(DependencyLifecycle.InstancePerCall));
            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
            await endpointInstance.SendLocal(new MessageToSend())
                .ConfigureAwait(false);
            ManualResetEvent.WaitOne();
        }

        class MessageToSend : IMessage
        {
        }

        class MessageHandler : IHandleMessages<MessageToSend>
        {
            public Task Handle(MessageToSend message, IMessageHandlerContext context)
            {
                return context.Reply(100);
            }
        }

        class Mutator : IMutateIncomingTransportMessages
        {

            public Task MutateIncoming(MutateIncomingTransportMessageContext context)
            {
                if (context.IsMessageOfTye<MessageToSend>())
                {
                    var sendingText = HeaderWriter.ToFriendlyString<HeaderWriterReturn>(context.Headers);
                    SnippetLogger.Write(
                        text: sendingText,
                        suffix: "Sending",
                        version: "6");
                }
                else
                {
                    var returnText = HeaderWriter.ToFriendlyString<HeaderWriterReturn>(context.Headers);
                    SnippetLogger.Write(
                        text: returnText,
                        suffix: "Returning",
                        version: "6");
                    ManualResetEvent.Set();
                }
                return Task.FromResult(0);
            }
        }
    }
}