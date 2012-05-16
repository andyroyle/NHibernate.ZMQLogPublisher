using System;
using Machine.Fakes;
using Machine.Specifications;
using NHibernate.ZMQLogPublisher;

namespace UnitTests
{
    public class When_starting_the_publisher_thread : WithSubject<Publisher>
    {
        Establish context = () =>
                                {
                                    
                                };

        private Because of = () => Subject.StartPublisherThread();

        private It should_ = () => { };
    }
}