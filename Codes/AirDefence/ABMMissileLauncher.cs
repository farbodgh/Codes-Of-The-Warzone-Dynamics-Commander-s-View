using UnityEngine;

public class ABMMissileLauncher : MonoBehaviour
{
    //GameOnjects
    private GameObject m_target;
    [SerializeField]
    private GameObject m_projectilePrefab;
    private GameObject[] m_missles = new GameObject[4];
    private GameObject[] m_spawnPoints = new GameObject[4];

    //booleans
    private bool m_newTarget = false;
    private bool m_canShootMissile = true;



    private float m_coolDownTime = 2.5f;
    //private float m_reloadingTime = 12;
    private float m_timeSinceLastLaunch;

    private void Awake()
    {
        InitializingObjectPool();
    }

    private void Update()
    {
        if (m_newTarget)
        {
            if (m_canShootMissile)
            {
                LaunchMissile();
            }
        }

       
        m_canShootMissile = CanShootMissile();

    }
    public void InitializingObjectPool()
    {
        for (int i = 0; i < m_missles.Length; i++)
        {

            m_spawnPoints[i] = gameObject.transform.Find("SpawnPoint" + (i + 1)).gameObject;

        }
        for (int i = 0; i < m_missles.Length; i++)
        {
            //The following line instantiates the missiles at the spawn points, but it preserves its rotation.
            m_missles[i] = Instantiate(m_projectilePrefab, m_spawnPoints[i].transform.position, m_projectilePrefab.transform.rotation);
            m_missles[i].SetActive(false);
            m_missles[i].GetComponent<ABM>().m_spawnPoint = m_spawnPoints[i].transform.position;
            m_missles[i].name = gameObject.name + "'sMissile" + (i + 1);
        }

    }


    public void GetTarget(GameObject target)
    {
        this.m_target = target;
        m_newTarget = true;
    }

    public bool CanShootMissile()
    {
        //There must be at least two missiles in the pool to shoot.
        if (Time.time - m_timeSinceLastLaunch < m_coolDownTime)
        {
            //Debug.Log("CanShootMisslefalse with time: " + Time.time);
            return false;
        }
        short missileCount = 0;
        foreach (GameObject missile in m_missles)
        {
            // Debug.Log("Inside the foreach loop");
            if (!missile.activeInHierarchy)
            {
                // Debug.Log("another available missile is found");
                missileCount++;
            }
        }
        if (missileCount >= 2)
        {
            //Debug.Log("CanShootMissile() is returning true");
            return true;
        }
        else
        {
            return false;
        }
    }
    private void LaunchMissile()
    {
        //Debug.Log("Shooting a missile at " + Time.time + " with target: " + m_target.name + " and position: " + m_target.transform.position);
        m_newTarget = false;

        short numberOfAvailableMissiles = 0;
        for (int i = 0; i < m_missles.Length; i++)
        {
            if (!m_missles[i].activeInHierarchy)
            {
                numberOfAvailableMissiles++;
            }
        }

        //If the launcher has less than two missiles, it will not shoot.
        // Ensure at least two missiles are inactive(ready to be shoot) before launching
        if (numberOfAvailableMissiles >= 2)
        {
            short missilesLaunched = 0;
            for (int i = 0; i < m_missles.Length && missilesLaunched < 2; i++)
            {
                if (!m_missles[i].activeInHierarchy)
                {
                    m_missles[i].transform.position = m_spawnPoints[i].transform.position;
                    m_missles[i].GetComponent<ABM>().m_target = this.m_target;
                    m_missles[i].SetActive(true);
                    missilesLaunched++;
                }
            }
            m_timeSinceLastLaunch = Time.time;
        }
    }



    
}
