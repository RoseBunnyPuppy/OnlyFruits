namespace OnlyFruitsMod.Infrastructure
{
    public class AssetDefinition<TAsset> 
        where TAsset : notnull
    {
        public string Name { get; set; }

        public AssetDefinition(string name)
        {
            this.Name = name;
        }

        public static implicit operator string(AssetDefinition<TAsset> asset) => asset.Name;
        public static implicit operator AssetDefinition<TAsset>(string name) => new(name);
    }
}
