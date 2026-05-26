using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Pemain")]
    public Transform target; // Tarik objek Pl_0 ke sini

    [Header("Pengaturan Kamera")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Collider Pembatas Map")]
    public BoxCollider2D mapCollider; // Ini sudah diperbaiki, tidak akan error lagi

    private float camHalfHeight;
    private float camHalfWidth;
    private float minX, maxX, minY, maxY;

    void Start()
    {
        // Hitung ukuran setengah dari lensa kamera
        Camera cam = GetComponent<Camera>();
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        // Hitung batas mati secara otomatis berdasarkan Box Collider 2D map kamu
        if (mapCollider != null)
        {
            Bounds bounds = mapCollider.bounds;
            minX = bounds.min.x + camHalfWidth;
            maxX = bounds.max.x - camHalfWidth;
            minY = bounds.min.y + camHalfHeight;
            maxY = bounds.max.y - camHalfHeight;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Cari posisi ideal kamera mengikuti target
        Vector3 desiredPosition = target.position + offset;

        // Kunci otomatis posisi kamera agar selalu pas di dalam batas collider map
        if (mapCollider != null)
        {
            float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);
            desiredPosition = new Vector3(clampedX, clampedY, desiredPosition.z);
        }

        // Gerakkan kamera secara smooth
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}