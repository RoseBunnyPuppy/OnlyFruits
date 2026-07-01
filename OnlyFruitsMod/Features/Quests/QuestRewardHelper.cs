using Microsoft.Xna.Framework.Input;
using Netcode;
using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.Logging;
using OnlyFruitsMod.Features.Prices;
using OnlyFruitsMod.Infrastructure;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.SpecialOrders;
using StardewValley.Network;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Rewards;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace OnlyFruitsMod.Features.Quests
{
   
    
    public class QuestRewardHelper
    {
        public static QuestRewardHelper Instance { get; } = new QuestRewardHelper();
        
        public void SetGemRewardToZero(SpecialOrderData specialOrder)
        {
            var moneyRewards = specialOrder.Rewards.Where(x => x.Type == HardcodedQuestConstants.RewardTypes.Gems).ToArray();
            foreach (var reward in moneyRewards)
            {
                const string ValueKey = "Amount";
                if (!reward.Data.TryGetValue(ValueKey, out _))
                {
                    Logger.Instance.Monitor.Log($"Failed to find the '{ValueKey}' data for the Gem reward for special order {specialOrder.Name}", LogLevel.Error);
                    continue;
                }

                reward.Data[ValueKey] = "0";
            }
        }
        public bool SetGemRewardToZero(SpecialOrder specialOrder)
        {
            bool changedAny = false;
            foreach (var rewardField in specialOrder.NetFields.GetFields().OfType<NetList<OrderReward, NetRef<OrderReward>>>())
            {
                foreach (var gemReward in rewardField.OfType<GemsReward>())
                {
                    gemReward.amount.Value = 0;
                    changedAny = true;
                }
            }
            return changedAny;
        }
       

        private bool TryGetDataGemRewardAmount(SpecialOrderData data, out int amount)
        {
            const string AmountKey = "Amount";
            var moneyRewards = data.Rewards.Where(x => x.Type == HardcodedQuestConstants.RewardTypes.Gems).ToArray();
            var match = moneyRewards.Where(x => x.Data.ContainsKey(AmountKey)).FirstOrDefault();
            if (match == null)
            {
                amount = default;
                return false;
            }
            var strValue = match.Data[AmountKey];
            return int.TryParse(strValue, out amount);
        }
        private bool TryGetDataMoneyRewardAmount(
            SpecialOrderData data,
            [NotNullWhen(returnValue: true)] out string? strAmount
        )
        {
            const string AmountKey = "Amount";
            var moneyRewards = data.Rewards.Where(x => x.Type == HardcodedQuestConstants.RewardTypes.Money).ToArray();
            var match = moneyRewards.Where(x => x.Data.ContainsKey(AmountKey)).FirstOrDefault();
            if (match == null)
            {
                strAmount = default;
                return false;
            }
            strAmount = match.Data[AmountKey];
            return true;
        }

        public bool RestoreGemReward(SpecialOrder specialOrder)
        {
            var data = specialOrder.GetData();
            if (data == null) return false;

            foreach (var rewardField in specialOrder.NetFields.GetFields().OfType<NetList<OrderReward, NetRef<OrderReward>>>())
            {
                foreach (var gemsReward in rewardField.OfType<GemsReward>())
                {
                    if (!TryGetDataGemRewardAmount(data, out var originalAmount))
                    {
                        Debugger.Break();
                        return false;
                    }

                    gemsReward.amount.Value = originalAmount;
                    return true;
                }
            }
            return false;
        }

        public bool RestoreMoneyReward(SpecialOrder specialOrder)
        {
            var data = specialOrder.GetData();
            if (data == null) return false;
            var netFields = specialOrder.NetFields.GetFields().ToArray();
            foreach (var rewardField in netFields.OfType<NetList<OrderReward, NetRef<OrderReward>>>())
            {
                foreach (var moneyReward in rewardField.OfType<MoneyReward>())
                {
                    if (!TryGetDataMoneyRewardAmount(data, out var originalAmount))
                    {
                        Debugger.Break();
                        return false;
                    }
    
                    if (originalAmount == "{Crop:Price}")
                    {
                        var preSelected = netFields.TryGetFieldByName<NetStringDictionary<string, NetString>>(HardcodedNetFieldNames.PreSelectedItems);

                        if (preSelected == null || !preSelected.TryGetValue("Crop", out var cropString))
                            return false;

                        if (PriceLocator.Instance.TryGetFullIdPrice(cropString, out var price))
                        {
                            moneyReward.amount.Value = price;
                            return true;
                        }
                        return false;
                    }
                    else if (int.TryParse(originalAmount, out var intAmount))
                    {
                        moneyReward.amount.Value = intAmount;
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }
        public bool SetMoneyRewardToZero(SpecialOrder specialOrder)
        {
            //var data = specialOrder.GetData();
            //if (data != null)
            //{
            //    const string AmountKey = "Amount";
            //    var moneyRewards = data.Rewards.Where(x => x.Type == HardcodedQuestConstants.RewardTypes.Money).ToArray();
            //    var match = moneyRewards.Where(x => x.Data.ContainsKey(AmountKey)).Single();
            //    Debugger.Break();
            //}
            foreach (var rewardField in specialOrder.NetFields.GetFields().OfType<NetList<OrderReward, NetRef<OrderReward>>>())
            {
                foreach (var moneyReward in rewardField.OfType<MoneyReward>())
                {
                    //var cachedMoneyField = specialOrder.GetCachedMoneyField();
                    //if (cachedMoneyField.Value == -1)
                    //{
                    //    var fallback = GetMoneyReward(specialOrder.GetData());
                    //    cachedMoneyField.Value = fallback;//  moneyReward.amount.Value;
                    //}
                    //else
                    //{
                    //    _ = 23;
                    //}

                    moneyReward.amount.Value = 0;
                    
                    return true;
                }
            }
            return false;
        }
        
        public void SetMoneyRewardToZero(SpecialOrderData specialOrder)
        {
            var moneyRewards = specialOrder.Rewards.Where(x => x.Type == HardcodedQuestConstants.RewardTypes.Money).ToArray();
            foreach (var reward in moneyRewards)
            {
                const string ValueKey = "Amount";
                if (!reward.Data.TryGetValue(ValueKey, out _))
                {
                    Logger.Instance.Monitor.Log($"Failed to find the '{ValueKey}' data for the Money reward for special order {specialOrder.Name}", LogLevel.Error);
                    continue;
                }
                reward.Data[ValueKey] = "0";
            }
        }

    }
}
