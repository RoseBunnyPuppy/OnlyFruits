using Netcode;

namespace OnlyFruitsMod.Extensions
{
    public static class NetFieldExtensions
    {
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
