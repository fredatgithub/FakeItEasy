﻿namespace FakeItEasy.Tests
{
    using System;
    using FakeItEasy.Core;
    using FakeItEasy.Tests.TestHelpers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class RaiseTests
    {
        private IFoo foo;
        private object sender;
        private EventArgs eventArguments;

        [SetUp]
        public void Setup()
        {
            this.foo = A.Fake<IFoo>();
            this.foo.SomethingHappened += new EventHandler(this.Foo_SomethingHappened);
            this.sender = null;
            this.eventArguments = null;
        }

        [Test]
        public void Raising_with_sender_and_arguments_should_raise_event_with_specified_sender()
        {
            // Arrange
            object senderToUse = new object();
            
            // Act
            this.foo.SomethingHappened += Raise.With(senderToUse, EventArgs.Empty).Now;

            // Assert
            this.sender.Should().Be(senderToUse);
        }

        [Test]
        public void Raising_with_sender_and_arguments_should_raise_event_with_specified_arguments()
        {
            // Arrange
            var arguments = new EventArgs();

            // Act
            this.foo.SomethingHappened += Raise.With(this.foo, arguments).Now;

            // Assert
            this.eventArguments.Should().BeSameAs(arguments);
        }

        [Test]
        public void Raising_with_arguments_only_should_raise_event_with_fake_as_sender()
        {
            // Arrange

            // Act
            this.foo.SomethingHappened += Raise.With(EventArgs.Empty).Now;

            // Assert
            this.sender.Should().BeSameAs(this.foo);
        }

        [Test]
        public void Raising_with_arguments_only_should_raise_event_with_specified_arguments()
        {
            // Arrange
            var arguments = new EventArgs();

            // Act
            this.foo.SomethingHappened += Raise.With(arguments).Now;

            // Assert
            this.eventArguments.Should().BeSameAs(arguments);
        }

        [Test]
        public void Now_should_throw_when_called_directly()
        {
            // Arrange
            var raiser = new Raise<EventArgs>(null, EventArgs.Empty);

            // Act
            var exception = Record.Exception(() => raiser.Now(null, null));

            // Assert
            exception.Should().BeAnExceptionOfType<NotSupportedException>();
        }

        [Test]
        public void Go_should_return_handler_with_Now_as_method()
        {
            // Arrange

            // Act
            var methodName = Raise.With(EventArgs.Empty).Go.Method.Name;

            // Assert
            methodName.Should().Be("Now");
        }

        [Test]
        public void WithEmpty_should_return_raise_object_with_event_args_empty_set()
        {
            // Arrange
            var result = Raise.WithEmpty();

            // Act
            var eventArgs = (result as IEventRaiserArguments).EventArguments;

            // Assert
            eventArgs.Should().Be(EventArgs.Empty);
        }

        [Test]
        public void Should_not_fail_when_raising_event_that_has_no_registered_listeners()
        {
            // Arrange
            this.foo = A.Fake<IFoo>();

            // Act
            var exception = Record.Exception(() => { foo.SomethingHappened += Raise.WithEmpty().Now; });

            // Assert
            exception.Should().BeNull();
        }

        [Test]
        public void Should_propagate_exception_thrown_by_event_handler()
        {
            // Arrange
            this.foo = A.Fake<IFoo>();
            this.foo.SomethingHappened += (s, e) => { throw new NotImplementedException(); };

            // Act
            Action action = () => this.foo.SomethingHappened += Raise.WithEmpty().Now;
            var exception = Record.Exception(action);

            // Assert
            exception.Should().BeAnExceptionOfType<NotImplementedException>()
                .And.StackTrace.Should().Contain("FakeItEasy.Tests.RaiseTests");
        }

        private void Foo_SomethingHappened(object newSender, EventArgs e)
        {
            this.sender = newSender;
            this.eventArguments = e;
        }
    }
}
