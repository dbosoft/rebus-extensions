using System;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Transport;
using Rebus.Transport.FileSystem;
using Rebus.Transport.InMem;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class DefaultTransportSelector : RebusTransportSelectorBase
{
    private readonly InMemNetwork _network;

    public DefaultTransportSelector(InMemNetwork network, IConfiguration configuration, ILogger log) : base(configuration, log)
    {
        _network = network;
    }

    public override string[] AcceptedConfigTypes => new[] { "inmemory", "filesystem" };

    protected override void ConfigureBusType(string busType, string queueName, StandardConfigurer<ITransport> configurer)
    {
        switch (busType)
        {
            case "filesystem":
                var path = Configuration[$"{ConfigurationName}:path"];

                if (path == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::path.");

                configurer.UseFileSystem(Path.Combine(path, "transport"), queueName);
                return;
            case "inmemory":
                configurer.UseInMemoryTransport(_network, queueName, ShouldStoreSubscriptions());
                return;
        }
    }

    protected override void ConfigureBusTypeAsOneWayClient(string busType, StandardConfigurer<ITransport> configurer)
    {
        switch (busType)
        {
            case "filesystem":
                var path = Configuration[$"{ConfigurationName}:path"];

                if (path == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::path.");

                configurer.UseFileSystemAsOneWayClient(path);
                return;
            case "inmemory":
                configurer.UseInMemoryTransportAsOneWayClient(_network, ShouldStoreSubscriptions());
                return;
        }
    }

    protected bool ShouldStoreSubscriptions()
    {
        var value = Configuration[$"{ConfigurationName}:storeSubscriptions"];
        if(string.IsNullOrWhiteSpace(value))
            return true;

        if (bool.TryParse(value, out var result))
            return result;

        throw new InvalidOperationException($"Invalid value for {ConfigurationName}::storeSubscriptions.");
    }
}