using System.Collections.Generic;
using UnityEngine;

public class CIWS : MonoBehaviour
{
    private AudioSource m_audioSource;
    [SerializeField]
    private GameObject m_projectilePrefab;
    private GameObject m_target;
    private GameObject m_secondTarget;
    private Queue<GameObject> m_bullets = new Queue<GameObject>();
    private GameObject m_tmpBullet;
    private Transform m_rotationAlignment;
    private Transform m_heightAlignment;
    private Transform m_barrel;
    private Transform m_bulletSpawnPoint;


    private float m_BarrelRotationSpeed = 180f;
    private float m_rotationAlignmentSpeed = 175;
    private float m_heightAlignmentSpeed = 90f;
    private float m_timeSinceLastLaunch;
    private float m_coolDownTime;
    private const float BULLETSPEED = 1028;

    private int m_range = 450;
    private int m_fireRate = 30;

    private bool m_isShooting = false;
    //Target related variables
    private bool m_isNewTarget = false;
    private Rigidbody m_targetRigidBody;
    private Vector3 m_targetPredictedPosition;



    private void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.loop = true;
        m_rotationAlignment = gameObject.transform.Find("Rotation");
        m_heightAlignment = gameObject.transform.Find("Rotation/gun_leggs_low/radar_parent");
        m_barrel = gameObject.transform.Find("Rotation/gun_leggs_low/radar_parent/barrels_low");
        m_bulletSpawnPoint = gameObject.transform.Find("Rotation/gun_leggs_low/radar_parent/BulletSpawnPoint");
        m_coolDownTime = 1f / m_fireRate;
        InitializeObjectPool();
    }


    void Update()
    {
        if (m_audioSource)
        {
            SoundController();
        }

        if (m_target != null)
        {
            CheckForNewTarget();
            Aim();
            Shoot();
        }


    }

    void FixedUpdate()
    {
        BarrelRotation();
    }

    void InitializeObjectPool()
    {
        GameObject tmpBullet;
        for (int i = 0; i < 95; i++)
        {
            tmpBullet = Instantiate(m_projectilePrefab, m_bulletSpawnPoint.position, m_bulletSpawnPoint.transform.rotation);
            tmpBullet.SetActive(false);
            m_bullets.Enqueue(tmpBullet);
        }
    }

    private void Shoot()
    {
        if (!m_target)
        {
            m_isShooting = false;
            return;
        }
        if (m_bullets.Peek().activeInHierarchy)
        {
            return;
        }
        //Shooting projectiles when the cooldown time is reached and the target is in range
        if (Time.time - m_timeSinceLastLaunch > m_coolDownTime && Vector3.Distance(transform.position, m_target.transform.position) <= m_range)
        {
            m_isShooting = true;
            m_timeSinceLastLaunch = Time.time;
            //First get a reference from the bullet
            m_tmpBullet = m_bullets.Dequeue();
            //Then set its position and rotation
            m_tmpBullet.transform.position = m_bulletSpawnPoint.position;
            m_tmpBullet.transform.rotation = m_bulletSpawnPoint.rotation;
            //Then set its target position (in this way the Turret accuracy increased)
            m_tmpBullet.gameObject.GetComponent<BulletCIWS>().targetPosition = m_targetPredictedPosition;
            //Then activate it
            m_tmpBullet.SetActive(true);
            //Then add it to the queue again so it will be ready to be used in the object pool
            m_bullets.Enqueue(m_tmpBullet);
        }
    }

    private void Aim()
    {
        if (!m_target)
        {
            return;
        }
        //Predict the target position based on simple physics, considering the target's velocity is constant
        m_targetPredictedPosition = PredictTargetPosition();
        //Rotation Alignment
        //Calculate the direction to the target
        Vector3 targetDirection = m_target.transform.position - m_barrel.position;
        targetDirection.y = 0;
        targetDirection.Normalize();

        //Calculate the rotation angle
        float angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;


        //gradually change the rotation
        Quaternion desiredRotation = Quaternion.Euler(m_rotationAlignment.localEulerAngles.x, angle, m_rotationAlignment.localEulerAngles.z);

        //Rotate towards the desired rotation
        m_rotationAlignment.localRotation = Quaternion.RotateTowards(m_rotationAlignment.localRotation, desiredRotation, m_rotationAlignmentSpeed * Time.deltaTime);


        //Height Alignment 
        Vector3 targetDirectionHeight = m_targetPredictedPosition - m_barrel.position;

        if (Vector3.Distance(m_target.transform.position, m_barrel.position) > m_range + 150)
        {
            return;
        }

        float barrelAngle = Vector3.SignedAngle(m_heightAlignment.up, targetDirectionHeight, m_heightAlignment.right);

        barrelAngle = Mathf.Clamp(barrelAngle, 25, 100);

        // Calculate the desired rotation
        Quaternion desiredRotationbarrel = Quaternion.Euler(barrelAngle, m_heightAlignment.localEulerAngles.y, m_heightAlignment.localEulerAngles.z);
        
        // Smoothly pitch the m_heightAlignment towards the desired rotation (turret's barrel aims toward the target based on its height)
        m_heightAlignment.localRotation = Quaternion.RotateTowards(m_heightAlignment.localRotation, desiredRotationbarrel, m_heightAlignmentSpeed * Time.deltaTime);



    }



    private void BarrelRotation()
    {
        m_barrel.transform.Rotate(0, 0, Time.fixedDeltaTime * m_BarrelRotationSpeed);
    }

    private Vector3 PredictTargetPosition()
    {
        if (m_target && m_targetRigidBody)
        {
            //Debug.Log($"Target velocity is : {m_targetRigidBody.velocity}");

            float bulletTravelTime = Vector3.Distance(m_target.transform.position, m_bulletSpawnPoint.position) / BULLETSPEED;
            Vector3 predictedPosition = m_target.transform.position + m_targetRigidBody.velocity * bulletTravelTime;
            return predictedPosition;
        }
       
        return Vector3.zero;

    }

    private void CheckForNewTarget()
    {
        if (m_isNewTarget && m_target)
        {
            m_isNewTarget = false;
        }

    }

    //This function gets details about new target from the AirDefence system
    public void GetNewTarget(GameObject target)
    {
        m_target = target;
        m_targetRigidBody = m_target.GetComponent<Rigidbody>();
        if(m_target.transform.transform.parent != null)
        {
            m_targetRigidBody = m_target.transform.parent.GetComponent<Rigidbody>();
        }
        m_isNewTarget = true;
    }

    void SoundController()
    {
        if (m_audioSource.isPlaying && !m_target)
        {
            m_audioSource.Stop();
            return;
        }

        if(m_audioSource.isPlaying && m_target) 
        {
            if(Vector3.Distance(transform.position, m_target.transform.position) > m_range)
            {
                m_audioSource.Stop();
                return;
            }
            return;
        }

        if (!m_audioSource.isPlaying && m_isShooting && m_target && !(Vector3.Distance(transform.position, m_target.transform.position) > m_range))
        {
            m_audioSource.Play();
            return;
        }

        if (m_audioSource.isPlaying && !m_isShooting)
        {
            m_audioSource.Stop();
            return;
        }
    }

    public bool IsSystemReady()
    {
        if (m_target == null && m_secondTarget == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
