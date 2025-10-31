using System.Collections.Generic;

namespace CityBuilder.Domain.Models
{
    public class BuildingGrid
    {
        public int Width { get; }
        public int Height { get; }
        
        private readonly Dictionary<GridPosition, Building> _buildings;

        public BuildingGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _buildings = new Dictionary<GridPosition, Building>();
        }

        public bool IsValidPosition(GridPosition position)
        {
            return position.X >= 0 && position.X < Width &&
                   position.Y >= 0 && position.Y < Height;
        }

        public bool IsOccupied(GridPosition position)
        {
            return _buildings.ContainsKey(position);
        }

        public bool CanPlaceBuilding(GridPosition position)
        {
            return IsValidPosition(position) && !IsOccupied(position);
        }

        public void PlaceBuilding(Building building)
        {
            _buildings[building.Position] = building;
        }

        public void RemoveBuilding(GridPosition position)
        {
            _buildings.Remove(position);
        }

        public Building GetBuilding(GridPosition position)
        {
            return _buildings.TryGetValue(position, out var building) ? building : null;
        }

        public IEnumerable<Building> GetAllBuildings()
        {
            return _buildings.Values;
        }

        public void Clear()
        {
            _buildings.Clear();
        }
    }
}