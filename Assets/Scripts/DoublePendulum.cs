using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DoublePendulum : MonoBehaviour
{
    #region Inspector Properties

    [Header("Pendulum Points")] 
    [SerializeField] private Transform anchor;
    [SerializeField] private PendulumPoint point1;
    [SerializeField] private float point1Mass = 10;
    [SerializeField] private PendulumPoint point2;
    [SerializeField] private float point2Mass = 10;
    [SerializeField] private Vector2 minMaxLengths = new Vector2(0.2f, 2f);

    [Header("Physics")] 
    [SerializeField] public float VelocityDamping = 0.03f;
    [SerializeField] public float Gravity = 9.8f;
    [SerializeField] public Vector2 GravityDirection = Vector2.down;
    [SerializeField] private float gravitySmoothing = 1f;
    [SerializeField] public bool UseGyroscope = true;

    [Header("Scene References")]
    [SerializeField] private LineRenderer line1;
    [SerializeField] private LineRenderer line2;
    [SerializeField] private ParticleSystem pointParticles;
    [SerializeField] private ParticleSystem backgroundParticles;
    [SerializeField] private CameraTracking cameraTracking;
    [SerializeField] private MusicManager musicManager;

    [Header("UI Elements")] 
    [SerializeField] private Slider speedSlider;
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private RectTransform tutorialScreen1;
    [SerializeField] private RectTransform tutorialScreen2;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled;
    
    #endregion

    #region Private vars

    private float p1angle;
    private float p2angle;
    
    private float p1v;
    private float p2v;
    
    private float p1a;
    private float p2a;

    private float p1Length;
    private float p2Length;

    private Vector2 gravDir;
    private Vector2 gravDirVel;

    private bool simRunning;
    private float simSpeed = 1f;

    private const string TutorialPref = "DidPendulumTutorial";

    #endregion

    #region Public Properties

    public bool IsSimRunning => simRunning;
    public float MinSegmentLength => minMaxLengths.x;
    public float MaxSegmentLength => minMaxLengths.y;

    #endregion
    

    private void Start()
    {
        point1.Pendulum = this;
        point2.Pendulum = this;

        if (SystemInfo.supportsGyroscope)
            Input.gyro.enabled = true;

        RandomizePoints();
        
        InitSim(true);
        gameObject.BroadcastMessage("ResetSimMessage");

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (PlayerPrefs.GetInt(TutorialPref, 0) != 1)
            StartCoroutine(ShowTutorial());
    }

    public void InitSim(bool fade)
    {
        Vector3 p1 = (point1.transform.localPosition).normalized;
        Vector3 p2 = (point2.transform.localPosition - point1.transform.localPosition).normalized;

        p1angle = Mathf.Atan2(Vector3.Dot(Vector3.Cross(Vector3.up, p1), Vector3.back),
            Vector3.Dot(Vector3.up, p1));
        p2angle = Mathf.Atan2(Vector3.Dot(Vector3.Cross(Vector3.up, p2), Vector3.back),
            Vector3.Dot(Vector3.up, p2));
        
        p1v = 0;
        p2v = 0;
        p1a = 0;
        p2a = 0;
        
        CalculateDeltas();

        pointParticles.Play();
        cameraTracking.enabled = true;
        simRunning = true;
        
        if (fade)
            screenFader.FadeIn();
    }

    public void StopSim()
    {
        speedSlider.value = 0f;
        pointParticles.Stop();
        cameraTracking.enabled = false;
        simRunning = false;
    }

    public void SetSpeed(float speed)
    {
        simSpeed = speed;

        if (speed >= 0.05f && !IsSimRunning)
            InitSim(false);

        var pps = pointParticles.main;
        pps.simulationSpeed = speed;
        var bps = backgroundParticles.main;
        bps.simulationSpeed = speed;

        cameraTracking.TimeScale = speed;
    }

    public void ResetSim()
    {
        screenFader.FadeOut(.5f, () =>
        {
            RandomizePoints();
            InitSim(true);
            gameObject.BroadcastMessage("ResetSimMessage");
        });
    }

    private void FixedUpdate()
    {
        if (UseGyroscope && SystemInfo.supportsGyroscope)
            GravityDirection = Input.gyro.gravity;

        Physics.gravity = GravityDirection * Gravity;
        gravDir = Vector2.SmoothDamp(gravDir, GravityDirection, ref gravDirVel, gravitySmoothing);

        if (simRunning)
            CalculateDeltas();
        
        if (debugEnabled)
            DrawDebugInfo();
    }

    private void Update()
    {
        if (simRunning)
        {
            point1.transform.localPosition = new Vector3((float)(p1Length * Math.Sin(p1angle)), (float)(p1Length * Math.Cos(p1angle)));
            point2.transform.localPosition = point1.transform.localPosition + new Vector3((float)(p2Length * Math.Sin(p2angle)), (float)(p2Length * Math.Cos(p2angle)));

            float colliderScale = ((p1Length + p2Length) - (MinSegmentLength * 2)) /
                                  ((MaxSegmentLength * 2) - (MinSegmentLength * 2));
            point1.SetColliderScalePercent(colliderScale);
            point2.SetColliderScalePercent(colliderScale);
        }

        line1.SetPositions(new [] {transform.position, point1.transform.position});
        line2.SetPositions(new [] {point1.transform.position, point2.transform.position});
        
        cameraTracking.TotalLength = p1Length + p2Length;
    }

    private IEnumerator ShowTutorial()
    {
        musicManager.enabled = false;
        SetSpeed(0f);
        tutorialScreen1.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(4f);
        
        screenFader.FadeOut(1f, () =>
        {
            tutorialScreen1.gameObject.SetActive(false);
            tutorialScreen2.gameObject.SetActive(true);
            screenFader.FadeIn();

            tutorialScreen2.Find("StartButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                tutorialScreen2.gameObject.SetActive(false);
                SetSpeed(1f);
                musicManager.enabled = true;
                PlayerPrefs.SetInt(TutorialPref, 1);
            });
        });
    }

    private void CalculateDeltas()
    {
        Vector2 dp1 = point1.transform.localPosition;
        Vector2 dp2 = point2.transform.localPosition - point1.transform.localPosition;

        p1Length = dp1.magnitude;
        p2Length = dp2.magnitude;

        CalculateAcceleration(out p1a, out p2a);
        p1v += p1a * Time.fixedDeltaTime * simSpeed * VelocityDamping;
        p2v += p2a * Time.fixedDeltaTime * simSpeed * VelocityDamping;
        p1angle  += p1v * simSpeed;
        p2angle  += p2v * simSpeed;
    }
    
    private void CalculateAcceleration(out float a1, out float a2)
    {
        float gravityAngle = GravityAngle(gravDir);
        float g = -Gravity * gravDir.magnitude;

        var m1 = point1Mass;
        var m2 = point2Mass;

        var p1angleR = p1angle - gravityAngle;
        var p1vR = p1v;
        var p2angleR = p2angle - gravityAngle;
        var p2vR = p2v;
        
        float num1 = -g * (2 * m1 + m2) * Mathf.Sin(p1angleR) - m2 * g * Mathf.Sin(p1angleR - 2 * p2angleR)
            - 2 * Mathf.Sin(p1angleR - p2angleR) * m2 * (p2vR * p2vR * p2Length + p1vR * p1vR * p1Length * Mathf.Cos(p1angleR - p2angleR));
        
        float den1 = p1Length * (2 * m1 + m2 - m2 * Mathf.Cos(2 * p1angleR - 2 * p2angleR));

        a1 = num1 / den1;
        
        
        float num2 = 2 * Mathf.Sin(p1angleR - p2angleR) * (p1vR * p1vR * p1Length *
            (m1 + m2) + g * (m1 + m2) * Mathf.Cos(p1angleR) + p2vR * p2vR * p2Length * m2 * Mathf.Cos(p1angleR - p2angleR));
        
        float den2 = p2Length * (2 * m1 + m2 - m2 * Mathf.Cos(2 * p1angleR - 2 * p2angleR));

        a2 = num2 / den2;
    }

    private void RandomizePoints()
    {
        point1.transform.position = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minMaxLengths.x, minMaxLengths.y);
        point2.transform.position = point1.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minMaxLengths.x, minMaxLengths.y));
    }
    
    private static float GravityAngle(Vector3 gravityDirection)
    {
        return Mathf.Atan2(Vector3.Dot(Vector3.Cross(Vector3.down, gravityDirection.normalized), Vector3.back),
            Vector3.Dot(Vector3.down, gravityDirection.normalized));
    }

    private void DrawDebugInfo()
    {
        Debug.DrawRay(transform.position, -anchor.transform.up);
    }

    private void OnValidate()
    {
        line1.SetPositions(new [] {transform.position, point1.transform.position});
        line2.SetPositions(new [] {point1.transform.position, point2.transform.position});
    }
}
