using System.Collections.Generic;
using System.Linq;
using ZMQ;

namespace NHibernate.ZMQLogPublisher
{
    public interface ISocketConfigurer
    {
        Socket GetSocket(SocketConfiguration configuration);
        IEnumerable<Socket> GetSockets(IEnumerable<SocketConfiguration> configurations);
    }

    public class SocketFactory : ISocketConfigurer
    {
        private readonly IContext _context;

        public SocketFactory(IContext context)
        {
            _context = context;
        }

        public Socket GetSocket(SocketConfiguration configuration)
        {
            return ConfigureSocket(_context.Socket(configuration.Type), configuration);
        }

        public IEnumerable<Socket> GetSockets(IEnumerable<SocketConfiguration> configurations)
        {
            return configurations.Select(c => ConfigureSocket(_context.Socket(c.Type), c)).ToList();
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