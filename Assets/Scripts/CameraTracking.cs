using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    public float TimeScale = 1f;
    public float TotalLength;
    
    [SerializeField] private float boundsSafeArea = 0.1f;
    [SerializeField] private float boundsScaleTime = 1f;

    private float boundsVelocity;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        var cameraBounds = OrthographicBounds(cam);
        float l = TotalLength + boundsSafeArea;
        float boundsRatio = Mathf.Max(l / cameraBounds.extents.x, l / cameraBounds.extents.y);

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, cam.orthographicSize * boundsRatio,
            ref boundsVelocity, boundsScaleTime, Mathf.Infinity, TimeScale * Time.deltaTime);
    }

    public void ForceSize()
    {
        var cameraBounds = OrthographicBounds(cam);
        float l = TotalLength + boundsSafeArea;
        float boundsRatio = Mathf.Max(l / cameraBounds.extents.x, l / cameraBounds.extents.y);

        cam.orthographicSize *= boundsRatio;
    }
    
    private static Bounds OrthographicBounds(Camera camera)
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float cameraHeight = camera.orthographicSize * 2;
        var bounds = new Bounds(
            camera.transform.position,
            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
        return bounds;
    }
}
