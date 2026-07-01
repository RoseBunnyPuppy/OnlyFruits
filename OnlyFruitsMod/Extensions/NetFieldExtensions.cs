using Netcode;
using OnlyFruitsMod.Features.Quests;

namespace OnlyFruitsMod.Extensions
{
    public static class NetFieldExtensions
    {
        /// <summary>
        ///     Performs an action while the <see cref="FieldValueEventHandlers.PreserveOldValue(NetInt, int, int)"/>
        ///   handler is locking the initial value of the field.
        /// </summary>
        public static void WithLockedField(this NetInt field, Action action)
        {
            field.fieldChangeEvent += FieldValueEventHandlers.PreserveOldValue;
            action();
            field.fieldChangeEvent -= FieldValueEventHandlers.PreserveOldValue;
        }

        /// <summary>
        ///     Performs an action while the <see cref="FieldValueEventHandlers.PreserveOldValue(NetInt, string, string)"/>
        ///   handler is locking the initial value of the field.
        /// </summary>
        public static void WithLockedField(this NetString field, Action action)
        {
            field.fieldChangeEvent += FieldValueEventHandlers.PreserveOldValue;
            action();
            field.fieldChangeEvent -= FieldValueEventHandlers.PreserveOldValue;
        }


        public static TField? TryGetFieldByName<TField>(this NetFields fields, string name)
            where TField : INetSerializable
        {
            return fields.GetFields().TryGetFieldByName<TField>(name);
        }

        public static TField? TryGetFieldByName<TField>(this IEnumerable<INetSerializable> fields, string name)
           where TField : INetSerializable
        {
            return fields
                .Where(field => field.Name == name)
                .OfType<TField>()
                .FirstOrDefault()
            ;
        }
    }
}
