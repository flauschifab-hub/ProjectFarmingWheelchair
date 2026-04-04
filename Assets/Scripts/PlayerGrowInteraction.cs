using System.Collections;
using UnityEngine;

public class PlayerGrowInteraction : MonoBehaviour
{
    public Animator animator;
    public float animationTime = 1.5f;

    public Collecter collecter;

    private bool isBusy = false;
    private bool playerInside = false;
    private Collider currentWeed;

    private void OnTriggerStay(Collider other)
    {
        // Auf BEIDE Tags reagieren
        if (other.CompareTag("Grow") || other.CompareTag("Weed"))
        {
            playerInside = true;
            currentWeed = other;

            if (!isBusy)
            {
                StartCoroutine(HandleWeedGrow(other));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Grow") || other.CompareTag("Weed"))
        {
            playerInside = false;
            currentWeed = null;
        }
    }

    IEnumerator HandleWeedGrow(Collider weed)
    {
        isBusy = true;

        WeedGrow grow = weed.GetComponent<WeedGrow>();

        if (grow == null)
        {
            isBusy = false;
            yield break;
        }

        Debug.Log("Animation gestartet");

        animator.SetTrigger("collect");

        yield return new WaitForSeconds(animationTime);

        if (!grow.isPlanted)
        {
            // 🌱 Pflanzen
            Debug.Log("Pflanzen...");
            grow.Plant();
        }
        else
        {
            // 🌿 Einsammeln
            Debug.Log("Einsammeln...");
            collecter.AddWeed();
            Destroy(weed.gameObject);
        }

        yield return new WaitForSeconds(0.2f);
        isBusy = false;
    }
}