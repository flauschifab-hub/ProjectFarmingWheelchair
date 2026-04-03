using UnityEngine;
using TMPro;

public class Collecter : MonoBehaviour
{
    public TextMeshProUGUI weedText;

    private int package = 0;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Grow")) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            WeedGrow grow = other.GetComponent<WeedGrow>();

            if (grow == null) return;

        
            if (!grow.IsReady() && !grow.IsGrowing())
            {
                grow.StartGrow();
                return;
            }

          
            if (grow.IsReady())
            {
                package++;
                weedText.text = "Weed: " + package;

                grow.Harvest();
            }
        }
    }
}