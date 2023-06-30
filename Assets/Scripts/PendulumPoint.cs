using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CircleCollider2D))]
public class PendulumPoint : MonoBehaviour, IDragHandler
{
    [HideInInspector] public DoublePendulum Pendulum;

    [SerializeField] private Transform parent;
    [SerializeField] private Transform child;

    [SerializeField] private Vector2 minMaxColliderScale = new Vector2(1f, 3f);

    private Camera _mainCamera;
    private CircleCollider2D _collider2D;

    private void Start()
    {
        _mainCamera = Camera.main;
        _collider2D = GetComponent<CircleCollider2D>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Pendulum) return;
        
        if (Pendulum.IsSimRunning)
            Pendulum.StopSim();
        
        var pos = _mainCamera.ScreenToWorldPoint(eventData.position);
        pos.z = 0;

        pos = ((pos - parent.position).normalized * Mathf.Clamp((pos - parent.position).magnitude, Pendulum.MinSegmentLength, Pendulum.MaxSegmentLength)) + parent.position;
        transform.position = new Vector3(pos.x, pos.y);

        if (child)
        {
            var childPos = child.transform.position;
            childPos = ((childPos - transform.position).normalized * Mathf.Clamp((childPos - transform.position).magnitude, Pendulum.MinSegmentLength, Pendulum.MaxSegmentLength)) + transform.position;
            child.position = new Vector3(childPos.x, childPos.y);
        }
    }

    public void SetColliderScalePercent(float percent)
    {
        _collider2D.radius = Mathf.Lerp(minMaxColliderScale.x, minMaxColliderScale.y, percent);
    }
}
