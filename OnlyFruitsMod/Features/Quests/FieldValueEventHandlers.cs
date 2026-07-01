using Netcode;

namespace OnlyFruitsMod.Features.Quests
{
    public static class FieldValueEventHandlers
    {
        /// <summary>
        ///     Updates the value of <paramref name="field"/> to always be
        ///   <paramref name="oldValue"/> whenever it is updated.  This 
        ///   handler should NOT cause an infinite loop since the
        ///   handler is removed before restoring the old value.
        /// </summary>
        public static void PreserveOldValue(NetString field, string oldValue, string newValue)
        {
            if (oldValue == newValue) return;
            field.fieldChangeEvent -= PreserveOldValue;
            field.Value = oldValue;
            field.fieldChangeEvent += PreserveOldValue;
        }

        /// <summary>
        ///     Updates the value of <paramref name="field"/> to always be
        ///   <paramref name="oldValue"/> whenever it is updated.  This 
        ///   handler should NOT cause an infinite loop since the
        ///   handler is removed before restoring the old value.
        /// </summary>
        public static void PreserveOldValue(NetInt field, int oldValue, int newValue)
        {
            if (oldValue == newValue) return;
            field.fieldChangeEvent -= PreserveOldValue;
            field.Value = oldValue;
            field.fieldChangeEvent += PreserveOldValue;
        }

        /// <summary>
        ///     Updates the value of <paramref name="field"/> to always be
        ///   0 whenever it is updated.  This 
        ///   handler should NOT cause an infinite loop since the
        ///   handler is removed before restoring the old value.
        /// </summary>
        public static void ResetFieldToZeroHandler(NetInt field, int oldValue, int newValue)
        {
            if (oldValue == newValue) return;
            field.fieldChangeEvent -= ResetFieldToZeroHandler;
            field.Value = 0;
            field.fieldChangeEvent += ResetFieldToZeroHandler;
        }
    }
}
