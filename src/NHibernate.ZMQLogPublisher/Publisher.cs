using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZMQ;

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
        private readonly AutoResetEvent threadRunningEvent;
        private Thread publisherThread;

        private bool running;
        private bool stopping;
        
        public Publisher(IContext context, IZmqLoggerFactory zmqLoggerFactory, ILoggerListener loggerListener)
        {
            _context = context;
            _zmqLoggerFactory = zmqLoggerFactory;
            _loggerListener = loggerListener;
            
            threadRunningEvent = new AutoResetEvent(false);
        }

        public bool Running
        {
            get
            {
                return running;
            }
        }

        public void StartPublisherThread()
        {
            publisherThread = new Thread(() => _loggerListener.ListenAndPublishLogMessages(threadRunningEvent, this.stopping));
            publisherThread.Start();

            threadRunningEvent.WaitOne(5000);
            running = true;
        }

        public void Shutdown()
        {
            this.stopping = true;
            this.running = false;

            this.threadRunningEvent.WaitOne();
            this.stopping = false;
        }

        public void AssociateWithNHibernate()
        {
            this._zmqLoggerFactory.Initialize(this._context);

            LoggerProvider.SetLoggersFactory(this._zmqLoggerFactory);
        }
    }
}