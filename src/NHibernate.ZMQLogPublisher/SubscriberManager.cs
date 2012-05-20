using System;
using System.Text;
using ZMQ;

namespace NHibernate.ZMQLogPublisher
{
    public interface ISubscriberManager
    {
        void Synchronise(Socket syncSocket, ref bool stopping);
    }

    public class SubscriberManager : ISubscriberManager
    {
        public void Synchronise(Socket syncSocket, ref bool stopping)
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
                syncSocket.Send(String.Empty, Encoding.Unicode);
            }
        }
    }
}