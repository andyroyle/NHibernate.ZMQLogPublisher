namespace NHibernate.ZMQLogPublisher
{
    using System;
    using System.Collections.Concurrent;
    using ZMQ;

    public interface IZmqLoggerFactory : ILoggerFactory
    {
        void Initialize(IContext ctx);
        IInternalLogger LoggerFor(string keyName);
        IInternalLogger LoggerFor(Type type);
        void StopSockets();
        string[] loggersToPublish { get; }
    }

    public class ZmqLoggerFactory : IZmqLoggerFactory
    {
        private readonly ConcurrentDictionary<string, ZmqLogger> loggers;

        public string[] loggersToPublish { get; private set; }

        private IContext context;

        public ZmqLoggerFactory(string[] loggersToPublish)
        {
            this.loggers = new ConcurrentDictionary<string, ZmqLogger>();
            this.loggersToPublish = loggersToPublish;
        }

        public void Initialize(IContext ctx)
        {
            this.context = ctx;

            foreach (var logger in this.loggers.Values)
            {
                lock (logger)
                {
                    logger.InitializeWithSocket(this.context.Socket(SocketType.PUSH));
                }
            }
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return this.loggers.GetOrAdd(
                keyName,
                key =>
                {
                    var logger = new ZmqLogger(keyName, Array.IndexOf(loggersToPublish, keyName) == -1);

                    if (PublishingManager.IsInstanceRunning)
                    {
                        logger.InitializeWithSocket(this.context.Socket(SocketType.PUSH));
                    }
                    return logger;
                });
        }

        public IInternalLogger LoggerFor(Type type)
        {
            return this.LoggerFor(type.FullName);
        }

        public void StopSockets()
        {
            foreach (var kvp in this.loggers)
            {
                ZmqLogger logger = kvp.Value;
                logger.StopSocket();
            }
        }
    }
}