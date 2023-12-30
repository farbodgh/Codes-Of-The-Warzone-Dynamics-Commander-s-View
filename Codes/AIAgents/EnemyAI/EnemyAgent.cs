using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAgent : MonoBehaviour , IDestroyable, IJob
{
    private NavMeshAgent m_navMeshAgent;

    private bool m_isAlive;

    private float m_hitPoint;
    private float m_armor;
    private float m_damage;

    #region Detection
    [SerializeField] private Transform[] m_raycastSpawnPoints;
    [SerializeField] private Transform m_raycastParent;
    //The range that the soldier can see enemies
    private float m_raycastRange = 210;
    private Transform m_target;
    [SerializeField] private LayerMask m_detectionLayerMask;
    #endregion


    #region Attack
    private float m_attackRange = 90;
    private float m_currentDistanceToTarget;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private Transform m_bulletSpawnPoint;


    const int NUMBEROFBULLETS = 40;
    private Queue<GameObject> m_bulletQueue = new Queue<GameObject>(NUMBEROFBULLETS);
    private GameObject m_currentBullet;

    private float m_timeSinceLastLaunch;
    private float m_coolDownTime;
    private int m_fireRate = 2;
    #endregion


    #region Wander
    private float m_WanderDistance = 10f;
    private bool m_isWandering;
    private float m_currentWanderTimer;
    private float m_maxWanderTimer = 30f;
    #endregion

    #region Animation 
    private Animator m_animator;
    public Transform aimPoint;
    private Transform m_noTargetAimPoint;
    #endregion

    private Ray m_visionRay;
    private float m_raycastAngle = 0f;
    private float m_raycastRotationSpeed = 30f;

    [SerializeField] private AudioClip m_ShootingSound;
    [SerializeField]
    private GameObject m_m4;
    private AudioSource m_audioSource;

    private enum m_soldierStates
    {
        Idle,
        Wandering,
        Running,
        Shooting,
    }


    private m_soldierStates m_currentSoldierState;

    private void Awake()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_animator = GetComponent<Animator>();

        aimPoint = transform.Find("EnemyCharacter/Rig 1/AimPoint");
        m_noTargetAimPoint = transform.Find("EnemyCharacter/Rig 1/NoTargetAimPoint");

        m_audioSource = m_m4.GetComponent<AudioSource>();

        m_coolDownTime = 1f / m_fireRate;
    }

    void Start()
    {
        m_isAlive = true;
        aimPoint.position = m_noTargetAimPoint.position;
        //Debug.DrawLine(m_bulletSpawnPoint.position, m_noTargetAimPoint.position, Color.red);
        m_hitPoint = 100f;
        m_armor = .1f;
        m_currentSoldierState = m_soldierStates.Idle;
        m_currentWanderTimer = Time.time;
        m_navMeshAgent.stoppingDistance = 2f;
        InitializeObjectPools();
        GameManager.Instance.m_numberOfEnemies++;

    }

    private void Update()
    {

        ((IJob)this).Execute();

    }

    private void DecisionMaker()
    {
        if (m_target != null)
        {

            RotateTowardsTarget();
            aimPoint.position = m_target.position + Vector3.up * 2f;

            m_currentDistanceToTarget = Vector3.Distance(transform.position, m_target.position);
            if (m_currentDistanceToTarget <= m_attackRange)
            {

                m_navMeshAgent.isStopped = true;

                //fire bullets
                if (CanFire())
                {
                    m_currentSoldierState = m_soldierStates.Shooting;
                    Fire();
                }
                else
                {
                    m_currentSoldierState = m_soldierStates.Idle;
                }
            }
            else
            {
                //if the target is out of range, then stop shooting
                if (m_currentDistanceToTarget >= m_attackRange + 10f)
                {
                    m_target = null;
                    m_currentSoldierState = m_soldierStates.Wandering;
                    return;
                }
                m_navMeshAgent.SetDestination(m_target.position);
                m_navMeshAgent.isStopped = false;
                m_currentSoldierState = m_soldierStates.Running;
            }
        }
        else
        {


            Debug.DrawLine(m_bulletSpawnPoint.position, m_noTargetAimPoint.position, Color.red);
            aimPoint.position = m_noTargetAimPoint.position;
            m_target = null;

            if (m_isWandering && m_navMeshAgent.remainingDistance <= m_navMeshAgent.stoppingDistance)
            {
                m_currentSoldierState = m_soldierStates.Idle;
                m_isWandering = false;
                m_navMeshAgent.isStopped = true;
                m_currentWanderTimer = Time.time;
            }

            if (!m_isWandering && m_currentWanderTimer + m_maxWanderTimer <= Time.time)
            {
                Wander();
            }

        }

        SetAnimationState(m_currentSoldierState);
    }

    private void Wander()
    {
        m_currentSoldierState = m_soldierStates.Wandering;
        m_isWandering = true;
        m_navMeshAgent.isStopped = false;
        m_navMeshAgent.SetDestination(GetRandomPosition());
    }

    public Vector3 GetRandomPosition()

    {
        Vector3 random = Random.insideUnitSphere * m_WanderDistance;
        random.y = 0f;
        Vector3 nextLocation = transform.position + random;
        if (NavMesh.SamplePosition(nextLocation, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            nextLocation = hit.position;

        }
        return nextLocation;
    }
    private void Vision()
    {
        
        //m_raycastParent.Rotate(0, 30 * Time.deltaTime, 0);
        // Check if the agent already has a target
        if (m_target != null)
        {
            return;
        }
        m_raycastAngle += m_raycastRotationSpeed * Time.deltaTime;
        for (int i = 0; i < m_raycastSpawnPoints.Length; i++)
        {
            m_visionRay = new Ray(m_raycastSpawnPoints[i].position, Quaternion.AngleAxis(m_raycastAngle, Vector3.up) * m_raycastSpawnPoints[i].forward);
            RaycastHit hit;
            if (Physics.Raycast(m_visionRay, out hit, m_raycastRange, m_detectionLayerMask))
            {
                Debug.DrawRay(m_raycastSpawnPoints[i].position, Quaternion.AngleAxis(m_raycastAngle, Vector3.up) * m_raycastSpawnPoints[i].forward * m_raycastRange, Color.red);
                Debug.Log("I Detected the Target");
                m_navMeshAgent.SetDestination(hit.transform.position);

                m_target = hit.transform;


            }
            else
            {
                Debug.DrawRay(m_raycastSpawnPoints[i].position, Quaternion.AngleAxis(m_raycastAngle, Vector3.up) * m_raycastSpawnPoints[i].forward * m_raycastRange, Color.yellow);
            }
        }
    }
    void RotateTowardsTarget()
    {
        Vector3 directionToTarget = m_target.position - gameObject.transform.position;
        // Ignore the y-axis for rotation
        directionToTarget.y = 0;

        // Calculate the rotation based on the gun's forward direction
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);

        // Apply the rotation to the NPC
        transform.rotation = targetRotation;
    }

    private void SetAnimationState(m_soldierStates state)
    {

        switch (state)
        {
            case m_soldierStates.Idle:
                m_animator.SetBool("isRunning", false);
                m_animator.SetBool("isShooting", false);
                m_animator.SetBool("isWandering", false);
                break;

            case m_soldierStates.Running:
                m_animator.SetBool("isRunning", true);
                m_animator.SetBool("isShooting", false);
                m_animator.SetBool("isWandering", false);
                break;

            case m_soldierStates.Shooting:
                m_animator.SetBool("isShooting", true);
                m_animator.SetBool("isRunning", false);
                m_animator.SetBool("isWandering", false);
                break;

            case m_soldierStates.Wandering:
                m_animator.SetBool("isRunning", false);
                m_animator.SetBool("isShooting", false);
                m_animator.SetBool("isWandering", true);
                break;
        }
    }

    void IDestroyable.GettingAnnihilated()
    {
        m_isAlive = false;
        Destroy(gameObject);
    }
    void IDestroyable.GetHit(int damage, float armorPiercing = 0)
    {
        if (armorPiercing >= m_armor)
        {
            m_hitPoint -= damage;
        }
        else if (armorPiercing < m_armor)
        {
            m_hitPoint -= damage * (1 - armorPiercing);
        }
        if (m_hitPoint <= 0)
        {
            ((IDestroyable)this).GettingAnnihilated();
        }
    }
    bool CanFire()
    {
        if (Time.time - m_timeSinceLastLaunch > m_coolDownTime)
        {
            m_timeSinceLastLaunch = Time.time;
            return true;
        }

        return false;
    }
    private void Fire()
    {


        m_currentBullet = m_bulletQueue.Dequeue();
        m_audioSource.PlayOneShot(m_ShootingSound);
        m_currentBullet.SetActive(true);
        m_bulletQueue.Enqueue(m_currentBullet);


    }


    void OnDestroy()
    {
        GameManager.Instance.m_numberOfEnemies--;
    }

    public void InitializeObjectPools()
    {
        for (int i = 0; i < NUMBEROFBULLETS; i++)
        {
            var bullet = Instantiate(m_bullet, m_bulletSpawnPoint.position, m_bulletSpawnPoint.rotation);
            Debug.Log($"bullet : {bullet.name}");
            bullet.GetComponent<EnemyAgentsBullet>().bulletSpawnPoint = m_bulletSpawnPoint;
            Debug.Log($"aimpoint : {aimPoint.name}");
            bullet.GetComponent<EnemyAgentsBullet>().bulletSpawnPoint.transform.rotation = m_bulletSpawnPoint.rotation;
            bullet.SetActive(false);
            m_bulletQueue.Enqueue(bullet);
        }

    }
    void IJob.Execute()
    {
        if (m_isAlive)
        {

            Vision();
            DecisionMaker();
        }
    }
}
