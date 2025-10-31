using CityBuilder.Domain.Models;
using UnityEngine;
using TMPro;

namespace CityBuilder.Presentation.Views
{
    public class BuildingView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private TextMeshPro _levelText;
        [SerializeField] private Transform _visualRoot;

        private Building _building;

        public Building Building => _building;

        public void Initialize(Building building, Material material)
        {
            _building = building;
            
            if (_meshRenderer != null && material != null)
            {
                _meshRenderer.material = material;
            }
            
            transform.position = new Vector3(building.Position.X, 0, building.Position.Y);
            UpdateLevelDisplay();
        }

        public void UpdateLevel()
        {
            UpdateLevelDisplay();
        }

        public void UpdatePosition()
        {
            transform.position = new Vector3(_building.Position.X, 0, _building.Position.Y);
        }

        private void UpdateLevelDisplay()
        {
            if (_levelText != null)
            {
                _levelText.text = $"Lv{_building.Level}";
                _levelText.transform.localPosition = new Vector3(0, 2f, 0);
            }
        }
    }
}