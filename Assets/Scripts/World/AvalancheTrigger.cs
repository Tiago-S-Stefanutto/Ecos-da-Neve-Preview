using UnityEngine;
using System.Collections;

public class AvalancheTrigger : MonoBehaviour
{
    public GameObject avalancheVisual;
    public float avalancheSpeed = 8f;
    public Transform player;
    public bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(StartAvalanche());
        }
    }

    private IEnumerator StartAvalanche()
    {
        // Initial shake
        CameraShake.Instance.Shake(2f);
        // Play sound
        // AudioManager.Instance.PlaySFX(avalancheSound);
        
        yield return new WaitForSeconds(1f);

        while (isTriggered)
        {
            avalancheVisual.transform.Translate(Vector3.right * avalancheSpeed * Time.deltaTime);
            // Constant light shake
            CameraShake.Instance.Shake(0.2f);
            yield return null;
        }
    }
}
