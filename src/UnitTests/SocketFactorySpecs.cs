using Machine.Fakes;
using Machine.Specifications;
using NHibernate.ZMQLogPublisher;
using ZMQ;
using It = Machine.Specifications.It;

namespace UnitTests
{
    public class SocketFactoryGeneralContext : WithSubject<SocketFactory>
    {
        Establish context = () =>
        {
            zmqContext = new Context(1);
        };

        protected static Socket result;
        protected static SocketConfiguration configuration;
        protected static ZMQ.Context zmqContext;
    }

    // Not sure about this test, feels wrong
    public class when_getting_a_configured_socket : SocketFactoryGeneralContext
    {
        private Establish context = () =>
        {
            configuration = new SocketConfiguration()
            {
                Address = "*:5467", // arbitrary port number, should be ok to bind to
                Transport = Transport.TCP,
                Type = SocketType.PUB
            };

            The<IContext>().WhenToldTo(x => x.Socket(SocketType.PUB)).Return(zmqContext.Socket(SocketType.PUB));
        };

        private Because of = () => result = Subject.GetConfiguredSocket(configuration);

        private It should_return_a_ZMQ_socket = () => result.ShouldBeOfType(typeof(Socket));

        private It should_ask_the_context_for_a_socket_of_type_pub = () => The<IContext>().WasToldTo(x => x.Socket(SocketType.PUB));
    }
}