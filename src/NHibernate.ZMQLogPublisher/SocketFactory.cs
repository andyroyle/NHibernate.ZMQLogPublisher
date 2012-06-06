using ZMQ;

namespace NHibernate.ZMQLogPublisher
{
    public interface ISocketConfigurer
    {
        Socket GetPublisherSocket();
        Socket GetLoggersSinkSocket();
        Socket GetSyncSocket();
        Socket GetConfiguredSocket(SocketConfiguration configuration);
    }

    public class SocketFactory : ISocketConfigurer
    {
        private readonly IContext _context;
        private readonly IConfiguration _configuration;

        public SocketFactory(IContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public Socket GetPublisherSocket()
        {
            return GetConfiguredSocket(_configuration.PublisherSocketConfig);
        }

        public Socket GetLoggersSinkSocket()
        {
            return GetConfiguredSocket(_configuration.LoggersSinkSocketConfig);
        }

        public Socket GetSyncSocket()
        {
            return GetConfiguredSocket(_configuration.SyncSocketConfig);
        }

        public Socket GetConfiguredSocket(SocketConfiguration configuration)
        {
            return ConfigureSocket(_context.Socket(configuration.Type), configuration);
        }
        
        private Socket ConfigureSocket(Socket socket, SocketConfiguration socketConfig)
        {
            socket.Bind(socketConfig.Transport, socketConfig.Address);
            socket.HWM = socketConfig.HighWaterMark;
            socket.Linger = socketConfig.Linger;

            return socket;
        }
    }
}