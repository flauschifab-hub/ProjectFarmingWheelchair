using System.Collections;
using UnityEngine;

public class WeedGrow : MonoBehaviour
{
   public bool isPlanted = false;

    public void Plant()
    {
        isPlanted = true;
        gameObject.tag = "Weed";
        Debug.Log("Weed wurde gepflanzt!");

    }
}