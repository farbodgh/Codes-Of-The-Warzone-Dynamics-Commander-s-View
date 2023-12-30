using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeloWeaponSystem : MonoBehaviour
{
    //refrences
    private Rigidbody m_rb;
    //Reference to HeloCameraController
    private Helicopter m_helicopterController;

    //Cameras
    private Camera m_interiorCamera;
    private Camera m_exteriorCamera;
    private Camera m_activeCamera;

    //Weapon System Game Objects
    private GameObject m_minigun;
    private GameObject m_barrel;
    private GameObject m_bulletSpawnPoint;

    //Shooting Setting
    private float m_minigunFireRate = 10.0f;
    private float m_timeSinceLastShoot = 0f;

    //Ground Detection
    private LayerMask m_gourndLayer;

    [Header("Projectile")]
    [SerializeField] private GameObject m_bulletPrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip m_minigunSound;
    [SerializeField] private AudioClip m_missileLaunchSound;
    private AudioSource m_audioSource;

    [Header("Input")]
    [SerializeField] private float m_mouseSensitivity = 75;
    Vector3 mousePosition;
    private float m_xRotation;

    [Header("UI")]
    public Image crosshairImage;
    private Queue<GameObject> m_missiles = new Queue<GameObject>(2);

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        //Creating and initializing the AudioSource Component;
        GameObject audioSourceObject = new GameObject("MyAudioSource");
        m_audioSource = audioSourceObject.AddComponent<AudioSource>();
        m_audioSource.clip = m_minigunSound;
        m_audioSource.playOnAwake = false;
        m_audioSource.volume = 0.5f;
        m_audioSource.pitch = 1.0f;
        m_missiles.Enqueue(gameObject.transform.Find("Regular/Missile_URP (1)").gameObject);
        m_missiles.Enqueue(gameObject.transform.Find("Regular/Missile_URP (2)").gameObject);

        m_gourndLayer = LayerMask.GetMask("Ground");
    }
    void Start()
    {
        m_helicopterController = GetComponent<Helicopter>();
        m_interiorCamera = m_helicopterController.interiorCamera;
        m_exteriorCamera = m_helicopterController.exteriorCamera;

        m_minigun = gameObject.transform.Find("Regular/MiniGunFixed").gameObject;
        m_barrel = gameObject.transform.Find("Regular/MiniGunFixed/MiniGunBarrel").gameObject;
        m_bulletSpawnPoint = gameObject.transform.Find("Regular/MiniGunFixed/MiniGunBarrel/BulletSpawnPoint").gameObject;



    }

    // Update is called once per frame
    void Update()
    {

        Aim();
        UpdateActiveCamera();

        m_timeSinceLastShoot += Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            if (m_timeSinceLastShoot >= (1 / m_minigunFireRate))
            {
                m_audioSource.Play();
                Instantiate(m_bulletPrefab, m_bulletSpawnPoint.transform.position, m_bulletSpawnPoint.transform.rotation);
                m_timeSinceLastShoot = 0;
            }

        }

        //if (Input.GetMouseButtonUp(1))
        //{
        //    FireMissile();
        //}

    }

    void Fire()
    {

        // Reset the time since last instantiation
        m_timeSinceLastShoot += Time.deltaTime;

        // Firing Minigun
        if (Input.GetMouseButton(0))
        {
            if (m_timeSinceLastShoot >= .05f)
            {
                Instantiate(m_bulletPrefab, m_bulletSpawnPoint.transform.position, m_bulletSpawnPoint.transform.rotation).GetComponent<Rigidbody>().AddForce(Vector3.forward * 10000);
            }
            m_timeSinceLastShoot = 0;
        }

    }


    void Aim()
    {
        float mouseX = Input.GetAxis("Mouse X") * m_mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * m_mouseSensitivity * Time.deltaTime;

        m_xRotation -= mouseY;
        m_xRotation = Mathf.Clamp(m_xRotation, -95, -55);

        // Constrain mouseX between 55 and -55
        mouseX = Mathf.Clamp(mouseX, -55, 55);
        m_minigun.transform.Rotate(new Vector3(0, mouseX, 0));
        //m_minigun.transform.localRotation = Quaternion.Euler(new Vector3(0, mouseX, 0));
        m_barrel.transform.localRotation = Quaternion.Euler(new Vector3(m_xRotation, 0, 0));

        // Perform a raycast from the m_barrel position
        RaycastHit hit;
        if (Physics.Raycast(m_bulletSpawnPoint.transform.position, m_bulletSpawnPoint.transform.forward, out hit, Mathf.Infinity, m_gourndLayer))
        {
            // Position the crosshair at the collision point in screen space
            Vector3 screenPoint = m_activeCamera.WorldToScreenPoint(hit.point);
            crosshairImage.transform.position = screenPoint;
        }
    }

    private void UpdateActiveCamera()
    {
        if (m_helicopterController.isExteriorView)
        {
            m_activeCamera = m_exteriorCamera;
        }
        if (m_helicopterController.isInteriorView)
        {
            m_activeCamera = m_interiorCamera;
        }
    }


    //private void FireMissile()
    //{
    //    if (m_missiles.Count <= 0)
    //    {
    //        return;
    //    }
    //    RaycastHit hit;
    //    if (Physics.Raycast(m_bulletSpawnPoint.transform.position, m_bulletSpawnPoint.transform.forward, out hit, Mathf.Infinity, m_gourndLayer))
    //    {
    //        // Position the crosshair at the collision point in screen space
    //        Vector3 screenPoint = m_activeCamera.WorldToScreenPoint(hit.point);
    //        crosshairImage.transform.position = screenPoint;
            

    //        var missile = m_missiles.Dequeue();
    //        missile.transform.Find("MissileParent").gameObject.SetActive(true);
    //        missile.GetComponent<HeloMissile>().Launch(screenPoint);
    //        m_audioSource.PlayOneShot(m_missileLaunchSound);
    //        missile.transform.parent.transform.SetParent(null);
    //    }
    //}
}

