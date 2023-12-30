using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenueHandler : MonoBehaviour
{
    public static PauseMenueHandler Instance { get; private set; }
    public GameObject m_pauseMenue;
    public bool isPaused = false;
    private CursorLockMode m_cursorLockStateBeforePause;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            m_pauseMenue.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {

        Cursor.lockState = m_cursorLockStateBeforePause;
        m_pauseMenue.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

    }

    private void Pause()
    {
        m_cursorLockStateBeforePause = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        m_pauseMenue.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Exit()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
