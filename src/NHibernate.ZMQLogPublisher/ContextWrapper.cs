using System.Collections.Generic;
using ZMQ;

namespace NHibernate.ZMQLogPublisher
{
    public interface IContext
    {
        Socket Socket(SocketType socketType);
        void Poller(List<Socket> sockets, int i);
    }

    public class ContextWrapper : IContext
    {
        private readonly ZMQ.Context _context;

        public ContextWrapper(ZMQ.Context context)
        {
            _context = context;
        }

        public Socket Socket(SocketType socketType)
        {
            return _context.Socket(socketType);
        }

        public void Poller(List<Socket> sockets, int i)
        {
            ZMQ.Context.Poller(sockets, i);
        }
    }
}