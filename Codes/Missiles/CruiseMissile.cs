using Unity.Jobs;
using UnityEngine;

public class CruiseMissile : MonoBehaviour, IJob, IExplosible
{
    private Transform m_target;
    private float m_speed = 250f;
    private float m_rotationSpeed = 120f;
    private Rigidbody m_rb;
    private Transform m_missileParent;
    private AudioSource m_audioSource;
    private enum MissileState { VerticalAscent, Horizontal, Descent }
    private MissileState currentState = MissileState.VerticalAscent;

    //Collision related logics
    private const int ExplosionEffectRadius = 25;
    private GameObject m_explosionEffect;
    private GameObject m_missileBody;

    private void Awake()
    {
        //MissileControllingsystem.Instance.RegisterCruiseMissile(gameObject);
        m_explosionEffect = transform.Find("Explosion").gameObject;
        m_missileBody = transform.Find("CruiseParrent/CruiseMissile").gameObject;
        gameObject.SetActive(false);
    }


    void OnEnable()
    {
        m_rb = GetComponent<Rigidbody>();
        m_missileParent = transform.Find("CruiseParrent");
        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.Play();

    }
    private void Update()
    {
        if (!m_target)
        {
            return;
        }
        Vector3 targetDirection = Vector3.zero;

        switch (currentState)
        {
            case MissileState.VerticalAscent:
                targetDirection = Vector3.up;
                if (m_missileParent.position.y >= 210)
                {
                    currentState = MissileState.Horizontal;
                }
                break;

            case MissileState.Horizontal:
                targetDirection = (m_target.position - m_missileParent.position).normalized;
                targetDirection.y = 0;
                if (Vector3.Distance(new Vector3(m_target.position.x, 0, m_target.position.z), new Vector3(m_missileParent.position.x, 0, m_missileParent.position.z)) <= 550)
                {
                    currentState = MissileState.Descent;
                }
                break;

            case MissileState.Descent:
                targetDirection = (m_target.position - m_missileParent.position).normalized;
                break;
        }

        RotateTowardsDirection(targetDirection);
        m_rb.velocity = m_missileParent.forward * m_speed;
    }

    private void RotateTowardsDirection(Vector3 direction)
    {
        Quaternion desiredRotation = Quaternion.LookRotation(direction);
        m_missileParent.rotation = Quaternion.RotateTowards(m_missileParent.rotation, desiredRotation, m_rotationSpeed * Time.deltaTime);
    }
    public void FireToTarget(Transform target)
    {
        this.m_target = target;
        gameObject.SetActive(true);

    }

    void IJob.Execute()
    {
        m_explosionEffect.SetActive(true);
        m_explosionEffect.transform.parent = null;
        m_missileBody.SetActive(false);
        Collider[] affectedColliders = Physics.OverlapSphere(transform.position, ExplosionEffectRadius);
        Transform colliderRoot;
        foreach (Collider collider in affectedColliders)
        {
            colliderRoot = collider.transform.root;
            if (colliderRoot.GetComponent<IExplosible>() != null)
            {
                colliderRoot.GetComponent<IExplosible>().TakeDamage(15000, 1, transform.position);
            }

            if (colliderRoot.GetComponent<IDestroyable>() != null)
            {
                colliderRoot.GetComponent<IDestroyable>().GetHit(5000, 1);
            }
        }
        Destroy(gameObject);
    }

    void IExplosible.Explode()
    {
        Destroy(gameObject);
    }

    void IExplosible.TakeDamage(int damage, float armorPiercing, UnityEngine.Vector3 explosionLocation)
    {
        ((IExplosible)this).Explode();
    }
    private void OnCollisionEnter(Collision collision)
    {
        ((IJob)this).Execute();
    }
}

