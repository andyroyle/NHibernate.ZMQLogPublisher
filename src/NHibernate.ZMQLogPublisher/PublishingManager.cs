namespace NHibernate.ZMQLogPublisher
{
    public class PublishingManager
    {
        private static IPublisher instance;
        
        public static bool IsInstanceRunning
        {
            get
            {
                return instance.Running;
            }
        }

        public static void Start()
        {
            var context = new ContextWrapper(new ZMQ.Context(1));
            var configuration = Configuration.LoadDefault();
            var loggerFactory = new ZmqLoggerFactory(configuration.LoggersToPublish.ToArray());
            var socketConfigurer = new SocketFactory(context, configuration);
            var subscriberManager = new SubscriberManager();
            var loggerListener = new LoggerListener(context, loggerFactory, socketConfigurer, subscriberManager);  

            Start(new Publisher(context, loggerFactory, loggerListener));
        }

        public static void Start(IPublisher configuredInstance)
        {   
            instance = configuredInstance;
            instance.StartPublisherThread();
            instance.AssociateWithNHibernate();
        }

        public static void Stop()
        {
            instance.Shutdown();
        }
    
    }
}
