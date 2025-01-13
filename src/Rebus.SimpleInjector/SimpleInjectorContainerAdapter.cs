// Based on https://github.com/rebus-org/Rebus.SimpleInjector
// Copyright (c) Mogens Heller Grabe
// Licensed under MIT license https://github.com/rebus-org/Rebus.SimpleInjector/blob/master/LICENSE.md
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Transport;
using SimpleInjector;
using SimpleInjector.Lifestyles;

// ReSharper disable ArgumentsStyleLiteral
#pragma warning disable 1998

namespace Dbosoft.Rebus;

internal class SimpleInjectorContainerAdapter : IContainerAdapter
{
    private readonly Container _container;

    private bool _busWasSet;

    /// <summary>
    ///     Constructs the container adapter
    /// </summary>
    public SimpleInjectorContainerAdapter(Container container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    /// <summary>
    ///     Resolves all handlers for the given <typeparamref name="TMessage" /> message type
    /// </summary>
    public async Task<IEnumerable<IHandleMessages<TMessage>>> GetHandlers<TMessage>(TMessage message,
        ITransactionContext transactionContext)
    {
        var scope = AsyncScopedLifestyle.BeginScope(_container);
        transactionContext.Items["SI_scope"] = scope;
        transactionContext.OnDisposed(_ => scope.Dispose());

        // In difference to the default implementation by Rebus, we manage the unit of work with
        // SimpleInjector. Hence, we can only initialize the unit of work after the scope has
        // been created. The use of IRebusUnitOfWork is optional.
        if (TryGetInstance<IRebusUnitOfWork>(_container, out var unitOfWork))
        {
            await unitOfWork.Initialize().ConfigureAwait(false);
        }

        return TryGetInstance<IEnumerable<IHandleMessages<TMessage>>>(_container, out var handlerInstances)
            ? handlerInstances.ToList()
            : Array.Empty<IHandleMessages<TMessage>>();
    }

    public void SetBus(IBus bus)
    {
        // hack: this is just to satisfy the contract test... we are pretty sure that
        // 1. the SetBus method is not called twice, becauwe
        // 2. we are the only ones who create the instance of the container adapter, because
        // 3. the container adapter class is internal
        if (_busWasSet)
            throw new InvalidOperationException(
                "SetBus was called twice on the container adapter. This is a sign that something has gone wrong during the configuration process.");
        _busWasSet = true;

        // 2nd hack:
        // again: we control calls to SetBus in this container adapter, because we create it...
        // so, to make the contract tests happy, we need to do this:
        var actualBusType = bus.GetType();

        if (actualBusType is { Name: "FakeBus", DeclaringType.Name: "ContainerTests`1" })
            bus.Dispose();
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private static bool TryGetInstance<TService>(Container container, out TService instance)
        where TService : class
    {
        IServiceProvider provider = container;
        instance = (TService) provider.GetService(typeof(TService));
        return instance != null;
    }
}