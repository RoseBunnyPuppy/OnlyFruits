using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Models;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace OnlyFruitsMod.ModParts.Core
{
    public class ModPartBase : IModPart
    {
        protected ModPartContext Context { get; }
        protected readonly IModHelper helper;
        protected readonly IMonitor monitor;
        protected readonly ModConfigInstance configInstance;

        public ModPartBase(
            ModPartContext context
        )
        {
            this.Context = context;
            this.helper = context.Helper;
            this.monitor = context.Monitor;
            this.configInstance = context.ConfigInstance;
        }


        public void Run()
        {
            this.AttachListeners();
            this.LoadNeededAssets();
        }

        /// <summary>
        ///     Attach handlers to the various events.  Auto-listens to
        ///   the <see cref="IContentEvents.AssetRequested"/>,
        ///   and the <see cref="ModConfigInstance.Changed"/> events.
        /// </summary>
        /// 
        protected virtual void AttachListeners()
        {
            this.helper.Events.Content.AssetRequested += this.OnAssetRequested;
            this.configInstance.Changed += OnModConfigChanged; ;
        }

       
        protected virtual void OnModConfigChanged(object? sender, EventArgs e)
        {

        }

        /// <summary>
        ///   Load any required gamecontent
        /// </summary>
        protected virtual void LoadNeededAssets()
        {
        }

      
        protected virtual void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {

        }
    }
}
