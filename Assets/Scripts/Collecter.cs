using UnityEngine;
using TMPro;

public class Collecter : MonoBehaviour
{
    public TextMeshProUGUI WeedText;
    [SerializeField] Timer timer;

    private int package = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weed"));
        {
            Debug.Log("Collision with the Player");
            package++;
            WeedText.text = "Weed: " + package;
            timer.AddTime(30f);


            Destroy(other.gameObject);
        }

         
    }
}
