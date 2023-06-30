using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public UnityEvent OnFadeIn;
    public UnityEvent OnFadeOut;
    
    private Coroutine _runningCoroutine;
    private CanvasGroup _canvasGroup;


    public void FadeIn(float time = 1f, Action onComplete = null)
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
        if (_runningCoroutine != null)
            StopCoroutine(_runningCoroutine);
        
        _canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
        _runningCoroutine = StartCoroutine(Fade(1f, 0f, time, () =>
        {
            gameObject.SetActive(false);
            OnFadeIn.Invoke();
            onComplete?.Invoke();
        }));
    }

    public void FadeOut(float time = 1f, Action onComplete = null)
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
        if (_runningCoroutine != null)
            StopCoroutine(_runningCoroutine);

        _canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
        _runningCoroutine = StartCoroutine(Fade(0f, 1f, time, () =>
        {
            OnFadeOut.Invoke();
            onComplete?.Invoke();
        }));
    }

    private IEnumerator Fade(float start, float target, float duration, Action onComplete = null)
    {
        var wait = new WaitForEndOfFrame();
        float time = 0f;
        while (time <= duration)
        {
            time += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(start, target, time / duration);
            yield return wait;
        }

        _canvasGroup.alpha = target;
        onComplete?.Invoke();
    }
}
