using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    [Header("Particle Systems")]
    public ParticleSystem jumpParticles;
    public ParticleSystem landParticles;
    public ParticleSystem wallParticles;
    public ParticleSystem dashParticles;
    public ParticleSystem runParticles;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayJump(Vector2 pos)
    {
        jumpParticles.transform.position = pos;
        jumpParticles.Play();
    }

    public void PlayLand(Vector2 pos)
    {
        landParticles.transform.position = pos;
        landParticles.Play();
    }

    public void PlayWall(Vector2 pos, bool right)
    {
        wallParticles.transform.position = pos;
        var main = wallParticles.main;
        // Adjust direction if needed
        wallParticles.Play();
    }

    public void PlayDash(Vector2 pos)
    {
        dashParticles.transform.position = pos;
        dashParticles.Play();
    }

    public void SetRunning(bool isRunning, Vector2 pos)
    {
        if (isRunning)
        {
            if (!runParticles.isPlaying) runParticles.Play();
            runParticles.transform.position = pos;
        }
        else
        {
            runParticles.Stop();
        }
    }
}
