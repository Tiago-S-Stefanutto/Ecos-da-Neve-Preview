using UnityEngine;
using System.Collections.Generic;

public class GhostEffect : MonoBehaviour
{
    public float ghostDelay = 0.05f;
    private float ghostDelayTimer;
    public GameObject ghostPrefab;
    public bool makeGhost = false;

    private void Update()
    {
        if (makeGhost)
        {
            if (ghostDelayTimer > 0)
            {
                ghostDelayTimer -= Time.deltaTime;
            }
            else
            {
                GameObject currentGhost = Instantiate(ghostPrefab, transform.position, transform.rotation);
                Sprite currentSprite = GetComponentInChildren<SpriteRenderer>().sprite;
                currentGhost.GetComponent<SpriteRenderer>().sprite = currentSprite;
                currentGhost.transform.localScale = GetComponentInChildren<SpriteRenderer>().transform.lossyScale;
                ghostDelayTimer = ghostDelay;
                Destroy(currentGhost, 0.5f);
            }
        }
    }
}
