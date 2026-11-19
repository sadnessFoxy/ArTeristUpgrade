using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Minimal replacement for the two DOTween calls this project used.
// A new tween on the same target cancels the previous one.
public static class SimpleTween
{
    private static TweenRunner runner;
    private static readonly Dictionary<UnityEngine.Object, Coroutine> active = new Dictionary<UnityEngine.Object, Coroutine>();

    public static void OrthoSize(Camera camera, float target, float duration, Action onComplete = null)
    {
        float start = camera.orthographicSize;
        Run(camera, duration, t => camera.orthographicSize = Mathf.LerpUnclamped(start, target, EaseOutCubic(t)), onComplete);
    }

    public static void AnchorPosY(RectTransform rect, float targetY, float duration, bool snapping = false)
    {
        float startY = rect.anchoredPosition.y;
        Run(rect, duration, t =>
        {
            float y = Mathf.LerpUnclamped(startY, targetY, t);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, snapping ? Mathf.Round(y) : y);
        }, null);
    }

    public static float EaseOutCubic(float t)
    {
        float inv = 1f - t;
        return 1f - inv * inv * inv;
    }

    private static void Run(UnityEngine.Object target, float duration, Action<float> step, Action onComplete)
    {
        TweenRunner host = GetRunner();
        if (active.TryGetValue(target, out Coroutine running) && running != null)
            host.StopCoroutine(running);

        active[target] = host.StartCoroutine(Animate(target, duration, step, onComplete));
    }

    private static IEnumerator Animate(UnityEngine.Object target, float duration, Action<float> step, Action onComplete)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            if (target == null)
                yield break;
            step(elapsed / duration);
            yield return null;
        }

        if (target == null)
            yield break;

        step(1f);
        active.Remove(target);
        onComplete?.Invoke();
    }

    private static TweenRunner GetRunner()
    {
        if (runner == null)
        {
            var go = new GameObject("SimpleTweenRunner") { hideFlags = HideFlags.HideAndDontSave };
            UnityEngine.Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<TweenRunner>();
        }
        return runner;
    }

    private class TweenRunner : MonoBehaviour { }
}
