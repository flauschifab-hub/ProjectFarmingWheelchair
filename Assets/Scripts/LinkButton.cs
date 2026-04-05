using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkButton : MonoBehaviour
{
    public string url = "";

    public void OpenLink()
    {
        Application.OpenURL(url);
    }
}
