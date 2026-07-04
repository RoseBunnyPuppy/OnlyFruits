using OnlyFruitsMod.Models;
using StardewModdingAPI;

namespace OnlyFruitsMod.Features.Logging
{
    public interface ILogger
    {
        /// <summary>
        ///   The maximum log level the user has configured.
        /// </summary>
        OnlyFruitsLogLevels MaxLogLevel { get; set; }

        /// <summary>
        ///   The underlying thing we'll log to.
        /// </summary>
        IMonitor Monitor { get; }

        /// <summary>
        ///     Returns a value indicating whether the specified <paramref name="level"/>
        ///   is allowed for the currently configured <see cref="MaxLogLevel"/>.
        /// </summary>
        bool CanLog(LogLevel level);

        /// <summary>Log a message for the player or developer.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The log severity level.</param>
        void Log(string message, LogLevel level);

        /// <summary>Log a message for the player or developer, but only if it hasn't already been logged since the last game launch.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The log severity level.</param>
        public void LogOnce(string message, LogLevel level);

    }
}
