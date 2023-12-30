using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenueMusicPlayer : MonoBehaviour
{
    public static MainMenueMusicPlayer Instance { get; private set; }

    private AudioSource m_audioSource;
    void Awake()
    {
        if (Instance == null)
        {
            m_audioSource = GetComponent<AudioSource>();
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        m_audioSource.Play();
    }
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            m_audioSource.Stop();
            Destroy(gameObject);
        }
    }
}
