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

        /// <summary>
        ///   The current configuration data.
        /// </summary>
        public ModConfig Config { get; private set; }

        /// <summary>
        ///   Raised when the persisted configuration changes.
        /// </summary>
        public event EventHandler? Changed;


        public ModConfigInstance(
            IModHelper helper
        ) {
            this.helper = helper;
            this.Config = this.helper.ReadConfig<ModConfig>();
        }

        public void Reload()
        {
            this.Config = this.helper.ReadConfig<ModConfig>();
            this.Save();
        }

        private void RaiseChanged()
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Write the configuration to disk, 
        ///   raise the <see cref="Changed"/> envent.
        /// </summary>
        public void Save()
        {
            this.helper.WriteConfig(this.Config);
            this.RaiseChanged();
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
