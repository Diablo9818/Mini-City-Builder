using System;
using System.Collections.Generic;
using CityBuilder.Application.DTOs;
using CityBuilder.Application.Events;
using CityBuilder.Domain.Models;
using CityBuilder.Infrastructure.Services;
using CityBuilder.Presentation.Views;
using MessagePipe;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using MessagePipeDisposableBag = MessagePipe.DisposableBag;

namespace CityBuilder.Presentation.Presenters
{
    public class GamePresenter : MonoBehaviour
    {
        [SerializeField] private GridView _gridView;
        [SerializeField] private BuildingFactory _buildingFactory;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private UIDocument _uiDocument;

        private BuildingGrid _grid;
        private Dictionary<string, BuildingView> _buildingViews;
        private InputActions _inputActions;
        
        private IPublisher<PlaceBuildingDto> _placeBuildingPublisher;
        private IPublisher<RemoveBuildingDto> _removeBuildingPublisher;
        private IPublisher<MoveBuildingDto> _moveBuildingPublisher;
        private IPublisher<UpgradeBuildingDto> _upgradeBuildingPublisher;
        private IPublisher<SaveGameDto> _saveGamePublisher;
        private IPublisher<LoadGameDto> _loadGamePublisher;

        private IDisposable _subscriptions;
        
        private BuildingType? _selectedBuildingType;
        private Building _selectedBuilding;
        private bool _isMovingMode;

        private Label _goldLabel;
        private Label _messageLabel;
        private Button _saveButton;
        private Button _loadButton;
        private Button _upgradeButton;
        private Button _moveButton;
        private Button _deleteButton;
        private VisualElement _buildingPanel;
        private VisualElement _selectionPanel;

        public void Initialize(
            BuildingGrid grid,
            IPublisher<PlaceBuildingDto> placeBuildingPublisher,
            IPublisher<RemoveBuildingDto> removeBuildingPublisher,
            IPublisher<MoveBuildingDto> moveBuildingPublisher,
            IPublisher<UpgradeBuildingDto> upgradeBuildingPublisher,
            IPublisher<SaveGameDto> saveGamePublisher,
            IPublisher<LoadGameDto> loadGamePublisher,
            ISubscriber<BuildingPlacedEvent> buildingPlacedSubscriber,
            ISubscriber<BuildingRemovedEvent> buildingRemovedSubscriber,
            ISubscriber<BuildingMovedEvent> buildingMovedSubscriber,
            ISubscriber<BuildingUpgradedEvent> buildingUpgradedSubscriber,
            ISubscriber<ResourcesChangedEvent> resourcesChangedSubscriber,
            ISubscriber<InsufficientResourcesEvent> insufficientResourcesSubscriber,
            ISubscriber<GameSavedEvent> gameSavedSubscriber,
            ISubscriber<GameLoadedEvent> gameLoadedSubscriber)
        {
            _grid = grid;
            _buildingViews = new Dictionary<string, BuildingView>();
            
            _placeBuildingPublisher = placeBuildingPublisher;
            _removeBuildingPublisher = removeBuildingPublisher;
            _moveBuildingPublisher = moveBuildingPublisher;
            _upgradeBuildingPublisher = upgradeBuildingPublisher;
            _saveGamePublisher = saveGamePublisher;
            _loadGamePublisher = loadGamePublisher;

            _gridView.Initialize(_grid);
            InitializeUI();
            InitializeInput();
            SubscribeToEvents(
                buildingPlacedSubscriber,
                buildingRemovedSubscriber,
                buildingMovedSubscriber,
                buildingUpgradedSubscriber,
                resourcesChangedSubscriber,
                insufficientResourcesSubscriber,
                gameSavedSubscriber,
                gameLoadedSubscriber);
        }

        private void InitializeUI()
        {
            var root = _uiDocument.rootVisualElement;
            
            _goldLabel = root.Q<Label>("GoldLabel");
            _messageLabel = root.Q<Label>("MessageLabel");
            _saveButton = root.Q<Button>("SaveButton");
            _loadButton = root.Q<Button>("LoadButton");
            
            _buildingPanel = root.Q<VisualElement>("BuildingPanel");
            var houseButton = _buildingPanel.Q<Button>("HouseButton");
            var farmButton = _buildingPanel.Q<Button>("FarmButton");
            var mineButton = _buildingPanel.Q<Button>("MineButton");

            _selectionPanel = root.Q<VisualElement>("SelectionPanel");
            _upgradeButton = _selectionPanel.Q<Button>("UpgradeButton");
            _moveButton = _selectionPanel.Q<Button>("MoveButton");
            _deleteButton = _selectionPanel.Q<Button>("DeleteButton");

            _selectionPanel.style.display = DisplayStyle.None;

            houseButton.clicked += () => SelectBuildingType(BuildingType.House);
            farmButton.clicked += () => SelectBuildingType(BuildingType.Farm);
            mineButton.clicked += () => SelectBuildingType(BuildingType.Mine);

            _saveButton.clicked += () => _saveGamePublisher.Publish(new SaveGameDto());
            _loadButton.clicked += () => _loadGamePublisher.Publish(new LoadGameDto());

            _upgradeButton.clicked += OnUpgradeClicked;
            _moveButton.clicked += OnMoveClicked;
            _deleteButton.clicked += OnDeleteClicked;
        }

        private void InitializeInput()
        {
            _inputActions = new InputActions();
            _inputActions.Enable();

            _inputActions.Building.Select1.performed += _ => SelectBuildingType(BuildingType.House);
            _inputActions.Building.Select2.performed += _ => SelectBuildingType(BuildingType.Farm);
            _inputActions.Building.Select3.performed += _ => SelectBuildingType(BuildingType.Mine);
            _inputActions.Building.Delete.performed += _ => OnDeleteClicked();
            _inputActions.Building.Click.performed += OnClickPerformed;
        }

        private void Update()
        {
            UpdateCursorHighlight();
        }

        private void UpdateCursorHighlight()
        {
            var mousePos = Mouse.current.position.ReadValue();
            
            Ray ray = _mainCamera.ScreenPointToRay(mousePos);
            
            Plane groundPlane = new Plane(Vector3.up, 0f); 
    
            if (groundPlane.Raycast(ray, out float distance))
            {
                var worldPos = ray.GetPoint(distance);
                var gridPos = _gridView.GetGridPositionFromWorld(worldPos);

                if (gridPos.HasValue)
                {
                    var canPlace = _selectedBuildingType.HasValue && _grid.CanPlaceBuilding(gridPos.Value);
                    _gridView.HighlightCell(gridPos.Value, canPlace);
                }
                else
                {
                    _gridView.ClearHighlight();
                }
            }
            else
            {
                _gridView.ClearHighlight(); 
            }
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            var mousePos = Mouse.current.position.ReadValue();
            
            Ray ray = _mainCamera.ScreenPointToRay(mousePos);
            
            Plane groundPlane = new Plane(Vector3.up, 0f); 
    
            if (groundPlane.Raycast(ray, out float distance))
            {
                var worldPos = ray.GetPoint(distance);
                var gridPos = _gridView.GetGridPositionFromWorld(worldPos);

                if (!gridPos.HasValue) return;
        
             

                if (_isMovingMode && _selectedBuilding != null)
                {
                    _moveBuildingPublisher.Publish(new MoveBuildingDto
                    {
                        BuildingId = _selectedBuilding.Id,
                        NewPosition = gridPos.Value
                    });
                    _isMovingMode = false;
                    DeselectBuilding();
                }
                else if (_selectedBuildingType.HasValue)
                {
                    _placeBuildingPublisher.Publish(new PlaceBuildingDto
                    {
                        Type = _selectedBuildingType.Value,
                        Position = gridPos.Value
                    });
                }
                else
                {
                    var building = _grid.GetBuilding(gridPos.Value);
                    if (building != null)
                    {
                        SelectBuilding(building);
                    }
                    else
                    {
                        DeselectBuilding();
                    }
                }
            }
            else
            {
                DeselectBuilding(); 
            }
        }

        private void SelectBuildingType(BuildingType type)
        {
            _selectedBuildingType = type;
            _isMovingMode = false;
            DeselectBuilding();
            ShowMessage($"Selected: {type}");
        }

        private void SelectBuilding(Building building)
        {
            _selectedBuilding = building;
            _selectedBuildingType = null;
            _selectionPanel.style.display = DisplayStyle.Flex;
            ShowMessage($"Selected: {building.Type} Lv{building.Level}");
        }

        private void DeselectBuilding()
        {
            _selectedBuilding = null;
            _selectionPanel.style.display = DisplayStyle.None;
        }

        private void OnUpgradeClicked()
        {
            if (_selectedBuilding == null) return;

            _upgradeBuildingPublisher.Publish(new UpgradeBuildingDto
            {
                BuildingId = _selectedBuilding.Id
            });
        }

        private void OnMoveClicked()
        {
            if (_selectedBuilding == null) return;

            _isMovingMode = true;
            ShowMessage("Click on a new position to move the building");
        }

        private void OnDeleteClicked()
        {
            if (_selectedBuilding == null) return;

            _removeBuildingPublisher.Publish(new RemoveBuildingDto
            {
                BuildingId = _selectedBuilding.Id
            });
            DeselectBuilding();
        }

        private void SubscribeToEvents(
            ISubscriber<BuildingPlacedEvent> buildingPlacedSubscriber,
            ISubscriber<BuildingRemovedEvent> buildingRemovedSubscriber,
            ISubscriber<BuildingMovedEvent> buildingMovedSubscriber,
            ISubscriber<BuildingUpgradedEvent> buildingUpgradedSubscriber,
            ISubscriber<ResourcesChangedEvent> resourcesChangedSubscriber,
            ISubscriber<InsufficientResourcesEvent> insufficientResourcesSubscriber,
            ISubscriber<GameSavedEvent> gameSavedSubscriber,
            ISubscriber<GameLoadedEvent> gameLoadedSubscriber)
        {
            var bag = MessagePipeDisposableBag.CreateBuilder();

            buildingPlacedSubscriber.Subscribe(OnBuildingPlaced).AddTo(bag);
            buildingRemovedSubscriber.Subscribe(OnBuildingRemoved).AddTo(bag);
            buildingMovedSubscriber.Subscribe(OnBuildingMoved).AddTo(bag);
            buildingUpgradedSubscriber.Subscribe(OnBuildingUpgraded).AddTo(bag);
            resourcesChangedSubscriber.Subscribe(OnResourcesChanged).AddTo(bag);
            insufficientResourcesSubscriber.Subscribe(OnInsufficientResources).AddTo(bag);
            gameSavedSubscriber.Subscribe(_ => ShowMessage("Game Saved!")).AddTo(bag);
            gameLoadedSubscriber.Subscribe(_ => OnGameLoaded()).AddTo(bag);

            _subscriptions = bag.Build();
        }

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            var view = _buildingFactory.CreateBuilding(evt.Building, transform);
            _buildingViews[evt.Building.Id] = view;
            ShowMessage($"{evt.Building.Type} placed!");
        }

        private void OnBuildingRemoved(BuildingRemovedEvent evt)
        {
            if (_buildingViews.TryGetValue(evt.BuildingId, out var view))
            {
                Destroy(view.gameObject);
                _buildingViews.Remove(evt.BuildingId);
            }
            ShowMessage("Building removed!");
        }

        private void OnBuildingMoved(BuildingMovedEvent evt)
        {
            if (_buildingViews.TryGetValue(evt.Building.Id, out var view))
            {
                view.UpdatePosition();
            }
            ShowMessage("Building moved!");
        }

        private void OnBuildingUpgraded(BuildingUpgradedEvent evt)
        {
            if (_buildingViews.TryGetValue(evt.Building.Id, out var view))
            {
                view.UpdateLevel();
            }
            ShowMessage($"Building upgraded to Lv{evt.Building.Level}!");
        }

        private void OnResourcesChanged(ResourcesChangedEvent evt)
        {
            _goldLabel.text = $"Gold: {evt.Gold}";
        }

        private void OnInsufficientResources(InsufficientResourcesEvent evt)
        {
            ShowMessage($"Insufficient gold! Need {evt.Required.Gold}, have {evt.Available}");
        }

        private void OnGameLoaded()
        {
            foreach (var view in _buildingViews.Values)
            {
                Destroy(view.gameObject);
            }
            _buildingViews.Clear();

            foreach (var building in _grid.GetAllBuildings())
            {
                var view = _buildingFactory.CreateBuilding(building, transform);
                _buildingViews[building.Id] = view;
            }

            ShowMessage("Game Loaded!");
        }

        private void ShowMessage(string message)
        {
            _messageLabel.text = message;
            _messageLabel.style.display = DisplayStyle.Flex;
            Observable.Timer(TimeSpan.FromSeconds(3))
                .Subscribe(_ =>
                {
                    _messageLabel.text = "";
                    _messageLabel.style.display = DisplayStyle.None;
                })
                .AddTo(this);
        }

        private void OnDestroy()
        {
            _inputActions?.Disable();
            _subscriptions?.Dispose();
        }
    }
}