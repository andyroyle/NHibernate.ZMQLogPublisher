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
        void ListenAndPublishLogMessages(AutoResetEvent callingThreadReset);
        void ConfigureSocket(Socket socket, SocketConfiguration socketConfig);
    }

    public class Publisher : IPublisher
    {
        private readonly IContext context;
        private readonly IConfiguration configuration;
        private readonly IZmqLoggerFactory _zmqLoggerFactory;
        private readonly AutoResetEvent threadRunningEvent;
        private Thread publisherThread;

        private bool running;
        private bool stopping;
        
        public Publisher(IConfiguration configuration)
            :this(configuration, new ContextWrapper(new ZMQ.Context(1)), new ZmqLoggerFactory(configuration.LoggersToPublish.ToArray()))
        {
        }

        public Publisher(IConfiguration configuration, IContext context, IZmqLoggerFactory zmqLoggerFactory)
        {
            this.context = context;
            this.configuration = configuration;
            this._zmqLoggerFactory = zmqLoggerFactory;
            
            this.threadRunningEvent = new AutoResetEvent(false);
        }

        public bool Running
        {
            get
            {
                return this.running;
            }
        }

        public void StartPublisherThread()
        {
            this.publisherThread = new Thread(() => this.ListenAndPublishLogMessages(threadRunningEvent));
            this.publisherThread.Start();

            this.threadRunningEvent.WaitOne(5000);
            this.running = true;
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
            this._zmqLoggerFactory.Initialize(this.context);

            LoggerProvider.SetLoggersFactory(this._zmqLoggerFactory);
        }

        public void ListenAndPublishLogMessages(AutoResetEvent callingThreadReset)
        {
            using (Socket publisher = this.context.Socket(SocketType.PUB),
                          loggersSink = this.context.Socket(SocketType.PULL),
                          syncSocket = this.context.Socket(SocketType.REP))
            {
                this.ConfigureSocket(publisher, this.configuration.PublisherSocketConfig);
                this.ConfigureSocket(syncSocket, this.configuration.SyncSocketConfig);
                this.ConfigureSocket(loggersSink, this.configuration.LoggersSinkSocketConfig);

                loggersSink.PollInHandler += (socket, revents) => publisher.Send(socket.Recv());
                
                callingThreadReset.Set();
                
                byte[] syncMessage = null;
                // keep waiting for syncMessage before starting to publish
                // unless we stop before we recieve the sync message
                while (!this.stopping && syncMessage == null)
                {
                    syncMessage = syncSocket.Recv(SendRecvOpt.NOBLOCK);
                }

                // send sync confirmation if we recieved a sync request
                if (syncMessage != null)
                {
                    syncSocket.Send(string.Empty, Encoding.Unicode);
                }

                while (!this.stopping)
                {
                    this.context.Poller(new List<Socket> { loggersSink, publisher }, 2000);
                }
            }

            callingThreadReset.Set();
            this._zmqLoggerFactory.StopSockets();
        }

        public void ConfigureSocket(Socket socket, SocketConfiguration socketConfig)
        {
            socket.Bind(socketConfig.Transport, socketConfig.Address);
            socket.HWM = socketConfig.HighWaterMark;
            socket.Linger = socketConfig.Linger;
        }
    }
}