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

        public LoggerListener(IContext context, IConfiguration configuration, IZmqLoggerFactory zmqLoggerFactory)
        {
            _context = context;
            _configuration = configuration;
            _zmqLoggerFactory = zmqLoggerFactory;
        }

        public void ListenAndPublishLogMessages(AutoResetEvent callingThreadReset, ref bool stopping)
        {
            using (Socket publisher = _context.Socket(SocketType.PUB),
                          loggersSink = _context.Socket(SocketType.PULL),
                          syncSocket = _context.Socket(SocketType.REP))
            {
                ConfigureSockets(new Dictionary<Socket, SocketConfiguration>(){
                    {publisher, _configuration.PublisherSocketConfig},
                    {syncSocket, _configuration.SyncSocketConfig},
                    {loggersSink, _configuration.LoggersSinkSocketConfig}
                });

                loggersSink.PollInHandler += (socket, revents) => publisher.Send(socket.Recv());

                callingThreadReset.Set();

                Synchronise(stopping, syncSocket);

                while (!stopping)
                {
                    _context.Poller(new List<Socket> { loggersSink, publisher }, 2000);
                }
            }

            callingThreadReset.Set();
            _zmqLoggerFactory.StopSockets();
        }

        private static void Synchronise(bool stopping, Socket syncSocket)
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

        private void ConfigureSockets(IEnumerable<KeyValuePair<Socket, SocketConfiguration>> toConfigure)
        {
            foreach (var pair in toConfigure)
            {
                ConfigureSocket(pair.Key, pair.Value);
            }
        }

        private void ConfigureSocket(Socket socket, SocketConfiguration socketConfig)
        {
            socket.Bind(socketConfig.Transport, socketConfig.Address);
            socket.HWM = socketConfig.HighWaterMark;
            socket.Linger = socketConfig.Linger;
        }
    }
}
