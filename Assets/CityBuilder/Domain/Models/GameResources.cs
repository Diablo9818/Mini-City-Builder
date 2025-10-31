namespace CityBuilder.Domain.Models
{
    public class GameResources
    {
        public int Gold { get; private set; }

        public GameResources(int initialGold = 1000)
        {
            Gold = initialGold;
        }

        public bool CanAfford(Cost cost)
        {
            return Gold >= cost.Gold;
        }

        public void Spend(Cost cost)
        {
            Gold -= cost.Gold;
        }

        public void Add(int gold)
        {
            Gold += gold;
        }

        public void SetGold(int gold)
        {
            Gold = gold;
        }
    }
}