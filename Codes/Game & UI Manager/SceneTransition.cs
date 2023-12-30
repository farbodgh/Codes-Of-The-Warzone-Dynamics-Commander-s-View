using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");

    }


    public void ShowStartMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void ShowHelpMenu()
    {
        SceneManager.LoadScene("HelpScene");
    }

    public void ExitTheGame()
    {
        Application.Quit();
    }
}
