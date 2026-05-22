using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager Instance { get; private set; }

    [Header("Wind Settings")]
    public float windStrength = 5f;
    public bool isWindActive = false;
    public Vector2 windDirection = Vector2.left;

    [Header("Visuals")]
    public ParticleSystem snowParticles;
    public AudioSource windAudio;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isWindActive)
        {
            if (snowParticles != null && !snowParticles.isPlaying) snowParticles.Play();
            if (windAudio != null && !windAudio.isPlaying) windAudio.Play();
        }
        else
        {
            if (snowParticles != null && snowParticles.isPlaying) snowParticles.Stop();
            if (windAudio != null && windAudio.isPlaying) windAudio.Stop();
        }
    }

    public void ApplyWind(Rigidbody2D rb)
    {
        if (isWindActive)
        {
            rb.AddForce(windDirection * windStrength, ForceMode2D.Force);
        }
    }
}
