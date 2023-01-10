using System;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Persistence.FileSystem;
using Rebus.Persistence.InMem;
using Rebus.Subscriptions;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class DefaultSubscriptionStoreSelector : GenericRebusSelectorBase<ISubscriptionStorage>
{
    private readonly InMemorySubscriberStore _store;
    

    public DefaultSubscriptionStoreSelector(InMemorySubscriberStore store, IConfiguration configuration, 
        ILogger log) : base(configuration, log)
    {
        _store = store;
    }

    public override string[] AcceptedConfigTypes => new[] { "inmemory", "filesystem" };
    public override string ConfigurationName => "bus";


    protected override void ConfigureByType(string busType, StandardConfigurer<ISubscriptionStorage> configurer)
    {
        switch (busType)
        {
            case "inmemory":
                configurer.StoreInMemory(_store);
                break;
            case "filesystem":
                var fileName = Configuration[$"{ConfigurationName}:subscriptionFile"];
                var path = Configuration[$"{ConfigurationName}:path"];

                fileName = fileName switch
                {
                    null when path == null => throw new InvalidOperationException(
                        $"Missing configuration entry for {ConfigurationName}::subscriptionFile or {ConfigurationName}::path."),

                    null => Path.Combine(path, "subscriptions.json"),
                    _ => fileName
                };

                configurer.UseJsonFile(fileName);
                return;
        }
    }
}