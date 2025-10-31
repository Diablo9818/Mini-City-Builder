using UnityEngine;
using UnityEngine.InputSystem;

namespace CityBuilder.Infrastructure.Input
{
    public class CameraInputHandler : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 20f;

        private InputActions _inputActions;
        private Vector2 _moveInput;
        private bool _isDragging;
        private Vector3 _dragStartPosition;

        private void Awake()
        {
            _inputActions = new InputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
            _inputActions.Camera.Move.performed += OnMovePerformed;
            _inputActions.Camera.Move.canceled += OnMoveCanceled;
            _inputActions.Camera.Zoom.performed += OnZoom;
            _inputActions.Camera.Drag.started += OnDragStarted;
            _inputActions.Camera.Drag.canceled += OnDragCanceled;
        }

        private void OnDisable()
        {
            _inputActions.Camera.Move.performed -= OnMovePerformed;
            _inputActions.Camera.Move.canceled -= OnMoveCanceled;
            _inputActions.Camera.Zoom.performed -= OnZoom;
            _inputActions.Camera.Drag.started -= OnDragStarted;
            _inputActions.Camera.Drag.canceled -= OnDragCanceled;
            _inputActions.Disable();
        }

        private void Update()
        {
            HandleMovement();
            HandleDrag();
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _moveInput = Vector2.zero;
        }

        private void OnZoom(InputAction.CallbackContext context)
        {
            Debug.Log("Zoom input detected");
            var scrollValue = context.ReadValue<float>();
            Debug.Log($"Scroll Value: {scrollValue}"); // Добавьте это
            
            var zoomAmount = -scrollValue * _zoomSpeed * 10.1f;
            Debug.Log($"Zoom Speed: {_zoomSpeed}, Calculated Zoom Amount: {zoomAmount}"); // Добавьте это

            var currentOrthographicSize = _camera.orthographicSize;
            Debug.Log($"Current Orthographic Size: {currentOrthographicSize}"); // Добавьте это

            var newSize = currentOrthographicSize + zoomAmount;
            Debug.Log($"Calculated New Size (before clamp): {newSize}"); // Добавьте это

            _camera.orthographicSize = Mathf.Clamp(newSize, _minZoom, _maxZoom);
            Debug.Log($"Clamped Orthographic Size (Min: {_minZoom}, Max: {_maxZoom}): {_camera.orthographicSize}"); // Добавьте это
        }

        private void OnDragStarted(InputAction.CallbackContext context)
        {
            _isDragging = true;
            _dragStartPosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }

        private void OnDragCanceled(InputAction.CallbackContext context)
        {
            _isDragging = false;
        }

        private void HandleMovement()
        {
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                var movement = new Vector3(_moveInput.x, _moveInput.y, 0f) * (_moveSpeed * Time.deltaTime);
                _camera.transform.position += movement;
            }
        }

        private void HandleDrag()
        {
            if (_isDragging)
            {
                var currentPosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var delta = _dragStartPosition - currentPosition;
                _camera.transform.position += delta;
                _dragStartPosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }
        }
    }
}