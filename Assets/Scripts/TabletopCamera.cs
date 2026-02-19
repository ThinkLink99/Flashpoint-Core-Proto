using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletopCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Optional. If left empty the script will look for a Tabletop object or use world origin.")]
    public Transform target;
    [Tooltip("Offset from the target center the camera will look at.")]
    public Vector3 targetOffset = Vector3.up * 1.0f;

    [Header("Distance / Zoom")]
    public float distance = 12f;
    public float minDistance = 3f;
    public float maxDistance = 50f;
    public float zoomSpeed = 10f;

    [Header("Rotation")]
    public bool allowRotation = true;
    public float rotationSpeed = 5f;
    public float minPitch = 5f;
    public float maxPitch = 80f;
    // Optional small rotation damping (set to 0 for immediate)
    public float rotationDamping = 0f;

    [Header("Panning")]
    public bool allowPan = true;
    public float panSpeed = 0.5f;
    public float keyboardPanSpeed = 5f;

    [Header("Smoothing")]
    public float moveDamping = 0;

    // internal state
    private float _yaw;
    private float _pitch;

    // The focal point the camera orbits around. When a target exists this is computed as target.position + _panOffset.
    private Vector3 _targetPosition;

    // Manual pan offset relative to the target (preserved across updates so pans are not lost).
    private Vector3 _panOffset = Vector3.zero;

    // input state (mutually exclusive)
    private bool _isRotating;
    private bool _isPanning;

    void Start()
    {
        // initialize target
        if (target == null)
        {
            var tt = FindObjectOfType<Tabletop>();
            if (tt != null) target = tt.transform;
        }

        // initialize pan offset and target position
        _panOffset = Vector3.zero;
        _targetPosition = (target != null) ? target.position + _panOffset : Vector3.zero;

        // initialize angles from current orientation so camera doesn't jump
        Vector3 e = transform.eulerAngles;
        _yaw = e.y;
        _pitch = e.x;
    }

    void LateUpdate()
    {
        HandleInput();
        ApplyTransform();
    }

    private void HandleInput()
    {
        // Zoom with wheel (always allowed)
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // Determine raw input intent
        bool rawRotate = allowRotation && Input.GetMouseButton(1);
        bool rawPan = allowPan && (Input.GetMouseButton(2) || (Input.GetMouseButton(0) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))));

        // Enforce exclusivity: rotation has priority if both buttons pressed; otherwise pan if pan pressed.
        if (rawRotate)
        {
            _isRotating = true;
            _isPanning = false;
        }
        else if (rawPan)
        {
            _isPanning = true;
            _isRotating = false;
        }
        else
        {
            _isRotating = false;
            _isPanning = false;
        }

        // Rotation input (only when rotating)
        if (_isRotating)
        {
            float mx = Input.GetAxisRaw("Mouse X");
            float my = Input.GetAxisRaw("Mouse Y");

            _yaw += mx * rotationSpeed;
            _pitch -= my * rotationSpeed;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        // Mouse panning (only when panning). Constrained to XZ plane.
        if (_isPanning)
        {
            float mx = Input.GetAxisRaw("Mouse X");
            float my = Input.GetAxisRaw("Mouse Y");

            Vector3 right = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
            Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

            // invert X so natural drag moves the world under the cursor; use forward for vertical mouse delta
            _panOffset += (-right * mx + -forward * my) * panSpeed;

            // ensure no vertical pan
            _panOffset.y = 0f;
        }

        // Keyboard pan (arrow keys / WASD) — only when not rotating (so it doesn't combine with orbit)
        float hor = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float ver = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
        if (!_isRotating && (Mathf.Abs(hor) > Mathf.Epsilon || Mathf.Abs(ver) > Mathf.Epsilon))
        {
            Vector3 camForward = transform.forward;
            camForward.y = 0;
            camForward.Normalize();
            Vector3 camRight = transform.right;
            camRight.y = 0;
            camRight.Normalize();

            _panOffset += (camRight * hor + camForward * ver) * keyboardPanSpeed * Time.deltaTime;
            _panOffset.y = 0f;
        }

        // Compute the effective target position.
        if (target != null)
        {
            // Follow the target but keep manual pan offset applied so panning isn't lost.
            _targetPosition = target.position + _panOffset;
        }
        else
        {
            // If no target, apply pan offset to the existing target position.
            _targetPosition += _panOffset;
            // reset pan offset because it's been applied directly to _targetPosition
            _panOffset = Vector3.zero;
        }
    }

    private void ApplyTransform()
    {
        // compute desired rotation & position from yaw/pitch (this rot matches the orbit used to compute desiredPosition)
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredTarget = _targetPosition + targetOffset;
        Vector3 desiredPosition = desiredTarget + rot * new Vector3(0f, 0f, -distance);

        // smooth move (position)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-moveDamping * Time.deltaTime));

        // Use the same orbital rotation (rot) to orient the camera. This keeps position and rotation consistent
        // and prevents the LookRotation mismatch jerk when position is being smoothed.
        if (_isPanning)
        {
            // keep current rotation unchanged while panning
        }
        else
        {
            if (rotationDamping <= 0f)
            {
                transform.rotation = rot;
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f - Mathf.Exp(-rotationDamping * Time.deltaTime));
            }
        }
    }
}
