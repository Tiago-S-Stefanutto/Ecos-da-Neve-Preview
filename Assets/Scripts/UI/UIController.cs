using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [Header("Fades")]
    public Image fadeOverlay;
    public float defaultFadeDuration = 1f;

    [Header("Messages")]
    public TextMeshProUGUI messageText;
    public CanvasGroup messageGroup;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        float d = duration < 0 ? defaultFadeDuration : duration;
        yield return StartCoroutine(FadeRoutine(1f, 0f, d));
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        float d = duration < 0 ? defaultFadeDuration : duration;
        yield return StartCoroutine(FadeRoutine(0f, 1f, d));
    }

    private IEnumerator FadeRoutine(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(start, end, elapsed / duration));
            yield return null;
        }
        fadeOverlay.color = new Color(0, 0, 0, end);
    }

    public void ShowMessage(string text, float duration = 3f)
    {
        StartCoroutine(MessageRoutine(text, duration));
    }

    private IEnumerator MessageRoutine(string text, float duration)
    {
        messageText.text = text;
        // Fade in group
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            messageGroup.alpha = elapsed / 0.5f;
            yield return null;
        }
        yield return new WaitForSeconds(duration);
        // Fade out group
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            messageGroup.alpha = 1f - (elapsed / 0.5f);
            yield return null;
        }
    }
}
