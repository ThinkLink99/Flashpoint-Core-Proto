using UnityEngine;

public class MoveableCamera : MonoBehaviour
{
    public float movementTime = 1f;

    public float moveSpeed = 20f;
    public float shiftMultiplier = 5f;

    public float rotateSpeed = 100f;
    public float zoomSpeed = 20f;

    public Vector3 newPosition;
    public Quaternion newRotation;

    public Transform camera;

    private void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
    }
    public void Update()
    {
        MoveCamera();
        RotateCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        var mvSpeed = moveSpeed;
        // If shfit key is pressed, increase move speed.
        // Otherwise, normal speed.
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            mvSpeed *= shiftMultiplier;
        }


        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
        }
        else
        {

        }

        if (Input.GetKey(KeyCode.W)) newPosition += (transform.forward * mvSpeed);
        if (Input.GetKey(KeyCode.S)) newPosition += (transform.forward * -mvSpeed);
        if (Input.GetKey(KeyCode.A)) newPosition += (transform.right * -mvSpeed);
        if (Input.GetKey(KeyCode.D)) newPosition += (transform.right * mvSpeed);

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
    }
    void RotateCamera()
    {
        if (Input.GetMouseButton(2)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Y axis rotation (orbit around board)
            newRotation *= Quaternion.Euler(Vector3.up * (mouseY * rotateSpeed));

            // X axis rotation (tilt up/down)
        }

        // Q/E keys for Y axis rotation
        if (Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotateSpeed);
        }
        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotateSpeed);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
    }
    void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        camera.position += camera.forward * scroll * zoomSpeed;
    }
}