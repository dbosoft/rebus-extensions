using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Transport;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class ChainedTransportSelector : RebusTransportSelectorBase
{
    private readonly ILogger _log;
    private readonly IDictionary<string, List<RebusTransportSelectorBase>> _selectors = new Dictionary<string, List<RebusTransportSelectorBase>>();

    public ChainedTransportSelector(IConfiguration configuration, ILogger log,
        IEnumerable<RebusTransportSelectorBase> selectors) : base(configuration, log)
    {
        _log = log;
        var selectorsArray = selectors as RebusTransportSelectorBase[] ?? selectors.ToArray();

        if (!selectorsArray.Any())
        {
            throw new ArgumentException("Selector list is empty", nameof(selectors));
        }

        var names = selectorsArray.Select(x => x.ConfigurationName).Distinct().ToArray();
        if (names.Length > 1)
        {
            throw new ArgumentException(
                $"All rebus selectors in a chained selector have to use the same configuration name. Found names: {string.Join(',', names)}", nameof(selectors));

        }

        AcceptedConfigTypes = selectorsArray.SelectMany(x => x.AcceptedConfigTypes).Distinct().ToArray();
        ConfigurationName = names[0];
        foreach (var selector in selectorsArray)
        {
            foreach (var configType in selector.AcceptedConfigTypes)
            {
                List<RebusTransportSelectorBase> list;
                if (_selectors.TryGetValue(configType, out var foundSelectors))
                {
                    list = foundSelectors;
                }
                else
                {
                    list = new List<RebusTransportSelectorBase>();
                    _selectors.Add(configType, list);
                }

                list.Add(selector);
            }
        }
    }

    protected override void ConfigureBusTypeAsOneWayClient(string busType, StandardConfigurer<ITransport> configurer)
    {

        foreach (var selector in _selectors[busType])
        {
            _log.LogTrace("Trying to configure rebus with selector {selectorType}", selector.GetType());
            try
            {
                selector.ConfigureAsOneWayClient(configurer);
                _log.LogDebug("Using rebus selector {selectorType}", selector.GetType());
                break;
            }
            catch (Exception ex)
            {
                _log.LogTrace(ex, "selector {selectorType} configuration failed", selector.GetType());

            }
        }
    }

    protected override void ConfigureBusType(string busType, string queueName, StandardConfigurer<ITransport> configurer)
    {

        foreach (var selector in _selectors[busType])
        {
            _log.LogTrace("Trying to configure rebus with selector {selectorType}", selector.GetType());
            try
            {
                selector.Configure(configurer, queueName);
                _log.LogDebug("Using rebus selector {selectorType}", selector.GetType());
                break;
            }
            catch (Exception ex)
            {
                _log.LogTrace(ex, "selector {selectorType} configuration failed", selector.GetType());

            }
        }
    }


    public override string[] AcceptedConfigTypes { get; }
    public override string ConfigurationName { get; }
}