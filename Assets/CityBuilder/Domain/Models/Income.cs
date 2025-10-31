namespace CityBuilder.Domain.Models
{
    public struct Income
    {
        public int GoldPerTick { get; }

        public Income(int goldPerTick)
        {
            GoldPerTick = goldPerTick;
        }

        public static Income Zero => new Income(0);
    }
}
