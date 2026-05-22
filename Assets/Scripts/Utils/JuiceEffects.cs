using UnityEngine;
using System.Collections;

public class JuiceEffects : MonoBehaviour
{
    [Header("Squash & Stretch")]
    public Transform spriteTransform;
    public float squashAmount = 0.2f;
    public float stretchAmount = 0.2f;
    public float recoverySpeed = 10f;

    [Header("Dash Effects")]
    public float dashFreezeDuration = 0.05f;
    public Color dashColor = new Color(0.5f, 0.8f, 1f, 1f);

    private Vector3 originalScale;
    private SpriteRenderer sr;

    private void Awake()
    {
        originalScale = spriteTransform.localScale;
        sr = spriteTransform.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Smoothly return to original scale
        spriteTransform.localScale = Vector3.Lerp(spriteTransform.localScale, originalScale, Time.deltaTime * recoverySpeed);
    }

    public void ApplySquash()
    {
        spriteTransform.localScale = new Vector3(originalScale.x + squashAmount, originalScale.y - squashAmount, originalScale.z);
    }

    public void ApplyStretch()
    {
        spriteTransform.localScale = new Vector3(originalScale.x - stretchAmount, originalScale.y + stretchAmount, originalScale.z);
    }

    public void TriggerDashFeedback()
    {
        StartCoroutine(DashFeedbackRoutine());
    }

    private IEnumerator DashFeedbackRoutine()
    {
        // Freeze frame
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(dashFreezeDuration);
        Time.timeScale = 1f;

        // Visual flash or color change can be added here
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = dashColor;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
        }
    }
}
