using System;
using System.Threading;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.ZMQLogPublisher;

namespace UnitTests
{
    public class When_starting_the_publisher_thread : WithSubject<Publisher>
    {
        Establish context = () => {};

        private Because of = () => Subject.StartPublisherThread();

        private It should_start_an_instance_of_the_logger_listener = 
            () => The<ILoggerListener>().WasToldTo(x => x.ListenAndPublishLogMessages(Moq.It.IsAny<AutoResetEvent>(), Moq.It.IsAny<bool>()));
    }
}