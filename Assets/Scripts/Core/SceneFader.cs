using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class SceneFader : MonoBehaviour
{
    [SerializeField] private AnimationCurve ease = null;
    private CanvasGroup canvasGroup;
    private Coroutine running;

    public bool IsFading => running != null;
    public float Alpha => canvasGroup.alpha;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (ease == null || ease.length == 0)
        {
            ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.99f;
        canvasGroup.interactable = false;
    }

    public void SetInstantVisible()
    {
        StopCurrent();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;
    }

    public void SetInstantHidden()
    {
        StopCurrent();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(1f, duration);
    }

    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(0f, duration);
    }

    private IEnumerator Fade(float target, float duration)
    {
        StopCurrent();
        running = StartCoroutine(FadeRoutine(target, duration));
        yield return running;
        running = null;
    }

    private IEnumerator FadeRoutine(float target, float duration)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        if (target > start)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = false;
        }

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float eval = ease.Evaluate(k);
            canvasGroup.alpha = Mathf.Lerp(start, target, eval);
            yield return null;
        }

        canvasGroup.alpha = target;

        if (Mathf.Approximately(target, 0f))
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        else
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = false;
        }
    }

    private void StopCurrent()
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
    }
}
