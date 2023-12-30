using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Security.Claims;
using Unity.Mathematics;
using Unity.Jobs;

public class FriendlyAgent : MonoBehaviour, IDestroyable, IJob
{
    private NavMeshAgent m_navMeshAgent;

    private float m_hitPoint;
    private float m_armor;
    private float m_damage;


    private bool m_isAlive;
    private bool m_isUserGivingOrder;


    
    #region Detection
    [SerializeField] private Transform[] m_raycastSpawnPoints;
    [SerializeField] private Transform m_raycastParent;
    //The range that the soldier can see enemies
    private float m_raycastRange = 210;
    private LayerMask m_enemyLayerMask;
    private Transform m_target;

    private float m_rotationSpeed = 30f;
    private float m_currentRotation;
    #endregion


    #region Attack
    private float m_attackRange = 90;

    const int NUMBEROFBULLETS = 40;
    private Queue<GameObject> m_bulletQueue = new Queue<GameObject>(NUMBEROFBULLETS);
    [SerializeField] private Transform m_bulletSpawnPoint;
    [SerializeField] private GameObject m_bullet;
    private GameObject m_currentBullet;

    private float m_timeSinceLastLaunch;
    private float m_coolDownTime;
    private int m_fireRate = 2;
    #endregion

    #region Animation
    private Animator m_animator;

    public Transform aimPoint;
    private Transform m_noTargetAimPoint;
    #endregion

    #region Sound
    [SerializeField] private AudioClip m_ShootingSound;
    private AudioSource m_audioSource;
    [SerializeField] private GameObject m_m4;
    #endregion

    #region Vision
    private Ray m_visionRay;
    private float m_raycastAngle = 0f;
    private float m_raycastRotationSpeed = 30f;
    #endregion

    private enum m_soldierStates
    {
        Idle,
        Running,
        Shooting,
    }

    private m_soldierStates m_currentSoldierState;
    void Awake()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_enemyLayerMask = LayerMask.GetMask("EnemySoldier");
        m_animator = GetComponent<Animator>();

        aimPoint = transform.Find("FriendlyCharacter/Rig 1/AimPoint");
        m_noTargetAimPoint = transform.Find("FriendlyCharacter/Rig 1/NoTargetAimPoint");

        //m_fireRateTimer = m_fireRate;
        m_audioSource = m_m4.GetComponent<AudioSource>();

        m_coolDownTime = 1f / m_fireRate;

    }

    void Start()
    {
        GameManager.Instance.m_numberOfFriendlyUnits++;
        aimPoint.position = m_noTargetAimPoint.position;

        UnitRegisterer.Instance.allSoldiers.Add(gameObject);
        m_isAlive = true;
        m_isUserGivingOrder = false;
        m_hitPoint = 100;
        m_armor = .1f;
        m_currentSoldierState = m_soldierStates.Idle;
        m_navMeshAgent.stoppingDistance = 2f;
        InitializeObjectPools();
    }

    void OnDestroy()
    {
        GameManager.Instance.m_numberOfFriendlyUnits--;
    }

    void Update()
    {

        ((IJob)this).Execute();

        //ResetDestination();   
    }




    //this method handles the soldier's AI
    private void DecisionMaker()
    {

        if (m_isUserGivingOrder)
        {

            //chekcs when the soldier reach its destination that the player gave
            if (m_navMeshAgent.remainingDistance <= m_navMeshAgent.stoppingDistance)
            {

                m_isUserGivingOrder = false;
            }
            return;
        }

        if (!m_isUserGivingOrder)
        {
            //AI logics

            if (m_target != null)
            {
                RotateTowardsTarget();

                aimPoint.position = m_target.position + Vector3.up * 2f;

                if (Vector3.Distance(transform.position, m_target.position) <= m_attackRange)
                {
                    //Debug.DrawLine(m_bulletSpawnPoint.position, aimPoint.position, Color.green);

                    m_navMeshAgent.ResetPath();
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
                    Debug.Log($"!!I am {gameObject.name}Moving toward the enemy {m_target.name} at position {m_target.transform.position}");
                    if(m_navMeshAgent.destination != m_target.position)
                    Move(m_target.position);
                    m_navMeshAgent.isStopped = false;
                    m_currentSoldierState = m_soldierStates.Running;
                }
            }
            else
            {
                //Debug.DrawLine(m_bulletSpawnPoint.position, m_noTargetAimPoint.position, Color.red);
                aimPoint.position = m_noTargetAimPoint.position;
                m_target = null;
                m_navMeshAgent.ResetPath();
                m_currentSoldierState = m_soldierStates.Idle;
                m_navMeshAgent.isStopped = false;

            }
            SetAnimationState(m_currentSoldierState);

        }
    }

    //This method is called when the soldier getts an order from the player
    public void GetOrder(Vector3 destination)
    {
        m_isUserGivingOrder = true;
        m_navMeshAgent.isStopped = false;
        m_currentSoldierState = m_soldierStates.Running;
        SetAnimationState(m_currentSoldierState);
        Move(destination);

        aimPoint.position = m_noTargetAimPoint.position;
        m_target = null;
    }

    public void GetOrderToKill(GameObject enemy)
    {
        if(enemy == null)
        {
            return;
        }
        m_target = enemy.transform;
        m_navMeshAgent.ResetPath();
        if((enemy.transform.position - transform.position).sqrMagnitude <= m_attackRange * m_attackRange)
        {
            m_navMeshAgent.SetDestination(enemy.transform.position);
        }


    }

    //handles the soldier's movement
    private void Move(Vector3 destination)
    {
        m_navMeshAgent.SetDestination(destination);
    }

    //The following methods handle the soldier's damage and dying
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


    private void Vision()
    {
        if(m_target != null)
        {
            return;
        }

        m_raycastAngle += m_raycastRotationSpeed * Time.deltaTime;
        for (int i = 0; i < m_raycastSpawnPoints.Length; i++)
        {

            m_visionRay = new Ray(m_raycastSpawnPoints[i].position, Quaternion.AngleAxis(m_raycastAngle, Vector3.up) * m_raycastSpawnPoints[i].forward);
            RaycastHit hit;
            if (Physics.Raycast(m_visionRay, out hit, m_raycastRange, m_enemyLayerMask))
            {
                Debug.DrawRay(m_raycastSpawnPoints[i].position, Quaternion.AngleAxis(m_raycastAngle, Vector3.up) * m_raycastSpawnPoints[i].forward * m_raycastRange, Color.yellow);
                
                m_navMeshAgent.SetDestination(hit.transform.position);

                m_target = hit.transform;
                break;
             


            }
            else
            {
                Debug.DrawRay(m_raycastSpawnPoints[i].position, Quaternion.AngleAxis(m_raycastAngle, Vector3.up) * m_raycastSpawnPoints[i].forward * m_raycastRange, Color.blue);
            }
        }
    }
    void RotateTowardsTarget()
    {
        Vector3 directionToTarget = m_target.position - gameObject.transform.position;
        directionToTarget.y = 0; // Ignore the y-axis for rotation

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
                break;

            case m_soldierStates.Running:
                m_animator.SetBool("isRunning", true);
                m_animator.SetBool("isShooting", false);
                break;

            case m_soldierStates.Shooting:
                m_animator.SetBool("isShooting", true);
                m_animator.SetBool("isRunning", false);
                break;
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

    public void InitializeObjectPools()
    {
        for (int i = 0; i < NUMBEROFBULLETS; i++)
        {
            var bullet = Instantiate(m_bullet, m_bulletSpawnPoint.position, m_bulletSpawnPoint.rotation);
            Debug.Log($"bullet : {bullet.name}");
            bullet.GetComponent<FriendlyAgentBullet>().bulletSpawnPoint = m_bulletSpawnPoint;
            Debug.Log($"aimpoint : {aimPoint.name}");
            bullet.GetComponent<FriendlyAgentBullet>().bulletSpawnPoint.transform.rotation = m_bulletSpawnPoint.rotation;
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

    //bool IsAgentMoving()
    //{
    //    // Check if the agent has a path and is moving
    //    return   m_navMeshAgent.velocity.magnitude > 0.1f;
    //}

    //public void ResetDestination()
    //{
    //    if (m_target == null)
    //    {
    //        m_navMeshAgent.ResetPath();
    //    }
    //}
}
