using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Tests;
using FluentAssertions;
using JetBrains.Annotations;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Transport.InMem;
using SimpleInjector;
using Xunit.Abstractions;
using SimpleInjector.Lifestyles;
using Xunit;

namespace Dbosoft.Rebus.SimpleInjector.Tests;

public class SimpleInjectorUnitOfWorkTests
{
    private readonly ITestOutputHelper _output;

    public SimpleInjectorUnitOfWorkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task WorksWithUnitOfWork()
    {
        await using var container = ArrangeRebus(o => o.EnableSimpleInjectorUnitOfWork());
        container.Register<IRebusUnitOfWork, TestRebusUnitOfWork>(Lifestyle.Scoped);
        container.Collection.Append<IHandleMessages<TestMessage>, TestMessageHandler>(Lifestyle.Scoped);

        await using var scope = AsyncScopedLifestyle.BeginScope(container);
        var bus = container.GetInstance<IBus>();
        var messageId = Guid.NewGuid();
        await bus.SendLocal(new TestMessage {  Id = messageId });
        await Task.Delay(1000);

        var collector = container.GetInstance<TestMessageCollector>();
        collector.Messages.Should().SatisfyRespectively(
            message => message.Id.Should().Be(messageId));
    }

    [Fact]
    public async Task WorksWithoutUnitOfWork()
    {
        await using var container = ArrangeRebus(_ => { });
        container.Collection.Append<IHandleMessages<TestMessage>, TestMessageHandler>(Lifestyle.Scoped);

        await using var scope = AsyncScopedLifestyle.BeginScope(container);
        var bus = container.GetInstance<IBus>();
        var messageId = Guid.NewGuid();
        await bus.SendLocal(new TestMessage { Id = messageId });
        await Task.Delay(1000);

        var collector = container.GetInstance<TestMessageCollector>();
        collector.Messages.Should().SatisfyRespectively(
            message => message.Id.Should().Be(messageId));
    }

    private Container ArrangeRebus(Action<OptionsConfigurer> configureOptions)
    {
        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        var rebusNetwork = new InMemNetwork();

        container.RegisterSingleton<TestMessageCollector>();
        container.ConfigureRebus(configurer =>
        {
            return configurer
                .Options(configureOptions)
                .Transport(cfg => cfg.UseInMemoryTransport(rebusNetwork, "main"))
                .Logging(x => x.Use(new RebusTestLogging(_output)))
                .Start();
        });

        return container;
    }

    private sealed class TestMessage
    {
        public Guid Id { get; set; }
    }

    [UsedImplicitly]
    private sealed class TestMessageHandler : IHandleMessages<TestMessage>
    {
        private readonly TestMessageCollector _collector;

        public TestMessageHandler(TestMessageCollector collector)
        {
            _collector = collector;
        }

        public Task Handle(TestMessage message)
        {
            _collector.Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class TestMessageCollector
    {
        public IList<TestMessage> Messages { get; } = new List<TestMessage>();
    }
}
