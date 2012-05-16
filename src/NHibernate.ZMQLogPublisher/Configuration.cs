using ZMQ;

namespace NHibernate.ZMQLogPublisher
{
    using System.Collections.Generic;

    public interface IConfiguration
    {
        SocketConfiguration SyncSocketConfig { get; set; }
        SocketConfiguration PublisherSocketConfig { get; set; }
        SocketConfiguration LoggersSinkSocketConfig { get; set; }
        
        List<string> LoggersToPublish { get; set; }
        
        Configuration AddLoggerKeyToPublish(string key);
        Configuration ConfigureSyncSocket(System.Action<SocketConfiguration> socketConfigAction);
        Configuration ConfigurePublisherSocket(System.Action<SocketConfiguration> socketConfigAction);
    }

    public class Configuration : IConfiguration
    {
        public SocketConfiguration SyncSocketConfig { get; set; }
        public SocketConfiguration PublisherSocketConfig { get; set; }
        public SocketConfiguration LoggersSinkSocketConfig { get; set; }

        public List<string> LoggersToPublish { get; set; }
        
        public static Configuration LoadDefault()
        {
            var config = new Configuration();
            config.SyncSocketConfig = new SocketConfiguration { Address = "*:68747", Transport = Transport.TCP};
            config.PublisherSocketConfig = new SocketConfiguration { Address = "*:68748", Transport = Transport.TCP };
            config.LoggersSinkSocketConfig = new SocketConfiguration {Address = "loggers", Transport = Transport.INPROC };

            config.LoggersToPublish = new List<string>
            { 
                "NHibernate.SQL", "NHibernate.Impl.SessionImpl", "NHibernate.Transaction.AdoTransaction",
                "NHibernate.AdoNet.AbstractBatcher"
            };

            return config;
        }

        public Configuration AddLoggerKeyToPublish(string key)
        {
            if (!LoggersToPublish.Contains(key))
            {
                LoggersToPublish.Add(key);
            }

            return this;
        }

        public Configuration ConfigureSyncSocket(System.Action<SocketConfiguration> socketConfigAction)
        {
            socketConfigAction(this.SyncSocketConfig);

            return this;
        }

        public Configuration ConfigurePublisherSocket(System.Action<SocketConfiguration> socketConfigAction)
        {
            socketConfigAction(this.PublisherSocketConfig);

            return this;
        }

        public Configuration ConfigureSinkSocket(System.Action<SocketConfiguration> socketConfigAction)
        {
            socketConfigAction(this.LoggersSinkSocketConfig);

            return this;
        }
    }
}