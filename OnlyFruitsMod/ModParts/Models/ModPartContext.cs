using OnlyFruitsMod.Features.ModConfiguration;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.ModParts.Models
{
    public record ModPartContext(
        IModHelper Helper,
        IMonitor Monitor,
        ModConfigInstance ConfigInstance,
        IManifest ModManifest
    );
}
