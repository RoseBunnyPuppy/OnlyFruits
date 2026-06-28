using StardewModdingAPI;
using System.Diagnostics;

namespace OnlyFruitsMod.Features.Quests
{
    public class QuestStatusLogger
    {
        private readonly IMonitor monitor;
        private readonly string source;
        public bool Verbose { get; set; } = true;

        public Dictionary<string, OrderPatchingFlavors?> LiveStatuses { get; } = new();
        public Dictionary<string, OrderPatchingFlavors?> AssetStatuses { get; } = new();

        public QuestStatusLogger(
            IMonitor monitor,
            string source
        )
        {
            this.monitor = monitor;
            this.source = source;
        }

        public void Reset()
        {
            this.LiveStatuses.Clear();
            this.AssetStatuses.Clear();
        }

        public void SetQuestIds(IEnumerable<string> questIds)
        {
            this.Reset();
            foreach (var id in questIds)
            {
                this.AssetStatuses[id] = null;
                this.LiveStatuses[id] = null;
            }
        }

        private Dictionary<string, OrderPatchingFlavors?> GetMap(bool isAsset)
        {
            if (isAsset) return this.AssetStatuses;
            return this.LiveStatuses;
        }
        public void LogQuestStatus(bool isAsset, string questId, OrderPatchingFlavors flavor, bool dontOverride = true)
        {
            var scope = isAsset ? "Asset" : "Live";
            var statusMap = this.GetMap(isAsset);
            if (this.Verbose) this.monitor.Log($"[{this.source} | {scope}] {questId}: {flavor}", LogLevel.Debug);
            if (!dontOverride)
            {
                statusMap[questId] = flavor;
                return;
            }
            if (statusMap.TryGetValue(questId, out var current))
            {
                if (!current.HasValue)
                {
                    statusMap[questId] = flavor;
                    return;
                }
                Debugger.Break();
            }
            statusMap[questId] = flavor;
        }

        public Dictionary<string, OrderPatchingFlavors?> GetData(bool isAsset)
        {
            return new Dictionary<string, OrderPatchingFlavors?>(this.GetMap(isAsset));
        }
        public IEnumerable<string> GetMissing(bool isAsset) => this.GetMap(isAsset).Where(kvp => kvp.Value == null).Select(kvp => kvp.Key);
    }
}