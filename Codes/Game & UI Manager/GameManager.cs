using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    private bool m_RTSPhase = false;
    private bool m_thirdPersonPhase = true;
    private bool m_isPlayerAlive = true;
    //The following variables are used to swithc between the two phases 
    //when the phase changes the gameobjects are enabled and disabled accordingly 
    //All the settings and behaviours are attached to the gameobjects 


    //List of the gameobjects that should be disabled or enabled when switching to RTS phase
    [SerializeField]
    private GameObject[] m_RTSGameObjects;
    //List of the gameobjects that should be disabled or enabled when switching to third person phase
    [SerializeField]
    private GameObject[] m_thirdPersonGameObjects;
    [SerializeField]
    private GameObject m_player;
    private JUFootPlacement m_jUFootPlacement;
    private CharacterAimManager m_characterAimManager;
    private CharacterMovementManager m_characterMovement;
    private PlayerWeaponController m_playerWeaponController;
    private Animator m_playerAnimator;
    private GameObject m_playerModel;
    //List of Gameobjects that should be disabled or enabled when the player is entering the helo or exiting the helo
    [SerializeField]
    private GameObject m_playerHelo;
    private HeloWeaponSystem m_heloWeaponSystem;
    private Helicopter m_heloMovement;
    private AudioListener m_heloAudioListener; 
    private RotorRotation[] m_rotorRotations = new RotorRotation[2];
    [SerializeField]
    private Camera m_heloInteriorCamera;
    [SerializeField]
    private Camera m_heloExteriorCamera;
    private bool m_isPlayerInHelo = false;
    private Rigidbody m_heloRigidBody;

    public static GameManager Instance;

    public int m_numberOfEnemies { set; get; }
    public int m_numberOfFriendlyUnits { set; get; } 
    private void Awake()
    {
        //Deactivating all Debugs if the game is not running in the editor
        #if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
        #endif

        m_playerModel = m_player.transform.Find("PlayerModel").gameObject;
        m_heloWeaponSystem = m_playerHelo.GetComponent<HeloWeaponSystem>();
        m_heloMovement = m_playerHelo.GetComponent<Helicopter>();
        m_rotorRotations[0] = m_playerHelo.transform.Find("Regular/RotorParent").GetComponent<RotorRotation>();
        m_rotorRotations[1] = m_playerHelo.transform.Find("Regular/TailRotorParent").GetComponent<RotorRotation>();

        m_characterAimManager = m_player.GetComponent<CharacterAimManager>();
        m_characterMovement = m_player.GetComponent<CharacterMovementManager>();
        m_playerWeaponController = m_player.transform.Find("PlayerModel/Armature/root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/M4_Carbine").GetComponent<PlayerWeaponController>();
        m_jUFootPlacement = m_player.GetComponent<JUFootPlacement>();
        m_playerAnimator = m_player.GetComponent<Animator>();
        m_heloAudioListener = m_heloExteriorCamera.GetComponent<AudioListener>();
        m_heloRigidBody = m_playerHelo.GetComponent<Rigidbody>();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        for(int i = 0; i < m_RTSGameObjects.Length; i++)
        {
            m_RTSGameObjects[i].SetActive(false);
        }

        for (int i = 0; i < m_thirdPersonGameObjects.Length; i++)
        {
            m_thirdPersonGameObjects[i].SetActive(true);
        }

    }

    private void Start()
    {
        m_heloAudioListener.enabled = false;
        m_heloExteriorCamera.enabled = false;
        m_heloInteriorCamera.enabled = false;
        m_heloMovement.enabled = false;
        m_heloWeaponSystem.enabled = false;
        m_rotorRotations[0].enabled = false;
        m_rotorRotations[1].enabled = false;
        m_heloRigidBody.constraints = RigidbodyConstraints.FreezeAll;

    }

    private void Update()
    {
        if (m_isPlayerAlive && Input.GetKeyDown(KeyCode.P))
        {
            SwitchPhase();
        }
        if (m_numberOfEnemies <= 9)
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("WinScene");
        }
        if (m_numberOfFriendlyUnits <= 1)
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("LoseScene");
        }
    }

    private void SwitchPhase()
    {
        if (m_RTSPhase)
        {
            SwitchToThirdPerson();
        }
        else if (m_thirdPersonPhase)
        {
            SwitchToRTS();
        }

    }

    private void SwitchToRTS()
    {
        //if the player is in the helo then it should not be able to switch to RTS phase
        if(m_isPlayerInHelo)
        {
            return;
        }
        Cursor.lockState = CursorLockMode.None;
        m_RTSPhase = true;
        m_thirdPersonPhase = false;
       for(int i = 0; i < m_RTSGameObjects.Length; i++)
        {
            m_RTSGameObjects[i].SetActive(true);
        }
        for (int i = 0; i < m_thirdPersonGameObjects.Length; i++)
        {
            m_thirdPersonGameObjects[i].SetActive(false);
        }
        m_characterAimManager.enabled = false;
        m_characterMovement.enabled = false;
        m_playerWeaponController.enabled = false;
    }

    private void SwitchToThirdPerson()
    {
        Cursor.lockState = CursorLockMode.Locked;
        m_RTSPhase = false;
        m_thirdPersonPhase = true;
        for(int i = 0; i < m_RTSGameObjects.Length; i++)
        {
            m_RTSGameObjects[i].SetActive(false);
        }
        m_characterAimManager.enabled = true;
        m_characterMovement.enabled = true;
        m_playerWeaponController.enabled = true;
        for (int i = 0; i < m_thirdPersonGameObjects.Length; i++)
        {
            m_thirdPersonGameObjects[i].SetActive(true);
        }
    }
    public void PlayerDie()
    {
        SwitchToRTS();
        Destroy(m_player);
    }

    public void EnterTheHelo()
    {
        if(m_RTSPhase)
        {
            return;
        }
        m_player.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        m_jUFootPlacement.enabled = false;
        m_isPlayerInHelo = true;
        m_heloExteriorCamera.enabled = true;
        m_heloInteriorCamera.enabled = false;
        m_heloMovement.enabled = true;
        m_heloWeaponSystem.enabled = true;
        m_rotorRotations[0].enabled = true;
        m_rotorRotations[1].enabled = true;
        m_characterAimManager.enabled = false;
        m_characterMovement.enabled = false;
        m_playerWeaponController.enabled = false;
        m_heloWeaponSystem.crosshairImage.gameObject.SetActive(true);
        m_playerAnimator.enabled = false;
        m_player.transform.SetParent(m_playerHelo.transform);
        m_heloAudioListener.enabled = true;
        m_heloRigidBody.constraints = RigidbodyConstraints.None;
    }

    public void ExitTheHelo()
    {
        m_heloAudioListener.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        m_playerModel.SetActive(true);
        m_player.transform.SetParent(null);
        m_playerAnimator.enabled = true;
        m_jUFootPlacement.enabled = true;
        m_isPlayerInHelo = false;
        m_heloExteriorCamera.enabled = false;
        m_heloInteriorCamera.enabled = false;
        m_heloMovement.enabled = false;
        m_heloWeaponSystem.enabled = false;
        m_rotorRotations[0].enabled = false;
        m_rotorRotations[1].enabled = false;
        m_characterAimManager.enabled = true;
        m_characterMovement.enabled = true;
        m_playerWeaponController.enabled = true;
        m_heloWeaponSystem.crosshairImage.gameObject.SetActive(false);
        m_player.SetActive(true);
        m_heloRigidBody.constraints = RigidbodyConstraints.FreezeAll;

    }
}
