using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Tests.Data;

public class TestProgressModel
{
    public DateTimeOffset Timestamp { get; set; }

    public object? Data { get; set; }
}
