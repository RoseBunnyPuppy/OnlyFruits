using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.Logging
{
    /// <summary>
    ///     Provides access to a singleton logger so I dont have
    ///   to pass around a <see cref="IMonitor"/> everywhere.
    /// </summary>
    public class Logger
    {
        public static Logger Instance { get; } = new Logger();

        private IMonitor? monitor;
        public IMonitor Monitor => monitor ?? throw new InvalidOperationException("No IMonitor is configured.");
        public void SetMonitor(IMonitor monitor)
        {
            this.monitor = monitor;
        }
    }
}
