using System.Collections;
using UnityEngine;

public class WeedGrow : MonoBehaviour
{
    public float growTime = 5f;
    public Vector3 grownScale = new Vector3(2f, 2f, 2f);

    private bool isGrowing = false;
    private bool isReady = false;



    public void StartGrow()
    {
        if (!isGrowing && !isReady)
        {
            StartCoroutine(Grow());
        }
    }

    IEnumerator Grow()
    {
        isGrowing = true;

        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < growTime)
        {
            t += Time.deltaTime;
            float progress = t / growTime;

            transform.localScale = Vector3.Lerp(startScale, grownScale, progress);

            yield return null;
        }

        transform.localScale = grownScale;

        isReady = true;
        isGrowing = false;
    }

    public bool IsReady()
    {
        return isReady;
    }

    public bool IsGrowing()
    {
        return isGrowing;
    }

    public void Harvest()
    {
        Destroy(gameObject);
    }
}