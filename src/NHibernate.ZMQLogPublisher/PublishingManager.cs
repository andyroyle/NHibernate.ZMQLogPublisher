namespace NHibernate.ZMQLogPublisher
{
    using ZMQ;

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
            Start(new Publisher(Configuration.LoadDefault()));
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
