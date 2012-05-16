using Machine.Specifications;
using Moq;
using NHibernate.ZMQLogPublisher;
using It = Machine.Specifications.It;

namespace UnitTests
{
    public class When_starting_the_publishingmanager
    {
        Establish context = () =>
                                {
                                    publisher = new Mock<IPublisher>();
                                };

        private Because of = () => PublishingManager.Start(publisher.Object);

        private It should_start_the_publisher_thread = 
            () => publisher.Verify(x => x.StartPublisherThread(), Times.Once());

        private It should_associate_with_nhibernate =
            () => publisher.Verify(x => x.AssociateWithNHibernate(), Times.Once());

        private static Mock<IPublisher> publisher;
    }

    public class When_stopping_the_publishingmanager
    {
        Establish context = () =>
        {
            publisher = new Mock<IPublisher>();
            PublishingManager.Start(publisher.Object);
        };

        private Because of = () => PublishingManager.Stop();

        private It should_shutdown_the_instance = 
            () => publisher.Verify(x => x.Shutdown(), Times.Once());

        private static Mock<IPublisher> publisher;
    }
}