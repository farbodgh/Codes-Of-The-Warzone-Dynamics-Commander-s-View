using System.Collections;
using UnityEngine;


public class ABM : MonoBehaviour
{
    private Rigidbody m_rb;
    private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip m_soundEffect;
    [SerializeField]
    public GameObject m_target { private get; set; }
    private float m_startTime;
    private float m_timeSinceLaunch;
    private float m_timeBeforeTerminalPhase = 1f;
    private float m_timeBeforeLepring = 5f;
    [SerializeField]
    private float m_terminalPhaseThrust = 24000;
    private float m_lerpTime = 0f;
    private bool m_isLerping;
    private Vector3 m_lerpStartPosition;
    private float m_lerpDuration = 5f;
    //The following variable is used to store the initial position of the missile, used for object pooling
    public Vector3 m_spawnPoint { private get; set; }

    [SerializeField]
    private GameObject m_explosionEffect;

    // Start is called before the first frame update
    private void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.clip = m_soundEffect;
        m_rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        //Debug.Log($"Missile: {gameObject.name} is launched at: {Time.time}");
        m_lerpDuration = Random.Range(2.5f, 2.8f);
        m_audioSource.Play();
        m_startTime = Time.time;
        m_rb.mass = 150;
        m_rb.velocity = Vector3.up * 90;
        IsUsingLerp();
    }
    private void OnDisable()
    {
        m_timeSinceLaunch = 0;
        m_target = null;
        m_isLerping = false;
        m_lerpTime = 0;
        m_rb.velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(-90, 0, 0);
        transform.position = m_spawnPoint;
        m_rb.angularVelocity = Vector3.zero;
        if (m_audioSource.isPlaying)
        {
            m_audioSource.Stop();
        }

    }
    // Update is called once per frame
    void Update()
    {
        m_timeSinceLaunch = Time.time - m_startTime;
    }

    private void FixedUpdate()
    {
        Movement();
        CheckDetonationPhase();
    }

    private void Movement()
    {
        if (m_timeSinceLaunch <= m_timeBeforeTerminalPhase)
        {
            InitialPhase();
        }

        if (!m_isLerping && m_timeSinceLaunch > m_timeBeforeTerminalPhase)
        {
            TerminalPhase();
        }

        if (m_isLerping)
        {
            LerpMovement();
        }
    }

    private void InitialPhase()
    {
        if (m_target)
        {
            Vector3 directionToTarget = m_target.transform.position - transform.position;

            // Calculate the desired rotation to point at the target
            Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);

            // Rotate the missile towards the desired rotation
            m_rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, desiredRotation, 45 * Time.fixedDeltaTime));
        }
    }

    private void TerminalPhase()
    {
        if (m_target)
        {
            Vector3 directionToTarget = m_target.transform.position - transform.position;

            // Calculate the desired rotation to point at the target
            Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);
            float scalerFactor = Mathf.Clamp01(directionToTarget.magnitude / 5000);
            directionToTarget.y *= scalerFactor;
            // Rotate the missile towards the desired rotation
            m_rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, desiredRotation, 120 * Time.fixedDeltaTime));

            // Apply force in the direction of the target
            m_rb.AddForce(transform.forward * m_terminalPhaseThrust);
        }

    }

    private void CheckDetonationPhase()
    {
        if (m_timeSinceLaunch > 12.5f)
        {
            gameObject.SetActive(false);
        }
        if (m_target == null || m_target.Equals(null))
        {
            gameObject.SetActive(false);

        }
        if (m_target != null && Vector3.Distance(transform.position, m_target.transform.position) < 50)
        {
            //Debug.Log($"Missile: {gameObject.name} is detonated at: {Time.time} and explodes missile {m_target.name}");
            var targetExplosible = m_target.GetComponent<IExplosible>();
            if (targetExplosible != null)
            {
                targetExplosible.Explode();
            }
            m_target = null;
            Instantiate(m_explosionEffect, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }


    }


    private void IsUsingLerp()
    {

        StartCoroutine(DecideLerpAfterSeconds(m_timeBeforeLepring));

    }

    private void LerpMovement()
    {
        m_lerpTime += Time.deltaTime;
        float lerpProgress = m_lerpTime / m_lerpDuration;

        if(m_target)
        transform.position = Vector3.Lerp(m_lerpStartPosition, m_target.transform.position, lerpProgress);

    }

    // In the final product the ABM missiles are not hitting their target every times
    private IEnumerator DecideLerpAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        //if(Random.value <= 0.4f)
        //{
        //    m_isLerping = true;
        //}
        //else
        //{
        //    m_isLerping = false;
        //}
        m_isLerping = true;
        m_lerpStartPosition = this.transform.position;
    }
}
