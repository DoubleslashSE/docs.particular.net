﻿namespace Core4.Handlers
{
    using Common;
    using NServiceBus;
    using NServiceBus.Logging;

    #region GenericMessageHandler

    public class GenericHandler : IHandleMessages<object>
    {
        static ILog logger = LogManager.GetLogger(typeof(GenericHandler));

        public void Handle(object message)
        {
            logger.Info($"Received a message of type {message.GetType().Name}.");
            SomeLibrary.SomeMethod(message);
        }
    }

    #endregion
}
