using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.Quests;
using OnlyFruitsMod.Features.Quests.Models;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.Models;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.SpecialOrders;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OnlyFruitsMod.ModParts
{
    /// <summary>
    ///   Removes monetary rewards for monster eradication quests.
    /// </summary>
    public class MonsterSlayerQuestsModPart : ModPartBase
    {
        public bool PreloadAssets { get; set; } = PreloadConfiguration.MonsterSlayer;
        public MonsterSlayerQuestsModPart(
            ModPartContext context
        ) : base(context)
        {

        }

        /// <summary>
        ///     When the configuration changes, invalidate the assets.
        /// </summary>
        protected override void OnModConfigChanged(object? sender, EventArgs e)
        {
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataMonsterSlayerQuests);
        }

        /// <inheritdoc/>
        protected override void LoadNeededAssets()
        {
            // dont pre-load shit unless needed
            if (!this.PreloadAssets) return;

            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataMonsterSlayerQuests);
        }


        private bool ShouldPatch(string slayerKey)
        {
            // dont patch if the challenge isnt enabled.
            if (!this.Context.PerSaveChallengeInstance.IsChallengeEnabled) return false;
            // dont patch if we _ARENT_ patching non-fruity slayer quests
            if (!this.configInstance.Config.Questing_NoMoneyFromNonFruityMonsterSlayerQuests) return false;

            // remove rewards for all but dinos (because they are `Pepper Rex`)
            return slayerKey == "Dinos";
        }

        /// <inheritdoc/>
        protected override void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataMonsterSlayerQuests))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataMonsterSlayerQuests).Data;

                    foreach (var kvp in data)
                    {
                        if (!this.ShouldPatch(kvp.Key)) continue;

                        kvp.Value.RewardItemPrice = 0;
                    }
                });
                return;
            }
           
        }

    }
}