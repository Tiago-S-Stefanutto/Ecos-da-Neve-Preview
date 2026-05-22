using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.deathSound);
            GameManager.Instance.PlayerDied();
        }
    }

    private void OnCollisionEnter2D(OnCollisionEnter2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.deathSound);
            GameManager.Instance.PlayerDied();
        }
    }
}
