using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MoveableCamera : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float rotateSpeed = 100f;
    public float zoomSpeed = 20f;

    Camera camera;
    private void Start()
    {
        if (camera is null)
        {
            camera = GetComponent<Camera>();
        }
    }
    public void Update()
    {
        MoveCamera();
        RotateCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        Vector3 move = Vector3.zero;
        var mvSpeed = moveSpeed;
        // If shfit key is pressed, increase move speed.
        // Otherwise, normal speed.
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            mvSpeed *= 2f;
        }

        if (Input.GetKey(KeyCode.W)) move += camera.transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= camera.transform.forward;
        if (Input.GetKey(KeyCode.A)) move -= camera.transform.right;
        if (Input.GetKey(KeyCode.D)) move += camera.transform.right;

        // Restrict movement to XZ plane
        move.y = 0;
        camera.transform.position += move.normalized * mvSpeed * Time.deltaTime;
    }
    void RotateCamera()
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Y axis rotation (orbit around board)
            camera.transform.RotateAround(Vector3.zero, Vector3.up, mouseX * rotateSpeed * Time.deltaTime);

            // X axis rotation (tilt up/down)
            Vector3 rightAxis = camera.transform.right;
            camera.transform.RotateAround(Vector3.zero, rightAxis, -mouseY * rotateSpeed * Time.deltaTime);
        }

        // Q/E keys for Y axis rotation
        if (Input.GetKey(KeyCode.Q))
        {
            camera.transform.RotateAround(Vector3.zero, Vector3.up, -rotateSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {
            camera.transform.RotateAround(Vector3.zero, Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
    void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        camera.transform.position += camera.transform.forward * scroll * zoomSpeed;
    }
}