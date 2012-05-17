using System;
using System.Threading;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.ZMQLogPublisher;

namespace UnitTests
{
    public class When_starting_the_publisher_thread : WithSubject<Publisher>
    {
        Establish context = () =>
                                {
                                    stopping = Moq.It.IsAny<bool>();
                                };

        private Because of = () => Subject.StartPublisherThread();

        private static bool stopping;

        private It should_start_an_instance_of_the_logger_listener = 
            () => The<ILoggerListener>().WasToldTo(x => x.ListenAndPublishLogMessages(Moq.It.IsAny<AutoResetEvent>(), ref stopping));
    }
}