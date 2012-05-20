using System.Collections.Generic;
using System.Text;
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
        private readonly IConfiguration _configuration;
        private readonly IZmqLoggerFactory _zmqLoggerFactory;
        private readonly ISocketConfigurer _socketFactory;

        public LoggerListener(IContext context, IConfiguration configuration, IZmqLoggerFactory zmqLoggerFactory, ISocketConfigurer socketFactory)
        {
            _context = context;
            _configuration = configuration;
            _zmqLoggerFactory = zmqLoggerFactory;
            _socketFactory = socketFactory;
        }

        public void ListenAndPublishLogMessages(AutoResetEvent callingThreadReset, ref bool stopping)
        {
            using (Socket publisher = _socketFactory.GetSocket(_configuration.PublisherSocketConfig),
                          loggersSink = _socketFactory.GetSocket(_configuration.LoggersSinkSocketConfig),
                          syncSocket = _socketFactory.GetSocket(_configuration.SyncSocketConfig))
            {
                loggersSink.PollInHandler += (socket, revents) => publisher.Send(socket.Recv());

                // tells the caller that the thread has started properly
                callingThreadReset.Set();

                Synchronise(syncSocket, ref stopping);

                while (!stopping)
                {
                    _context.Poller(new List<Socket> { loggersSink, publisher }, 2000);
                }
            }

            callingThreadReset.Set();
            _zmqLoggerFactory.StopSockets();
        }

        private static void Synchronise(Socket syncSocket, ref bool stopping)
        {
            byte[] syncMessage = null;
            // keep waiting for syncMessage before starting to publish
            // unless we stop before we recieve the sync message
            while (!stopping && syncMessage == null)
            {
                syncMessage = syncSocket.Recv(SendRecvOpt.NOBLOCK);
            }

            // send sync confirmation if we recieved a sync request
            if (syncMessage != null)
            {
                syncSocket.Send(string.Empty, Encoding.Unicode);
            }
        }
    }
}
