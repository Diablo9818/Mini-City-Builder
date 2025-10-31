using System.Collections.Generic;
using CityBuilder.Domain.Models;
using UnityEngine;

namespace CityBuilder.Presentation.Views
{
    [CreateAssetMenu(fileName = "BuildingFactory", menuName = "CityBuilder/BuildingFactory")]
    public class BuildingFactory : ScriptableObject
    {
        [SerializeField] private BuildingPrefabData[] _prefabs;
        
        private Dictionary<BuildingType, BuildingPrefabData> _prefabMap;

        private void OnEnable()
        {
            _prefabMap = new Dictionary<BuildingType, BuildingPrefabData>();
            foreach (var data in _prefabs)
            {
                _prefabMap[data.Type] = data;
            }
        }

        public BuildingView CreateBuilding(Building building, Transform parent)
        {
            if (_prefabMap == null)
            {
                OnEnable();
            }

            if (!_prefabMap.TryGetValue(building.Type, out var data))
            {
                Debug.LogError($"No prefab found for building type: {building.Type}");
                return null;
            }

            var instance = Instantiate(data.Prefab, parent);
            instance.Initialize(building, data.Material); // ← Здесь Material, не Sprite!
            return instance;
        }

        public Material GetMaterial(BuildingType type)
        {
            if (_prefabMap == null)
            {
                OnEnable();
            }

            return _prefabMap.TryGetValue(type, out var data) ? data.Material : null;
        }

        [System.Serializable]
        public class BuildingPrefabData
        {
            public BuildingType Type;
            public BuildingView Prefab;
            public Material Material; // ← Material для 3D
        }
    }
}