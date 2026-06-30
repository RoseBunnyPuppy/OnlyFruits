using OnlyFruitsMod.Models;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.ModConfiguration
{
    public class ModConfigInstance
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;

        /// <summary>
        ///   The current configuration data.
        /// </summary>
        public ModConfig Config { get; private set; }
        private bool PreviousAlwaysAskStatus { get; set; }

        /// <summary>
        ///   Raised when the persisted configuration changes.
        /// </summary>
        public event EventHandler? Changed;

        public event EventHandler? AlwaysAskAboutChallengeChanged;


        public ModConfigInstance(
            IModHelper helper,
            IMonitor monitor
        ) {
            this.helper = helper;
            this.monitor = monitor;
            this.Config = this.helper.ReadConfig<ModConfig>();
            this.PreviousAlwaysAskStatus = this.Config.AlwaysAskWhetherToUseChallenge;
        }


        public void RaiseChanged()
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }
        public void RaiseAlwaysAskChanged()
        {
            this.AlwaysAskAboutChallengeChanged?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        ///     Write the configuration to disk, 
        ///   raise the <see cref="Changed"/> envent.
        /// </summary>
        public void Save()
        {
            var prev = this.PreviousAlwaysAskStatus;
            var current = this.Config.AlwaysAskWhetherToUseChallenge;
            
            this.PreviousAlwaysAskStatus = current;
            this.helper.WriteConfig(this.Config);
            this.RaiseChanged();

            if (prev != current)
                this.RaiseAlwaysAskChanged();
        }

        /// <summary>
        ///     Reset the config to the default values,
        ///   writes the configuration to disk, 
        ///   raise the <see cref="Changed"/> envent.
        /// </summary>
        public void Reset()
        {
            this.Config = new ModConfig();
            this.Save();
        }
    }
}
