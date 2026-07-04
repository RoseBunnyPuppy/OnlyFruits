using OnlyFruitsMod.Models;
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
    public class Logger : ILogger
    {
        static Logger FullInstance { get; } = new Logger();
        public static ILogger Instance => FullInstance;



        /// <inheritdoc/>
        public OnlyFruitsLogLevels MaxLogLevel { get; set; } = ModConfig.DefaultLogLevel;

        private IMonitor? monitor;

        /// <inheritdoc/>
        public IMonitor Monitor => monitor ?? throw new InvalidOperationException("No IMonitor is configured.");


       
        /// <inheritdoc/>
        public bool CanLog(LogLevel level)
        {
            switch (this.MaxLogLevel)
            {
                case OnlyFruitsLogLevels.None: return false;
                case OnlyFruitsLogLevels.All: return true;
                case OnlyFruitsLogLevels.Debug:
                    return level switch
                    {
                        LogLevel.Debug => true,
                        LogLevel.Info => true,
                        LogLevel.Warn => true,
                        LogLevel.Error => true,
                        _ => false,
                    };
                case OnlyFruitsLogLevels.Info:
                    return level switch
                    {
                        LogLevel.Info => true,
                        LogLevel.Warn => true,
                        LogLevel.Error => true,
                        _ => false,
                    };
                case OnlyFruitsLogLevels.Error:
                    return level switch
                    {
                        LogLevel.Error => true,
                        _ => false,
                    };
                default:
                    return false;
            }
        }


        public void SetMonitor(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        
        /// <inheritdoc/>
        public void Log(string message, LogLevel level)
        {
            if (!this.CanLog(level)) return;

            this.Monitor.Log(message, level);
        }


    
        /// <inheritdoc/>
        public void LogOnce(string message, LogLevel level)
        {
            if (!this.CanLog(level)) return;

            this.Monitor.LogOnce(message, level);
        }
    }
}
