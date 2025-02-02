using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RetryMenu : MonoBehaviour
{

    public Button buttonRetry;
    public Button buttonQuit;

    public void FinishParty() {
        buttonRetry.gameObject.SetActive(true);
        buttonQuit.gameObject.SetActive(true);
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
