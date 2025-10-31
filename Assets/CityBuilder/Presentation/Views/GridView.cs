using System;
using System.Collections.Generic;
using CityBuilder.Domain.Models;
using UnityEngine;

namespace CityBuilder.Presentation.Views
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Material _validMaterial;
        [SerializeField] private Material _invalidMaterial;
        [SerializeField] private Material _normalMaterial;
        
        private BuildingGrid _grid;
        private GameObject[,] _cells;
        private GameObject _highlightedCell;

        public void Initialize(BuildingGrid grid)
        {
            _grid = grid;
            CreateGrid();
        }

        private void CreateGrid()
        {
            Debug.Log("Creating grid");
            _cells = new GameObject[_grid.Width, _grid.Height];

            for (var y = 0; y < _grid.Height; y++)
            {
                for (var x = 0; x < _grid.Width; x++)
                {
                    var cell = Instantiate(_cellPrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                    cell.name = $"Cell_{x}_{y}";
                    _cells[x, y] = cell;
                    
                    var renderer = cell.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = _normalMaterial;
                    }
                }
            }
        }

        public void HighlightCell(GridPosition position, bool canPlace)
        {
            ClearHighlight();

            if (!_grid.IsValidPosition(position)) return;

            _highlightedCell = _cells[position.X, position.Y];
            var renderer = _highlightedCell.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = canPlace ? _validMaterial : _invalidMaterial;
            }
        }

        public void ClearHighlight()
        {
            if (_highlightedCell != null)
            {
                var renderer = _highlightedCell.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = _normalMaterial;
                }
                _highlightedCell = null;
            }
        }

        public GridPosition? GetGridPositionFromWorld(Vector3 worldPosition)
        {
            if (_grid == null) return null;
            
            var x = Mathf.RoundToInt(worldPosition.x);
            var y = Mathf.RoundToInt(worldPosition.z);
            
            var position = new GridPosition(x, y);
            return _grid.IsValidPosition(position) ? position : null;
        }
    }
}