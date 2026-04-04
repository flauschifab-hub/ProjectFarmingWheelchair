using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Game sollte schließen ka hab keine zeit mehr für nen build");
        Application.Quit();
    }
}