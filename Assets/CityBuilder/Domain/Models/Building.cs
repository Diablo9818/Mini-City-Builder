namespace CityBuilder.Domain.Models
{
    public class Building
    {
        public string Id { get; }
        public BuildingType Type { get; }
        public GridPosition Position { get; private set; }
        public int Level { get; private set; }

        public Building(string id, BuildingType type, GridPosition position, int level = 1)
        {
            Id = id;
            Type = type;
            Position = position;
            Level = level;
        }

        public void SetPosition(GridPosition newPosition)
        {
            Position = newPosition;
        }

        public void Upgrade()
        {
            Level++;
        }

        public bool CanUpgrade(int maxLevel)
        {
            return Level < maxLevel;
        }
    }
}