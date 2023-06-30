using System.Collections.Generic;
using UnityEngine;
 
[RequireComponent(typeof(Canvas))]
public class CanvasHelper : MonoBehaviour
{
    private static readonly List<CanvasHelper> helpers = new List<CanvasHelper>();
    private static bool screenChangeVarsInitialized;
    private static ScreenOrientation lastOrientation = ScreenOrientation.Landscape;
    private static Vector2Int lastResolution = Vector2Int.zero;
    private static Rect lastSafeArea = Rect.zero;
 
    private Canvas canvas;
    private RectTransform safeAreaTransform;
 
    void Awake()
    {
        if (!helpers.Contains(this))
            helpers.Add(this);
   
        canvas = GetComponent<Canvas>();
   
        safeAreaTransform = transform.Find("SafeArea") as RectTransform;
   
        if (!screenChangeVarsInitialized)
        {
            lastOrientation = Screen.orientation;
            lastResolution.x = Screen.width;
            lastResolution.y = Screen.height;
            lastSafeArea = Screen.safeArea;
   
            screenChangeVarsInitialized = true;
        }
       
        ApplySafeArea();
    }
 
    void Update()
    {
        if (helpers[0] != this)
            return;
   
        if (Application.isMobilePlatform && Screen.orientation != lastOrientation)
            OrientationChanged();
   
        if (Screen.safeArea != lastSafeArea)
            SafeAreaChanged();
   
        if (Screen.width != lastResolution.x || Screen.height != lastResolution.y)
            ResolutionChanged();
    }
 
    void ApplySafeArea()
    {
        if (safeAreaTransform == null)
            return;
   
        var safeArea = Screen.safeArea;
   
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;
        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;
   
        safeAreaTransform.anchorMin = anchorMin;
        safeAreaTransform.anchorMax = anchorMax;
    }
 
    void OnDestroy()
    {
        if (helpers != null && helpers.Contains(this))
            helpers.Remove(this);
    }
 
    private static void OrientationChanged()
    {
        lastOrientation = Screen.orientation;
        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;
    }
 
    private static void ResolutionChanged()
    {
        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;
    }
 
    private static void SafeAreaChanged()
    {
        lastSafeArea = Screen.safeArea;

        foreach (var t in helpers)
            t.ApplySafeArea();
    }
}