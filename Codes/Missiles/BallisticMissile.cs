using Unity.Jobs;
using UnityEngine;

public class BallisticMissile : MonoBehaviour,IJob, IExplosible
{
    private float m_rotateSpeed = 80;
    private float m_startTime;
    private float m_timeSinceLaunch;
    private float m_timeBeforeSecondPhase = 1.5f;
    private float m_timeBeforeThirdPhase = 10f;
    private GameObject m_target;
    private Rigidbody m_rb;
    private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip m_initialSound;
    [SerializeField]
    private AudioClip m_sound;
    private float m_terminalPhaseThrust = 18000;
    private bool m_isTerminalPhaseConditionMet = true;
    private float lerpTime = 0f;
    private Vector3 lerpStartPosition;
    private float lerpDuration;  // completion duration of lerp in seconds
    private bool m_isSecondMusicPlayed = false;

    private GameObject m_VFXOfSecondPhase;

    //Collision related logics
    private const int ExplosionEffectRadius = 36;
    private GameObject m_explosionEffect;
    private GameObject m_missileBody;




    void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.clip = m_initialSound;
        m_rb = GetComponent<Rigidbody>();
        m_explosionEffect = transform.Find("Explosion").gameObject;
        m_missileBody = transform.Find("Ballistic").gameObject;
        //MissileControllingsystem.Instance.RegisterBallisticMissile(gameObject);
        gameObject.SetActive(false);
        m_VFXOfSecondPhase = transform.Find("VFX").gameObject;
        m_rb.mass = 1000;
    }
    void Start()
    {
        m_VFXOfSecondPhase.SetActive(false);
        Debug.Log($"Name of the target is: {m_target.name}");
    }
    private void OnEnable()
    {
        m_startTime = Time.time;
        m_rb.velocity = Vector3.up * 45;

        lerpDuration = 5f;
        m_audioSource.Play();
    }
    void Update()
    {
        m_timeSinceLaunch = Time.time - m_startTime;
    }

    private void FixedUpdate()
    {

        MissileMovement();
    }
    

    private void InitialeLaunchingPhase()
    {
        m_rb.AddForce(transform.forward * 12000);
    }

    private void SecondPhase()
    {
        if(m_VFXOfSecondPhase.activeSelf == false)
        {
            m_VFXOfSecondPhase.SetActive(true);
        }
        if(!m_isSecondMusicPlayed)
        {
            m_audioSource.clip = m_sound;
            m_audioSource.Play();
            m_isSecondMusicPlayed = true;
        }
        m_rb.AddForce(transform.forward * 71000);
    }


    private void TerminalPhase()
    {
        if (!m_target)
        {
            Destroy(gameObject);
        }
        // Calculate the direction to the target
        Vector3 directionToTarget = (m_target.transform.position - transform.position).normalized;

        //Lerp Activation Condition
        if (transform.position.y <= 5000)
        {
            m_isTerminalPhaseConditionMet = false;
            lerpStartPosition = this.transform.position;
        }


        // Apply force in the direction of the target
        m_rb.AddForce(directionToTarget * m_terminalPhaseThrust);

        // Smoothly rotate the missile towards the desired rotation
        Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);

        m_rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, desiredRotation, m_rotateSpeed * Time.fixedDeltaTime));

        //Debug.Log($"Terminal Phase - velocity is: {m_rb.velocity}");
    }

    private void ThirdPhase()
    {
        Vector3 directionToTarget = m_target.transform.position - transform.position;

        // Calculate the desired rotation to point at the target
        Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);

        //  Rotate the missile towards the desired rotation
        m_rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, desiredRotation, .4f * Time.fixedDeltaTime));
    }

    //it should check its sourding after the missile explodes to see what objects are affected
    //Since this might be a CPU costly job I leverages multi-threading the prevent any lag on the main game thread

    void IExplosible.Explode()
    {
        Debug.Log("Explode");
        m_explosionEffect.SetActive(true);
        m_explosionEffect.transform.parent = null;
        Destroy(gameObject);
    }

    void IExplosible.TakeDamage(int damage, float armorPiercing, UnityEngine.Vector3 explosionLocation)
    {
        ((IExplosible)this).Explode();
    }


    void IJob.Execute()
    {
        m_explosionEffect.SetActive(true);
        m_explosionEffect.transform.parent = null;
        m_missileBody.SetActive(false);


        Collider[] affectedColliders = Physics.OverlapSphere(transform.position, ExplosionEffectRadius);
        Debug.Log($"affectedColliders.count : {affectedColliders.Length}");
        Transform colliderRoot;

        foreach (Collider collider in affectedColliders)
        {
            colliderRoot = collider.transform.root;
            Debug.Log($"name of the colliderRoot is: {colliderRoot.name}");
            Debug.Log($"name of the collider is: {colliderRoot.name}");
            if (colliderRoot.GetComponent<IExplosible>() != null)
            {
                Debug.Log($"Name of the collider is: {colliderRoot.name}");
                colliderRoot.GetComponent<IExplosible>().TakeDamage(15000, 1, transform.position);
            }

            if (colliderRoot.GetComponent<IDestroyable>() != null)
            {
                colliderRoot.GetComponent<IDestroyable>().GetHit(5000, 1);
            }
        }

        Destroy(gameObject);

    }

    private void OnCollisionEnter(Collision collision)
    {
        ((IJob)this).Execute();  
    }


    private void MissileMovement()
    {
        if (m_timeSinceLaunch < m_timeBeforeSecondPhase)
        {
            //Debug.Log($"First Phase time: {m_timeSinceLaunch}");
            InitialeLaunchingPhase();
        }
        else if (m_timeSinceLaunch < m_timeBeforeThirdPhase)
        { 
            SecondPhase();
        }
        else if (m_isTerminalPhaseConditionMet && m_rb.velocity.y <= 45)
        {
            TerminalPhase();
        }
        else if (m_rb.velocity.y <= 0 && transform.position.y <= 5000) 
        {
            LerpToTarget();
        }
        else
        {
            //Debug.Log($"Third Phase time: {m_timeSinceLaunch}");
            ThirdPhase();
        }
    }

    private void LerpToTarget()
    {
        if (m_target)
        {
            lerpTime += Time.fixedDeltaTime;


            float lerpProgress = lerpTime / lerpDuration;


            transform.position = Vector3.Lerp(lerpStartPosition, m_target.transform.position, lerpProgress);
        }
    }

    public void FireToTarget(GameObject target)
    {
        m_target = target;
        gameObject.SetActive(true);
        
    }

}

