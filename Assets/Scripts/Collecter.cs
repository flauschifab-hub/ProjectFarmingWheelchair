using UnityEngine;
using TMPro;

public class Collecter : MonoBehaviour
{
    public TextMeshProUGUI WeedText;
    [SerializeField] Timer timer;

    private int package = 0;

    public void AddWeed()
    {
        package++;
        WeedText.text = "Weed: " + package;
        timer.AddTime(30f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weed"))
        {
            Debug.Log("Normales Weed eingesammelt");

            AddWeed();
            Destroy(other.gameObject);
        }
    }
}