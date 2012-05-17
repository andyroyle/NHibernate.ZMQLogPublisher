using System.Threading;

namespace NHibernate.ZMQLogPublisher
{
    public interface IPublisher
    {
        bool Running { get; }
        void Shutdown();
        void StartPublisherThread();
        void AssociateWithNHibernate();
    }

    public class Publisher : IPublisher
    {
        private readonly IContext _context;
        private readonly IZmqLoggerFactory _zmqLoggerFactory;
        private readonly ILoggerListener _loggerListener;
        private readonly AutoResetEvent _threadStateChangedEvent;
        private Thread _publisherThread;

        private bool _running;
        private bool _stopping;
        
        public Publisher(IContext context, IZmqLoggerFactory zmqLoggerFactory, ILoggerListener loggerListener)
        {
            _context = context;
            _zmqLoggerFactory = zmqLoggerFactory;
            _loggerListener = loggerListener;
            
            _threadStateChangedEvent = new AutoResetEvent(false);
        }

        public bool Running { get { return _running; } }

        public void StartPublisherThread()
        {
            _publisherThread = new Thread(() => _loggerListener.ListenAndPublishLogMessages(_threadStateChangedEvent, this._stopping));
            _publisherThread.Start();

            _threadStateChangedEvent.WaitOne(5000);
            _running = true;
        }

        public void Shutdown()
        {
            _stopping = true;
            _running = false;

            _threadStateChangedEvent.WaitOne();
            _stopping = false;
        }

        public void AssociateWithNHibernate()
        {
            _zmqLoggerFactory.Initialize(_context);
            LoggerProvider.SetLoggersFactory(_zmqLoggerFactory);
        }
    }
}