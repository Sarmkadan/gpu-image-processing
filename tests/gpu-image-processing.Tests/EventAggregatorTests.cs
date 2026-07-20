#nullable enable
using System;
using Xunit;
using GpuImageProcessing.Events;

namespace GpuImageProcessing.Tests
{
    public class EventAggregatorTests
    {
        private class TestEvent : DomainEvent
        {
            public string Message { get; set; }
        }

        [Fact]
        public void Subscribe_and_publish_delivers_event()
        {
            var aggregator = new EventAggregator();
            TestEvent? received = null;
            var subscription = aggregator.Subscribe<TestEvent>(e => received = e);

            var testEvent = new TestEvent { Message = "hello" };
            aggregator.Publish(testEvent);

            Assert.NotNull(received);
            Assert.Equal(testEvent.Message, received.Message);
            subscription.Dispose();
        }

        [Fact]
        public void Unsubscribe_stops_delivery()
        {
            var aggregator = new EventAggregator();
            TestEvent? received = null;
            var subscription = aggregator.Subscribe<TestEvent>(e => received = e);
            subscription.Dispose();

            var testEvent = new TestEvent { Message = "hello" };
            aggregator.Publish(testEvent);

            Assert.Null(received);
        }

        [Fact]
        public void Multiple_subscribers_all_receive()
        {
            var aggregator = new EventAggregator();
            TestEvent? received1 = null;
            TestEvent? received2 = null;
            var sub1 = aggregator.Subscribe<TestEvent>(e => received1 = e);
            var sub2 = aggregator.Subscribe<TestEvent>(e => received2 = e);

            var testEvent = new TestEvent { Message = "hello" };
            aggregator.Publish(testEvent);

            Assert.NotNull(received1);
            Assert.NotNull(received2);
            Assert.Equal(testEvent.Message, received1.Message);
            Assert.Equal(testEvent.Message, received2.Message);

            sub1.Dispose();
            sub2.Dispose();
        }

        [Fact]
        public void Publish_with_no_subscribers_does_not_throw()
        {
            var aggregator = new EventAggregator();
            var testEvent = new TestEvent { Message = "hello" };
            var exception = Record.Exception(() => aggregator.Publish(testEvent));
            Assert.Null(exception);
        }
    }
}
