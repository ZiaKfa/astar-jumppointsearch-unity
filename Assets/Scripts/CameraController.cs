using UnityEngine;

public class RuntimeCamera2D : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float panSpeed = 1f;
    public float zoomSpeed = 5f;
    public float minZoom = 1f;
    public float maxZoom = 50f;

    private Camera cam;
    private Vector3 lastMousePos;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void Update()
    {
        HandleMovement();
        HandlePan();
        HandleZoom();
    }

    void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += Vector3.up;
        if (Input.GetKey(KeyCode.S)) move += Vector3.down;
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;

        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(2))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            cam.transform.position -= delta * panSpeed * cam.orthographicSize * 0.01f;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.0001f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}
