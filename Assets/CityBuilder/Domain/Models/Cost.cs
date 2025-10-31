namespace CityBuilder.Domain.Models
{
    public struct Cost
    {
        public int Gold { get; }

        public Cost(int gold)
        {
            Gold = gold;
        }

        public static Cost Zero => new Cost(0);
    }
}