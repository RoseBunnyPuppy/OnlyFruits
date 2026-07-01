using Netcode;

namespace OnlyFruitsMod.Features.Quests
{
    public static class FieldValueEventHandlers
    {
        public static void PreserveOldValue(NetString field, string oldValue, string newValue)
        {
            if (oldValue == newValue) return;
            field.fieldChangeEvent -= PreserveOldValue;
            field.Value = oldValue;
            field.fieldChangeEvent += PreserveOldValue;
        }

        public static void PreserveOldValue(NetInt field, int oldValue, int newValue)
        {
            if (oldValue == newValue) return;
            field.fieldChangeEvent -= PreserveOldValue;
            field.Value = oldValue;
            field.fieldChangeEvent += PreserveOldValue;
        }

        public static void ResetFieldToZeroHandler(NetInt field, int oldValue, int newValue)
        {
            if (oldValue == newValue) return;
            field.Value = 0;
        }
    }
}
