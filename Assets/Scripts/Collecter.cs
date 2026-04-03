using UnityEngine;
using TMPro;

public class Collecter : MonoBehaviour
{
    //[Header("Audio")]
    //public AudioSource landSource;     // Landing Sound
   // public AudioSource collectSource;  // Collect Sound
   [SerializeField] Timer timer;

    private int package = 0;
    public TextMeshProUGUI WeedText;

    
    void OnCollisionEnter(Collision other)
    {
    Debug.Log("Collision erkannt: " + other.gameObject.name);
    }
    
    private void OnTriggerEnter(Collider other)
    {
    if (other.CompareTag("Weed"))
    {
        Debug.Log("Weed eingesammelt");

        package++;
        WeedText.text = "Weed: " + package;

        timer.AddTime(30f);

        Destroy(other.gameObject);
    }
}
}
