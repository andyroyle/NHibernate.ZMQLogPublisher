using System.Collections.Generic;
using ZMQ;
using System.Threading;

namespace NHibernate.ZMQLogPublisher
{
    public interface ILoggerListener
    {
        void ListenAndPublishLogMessages(AutoResetEvent callingThreadReset, ref bool stopping);
    }

    public class LoggerListener : ILoggerListener
    {
        private readonly IContext _context;
        private readonly IZmqLoggerFactory _zmqLoggerFactory;
        private readonly ISocketConfigurer _socketFactory;
        private readonly ISubscriberManager _subscriberManager;

        public LoggerListener(IContext context, IZmqLoggerFactory zmqLoggerFactory, ISocketConfigurer socketFactory, ISubscriberManager subscriberManager)
        {
            _context = context;
            _zmqLoggerFactory = zmqLoggerFactory;
            _socketFactory = socketFactory;
            _subscriberManager = subscriberManager;
        }

        public void ListenAndPublishLogMessages(AutoResetEvent callingThreadReset, ref bool stopping)
        {
            using (Socket publisher = _socketFactory.GetPublisherSocket(),
                          loggersSink = _socketFactory.GetLoggersSinkSocket(),
                          syncSocket = _socketFactory.GetSyncSocket())
            {
                loggersSink.PollInHandler += (socket, revents) => publisher.Send(socket.Recv());
                callingThreadReset.Set();

                _subscriberManager.Synchronise(syncSocket, ref stopping);

                while (!stopping)
                {
                    _context.Poller(new List<Socket> { loggersSink, publisher }, 2000);
                }
            }

            callingThreadReset.Set();
            _zmqLoggerFactory.StopSockets();
        }
    }
}
