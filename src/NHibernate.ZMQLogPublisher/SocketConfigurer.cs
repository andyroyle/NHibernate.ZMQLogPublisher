using System.Collections.Generic;
using ZMQ;

namespace NHibernate.ZMQLogPublisher
{
    public interface ISocketConfigurer
    {
        void ConfigureSockets(IEnumerable<KeyValuePair<Socket, SocketConfiguration>> toConfigure);
        void ConfigureSocket(Socket socket, SocketConfiguration socketConfig);
    }

    public class SocketConfigurer : ISocketConfigurer
    {
        public void ConfigureSockets(IEnumerable<KeyValuePair<Socket, SocketConfiguration>> toConfigure)
        {
            foreach (var pair in toConfigure)
            {
                ConfigureSocket(pair.Key, pair.Value);
            }
        }

        public void ConfigureSocket(Socket socket, SocketConfiguration socketConfig)
        {
            socket.Bind(socketConfig.Transport, socketConfig.Address);
            socket.HWM = socketConfig.HighWaterMark;
            socket.Linger = socketConfig.Linger;
        }
    }
}