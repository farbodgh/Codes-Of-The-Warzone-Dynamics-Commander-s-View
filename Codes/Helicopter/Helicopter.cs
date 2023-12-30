using UnityEngine;
using DG.Tweening;

public class Helicopter : MonoBehaviour,IExplosible
{
    //Refrence
    private Rigidbody m_rigidbidy;
    private GameObject m_explodedVersion;
    private GameObject m_originalVersion;
    [SerializeField]
    private Collider m_collider;

    [Header("Rotor Components")]
    public RotorRotation mainRotor;
    public RotorRotation tailRotor;

    [Header("Engine Settings")]
    [SerializeField] private float m_engineStartSpeed;
    private float m_enginePower;
    private float m_timeSinceEngineStart;
    private float m_engineWarmupTime = 7;
    private bool m_isEngineReducingForce = false;
    private bool m_isEngineOff;
    public float enginePower
    {
        get { return m_enginePower; }
        set { mainRotor.rotorSpeed = Mathf.Ceil(value * 250f); tailRotor.rotorSpeed = Mathf.Ceil(value * 500f); m_enginePower = value; }
    }

    [Header("Movement Settings")]
    [SerializeField] private float m_engineLift = 0.0075f;
    [SerializeField] private float m_effectiveHeight;
    [SerializeField] private float m_forwardForce;
    [SerializeField] private float m_backwardForce;
    [SerializeField] private float m_turnForce;
    private float m_turnForceHelper = 1.5f;
    private float m_turning = 0f;
    private Vector2 m_movment = Vector2.zero;


    [Header("Tilt Settings")]
    [SerializeField] private float m_forwardTiltForce;
    [SerializeField] private float m_turnTiltForce;
    private Vector2 m_tilting = Vector2.zero;

    [Header("Ground Detection")]
    private LayerMask m_groundLayer;
    [SerializeField] private GameObject m_raycastPos;
    private float m_distaceToGround;
    private bool m_isGrounded = true;

    [Header("Audio")]
    [SerializeField] private AudioClip m_interiorClip;
    [SerializeField] private AudioClip m_exteriorClip;
    [SerializeField] private AudioClip m_engineStartClip;
    private AudioSource m_helicopterSound;
    private bool m_isEngineStartSoundPlayed = true;
    private bool m_isEngineInteriorSoundPlayed = true;
    private bool m_isEngineExteriorSoundPlayed = false;


    [Header("Cameras")]
    [SerializeField] public Camera interiorCamera;
    [SerializeField] public Camera exteriorCamera;
    public bool isInteriorView { private set; get; } = false;
    public bool isExteriorView { private set; get; } = true;
    private bool m_switchCamera = false;

    private int m_hitPoint = 600;
    private float m_armor = .2f;

    Vector3 m_explosionLocation;

    private void Awake()
    {
        //Camera Controller
        interiorCamera.GetComponent<Camera>().enabled = false;
        exteriorCamera.GetComponent<Camera>().enabled = true;
        //Helo controller
        m_rigidbidy = GetComponent<Rigidbody>();
        m_isEngineOff = true;
        m_helicopterSound = GetComponent<AudioSource>();
        m_helicopterSound.Stop();
        m_groundLayer = LayerMask.GetMask("Ground");
        m_explodedVersion = transform.Find("Explosion").gameObject;
        m_originalVersion = transform.Find("Regular").gameObject;

    }
    private void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.F))
        {
            ExitTheHelo();
        }

        SoundController();
        CameraController();
        DetectGround();
        ProcessInput();
        HandleEngine();
    }

    private void FixedUpdate()
    {
        Hover();
        Movements();
        Tilt();
    }

    private void SoundController()
    {

        if (!m_isEngineStartSoundPlayed && m_isGrounded)
        {
            Debug.Log("Engine start sound is activated");
            m_helicopterSound.clip = m_engineStartClip;
            m_helicopterSound.Play();
            m_helicopterSound.loop = false;
            m_isEngineStartSoundPlayed = true;
            return;
        }
        //ensures the sound does not change when the engine is in its warmpup phase;
        if (m_helicopterSound.isPlaying && !(m_timeSinceEngineStart + m_engineWarmupTime < Time.time)) { return; }

        // Check if the engine interior sound hasn't been played, the engine is on, and it's in interior view
        if (!m_isEngineInteriorSoundPlayed && !m_isEngineOff && isInteriorView && !isExteriorView)
        {
            Debug.Log("Engine interior sound is activated");
            m_helicopterSound.loop = true;
            m_helicopterSound.clip = m_interiorClip;
            m_helicopterSound.Play();
            m_isEngineInteriorSoundPlayed = true;
            return;
        }

        // Check if the engine exterior sound hasn't been played, the engine is on, and it's in exterior view
        if (!m_isEngineExteriorSoundPlayed && !m_isEngineOff && isExteriorView && !isInteriorView)
        {
            Debug.Log("Engine Exterior Sound is activated");
            m_helicopterSound.loop = true;
            m_helicopterSound.clip = m_exteriorClip;
            m_helicopterSound.Play();
            m_isEngineExteriorSoundPlayed = true;
            return;
        }

        return;
    }

    // Process user input for engine control and movement
    private void ProcessInput()
    {
        // Check if the engine has been shut down; if so, do not apply any forces to the helicopter.
        if (m_isEngineOff) return;

        if (m_isGrounded)
        {
            if (Input.GetKey(KeyCode.C))
            {
                if (enginePower > 0)
                {
                    // Gradually reduce engine power when landing to avoid a sudden drop.
                    enginePower = Mathf.Lerp(enginePower, 0f, 0.5f * Time.fixedDeltaTime);
                }
                else
                {
                    enginePower = 1;
                }
            }
        }
        else
        {
            m_movment.x = Input.GetAxis("Horizontal");
            m_movment.y = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.C))
            {
                if (enginePower > 5f)
                {
                    // Gradually reduce engine power when ascending to maintain control.
                    enginePower = Mathf.Lerp(enginePower, 4f, 1.5f * Time.fixedDeltaTime);
                }
                else
                {
                    enginePower = 4f;
                }
                m_isEngineReducingForce = true;
                Debug.Log("engine power when descending: " + enginePower + "Velocity" + m_rigidbidy.velocity);
            }
        }

        if (Input.GetAxis("Throttle") > 0f)
        {
            if (m_timeSinceEngineStart + m_engineWarmupTime < Time.time)
            {
                enginePower += m_engineLift;
            }

            Debug.Log("engine power when throttle: " + enginePower);
        }

        // Ensure that engine power does not drop too low when ascending and not on the ground.
        if (enginePower < 56 && Input.GetAxis("Vertical") > 0f && !m_isGrounded)
        {
            // Check if the engine is not actively reducing force (used to prevent conflicting adjustments).
            if (!m_isEngineReducingForce)
            {
                // Gradually increase engine power when ascending, maintaining control.
                enginePower = Mathf.Lerp(enginePower, 57.5f, 0.5f * Time.fixedDeltaTime);
            }
            else
            {
                m_isEngineReducingForce = false;
            }
        }

    }


    // Detect whether the helicopter is grounded
    private void DetectGround()
    {
        // Create a raycast to detect the ground below the helicopter.
        RaycastHit hit;
        Vector3 direction = transform.TransformDirection(Vector3.down);
        Vector3 raycastOrigin = m_raycastPos.transform.position;

        // Create a ray with the defined origin and direction.
        Ray ray = new Ray(raycastOrigin, direction);

        // Perform the raycast and check for collisions with objects on the "groundLayer" layer within a range of 1000 units.
        if (Physics.Raycast(ray, out hit, 1000, m_groundLayer))
            if (Physics.Raycast(ray, out hit, 1000, m_groundLayer))
            {
                m_distaceToGround = hit.distance;
                if (m_distaceToGround < 2)
                {
                    m_isGrounded = true;
                }
                else
                {
                    m_isGrounded = false;
                }
            }

    }

    // Start or stop the helicopter's engine
    private void HandleEngine()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartEngine();
            m_isEngineStartSoundPlayed = false;
            Debug.Log("engine started : " + enginePower);

        }
        else if (Input.GetKeyDown(KeyCode.Z) && m_isGrounded)
        {
            StopEngine();
            Debug.Log("engine stopped : " + enginePower);
        }

    }

    // Apply upward force to simulate hovering
    private void Hover()
    {
        if (!m_isEngineOff)
        {
            // Calculate the upward force based on the helicopter's position relative to the effective height.
            // This force decreases as the helicopter gets closer to the effective height.
            float upForce = 1 - Mathf.Clamp(m_rigidbidy.transform.position.y / m_effectiveHeight, 0, 1);

            // Interpolate (lerp) between 0 and the engine power based on the upForce and time step.
            upForce = Mathf.Lerp(0, enginePower, upForce * Time.fixedDeltaTime * 50) * m_rigidbidy.mass;

            // Apply the calculated upward force to the helicopter's rigidbody, lifting it.
            m_rigidbidy.AddRelativeForce(Vector3.up * upForce);
        }
    }

    // Handle forward, backward, and turning movements
    private void Movements()
    {
        if (Input.GetAxis("Vertical") > 0)
        {
            // The force applied is proportional to the player's input, forward force, and the helicopter's mass.
            m_rigidbidy.AddRelativeForce(Vector3.forward * Mathf.Max(0f, m_movment.y * m_forwardForce * m_rigidbidy.mass));
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            // The force applied is proportional to the player's input, backward force, and the helicopter's mass.
            m_rigidbidy.AddRelativeForce(Vector3.back * Mathf.Max(0f, -m_movment.y * m_backwardForce * m_rigidbidy.mass));
        }

        float turn = m_turnForce * Mathf.Lerp(m_movment.x, m_movment.x * (m_turnForceHelper - Mathf.Abs(m_movment.y)), Mathf.Max(0f, m_movment.y));
        // Smoothly adjust the current turning value to the calculated turn value over time.
        m_turning = Mathf.Lerp(m_turning, turn, Time.fixedDeltaTime * m_turnForce);
        // Apply torque (rotation) to the helicopter based on the turning value, mass, and the y-axis.
        m_rigidbidy.AddRelativeTorque(0f, m_turning * m_rigidbidy.mass, 0f);
    }

    // Tilt the helicopter based on user input
    private void Tilt()
    {
        // Calculate and interpolate (smoothly adjust) the tilt along the y-axis based on the forward input.
        m_tilting.y = Mathf.Lerp(m_tilting.y, m_movment.y * m_forwardTiltForce, Time.deltaTime);
        // Calculate and interpolate (smoothly adjust) the tilt along the x-axis based on the turning input.
        m_tilting.x = Mathf.Lerp(m_tilting.x, m_movment.x * m_turnTiltForce, Time.deltaTime);

        // Update the local rotation of the helicopter's transform to apply the calculated tilt.
        // This helps simulate the helicopter's tilting motion as it moves forward or turns.
        m_rigidbidy.transform.localRotation = Quaternion.Euler(m_tilting.y, m_rigidbidy.transform.localEulerAngles.y, -m_tilting.x);
    }


    private void StartEngine()
    {
        if (m_isEngineOff)
        {
            DOTween.To(Starting, 0f, 8, m_engineStartSpeed);
            m_isEngineOff = false;
            m_timeSinceEngineStart = Time.time;
        }
    }
    private void Starting(float value)
    {
        enginePower = value;
    }
    private void StopEngine()
    {
        m_isEngineOff = true;
        if (m_helicopterSound.isPlaying)
        {
            m_helicopterSound.Stop();
        }
        DOTween.To(Stopping, enginePower, 0f, m_engineStartSpeed);
    }
    private void Stopping(float value)
    {
        enginePower = value;
    }
    private void CameraController()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_switchCamera = !m_switchCamera;

            // Toggle between cameras based on m_switchCamera.
            if (m_switchCamera)
            {
                interiorCamera.enabled = true;
                exteriorCamera.enabled = false;
                isInteriorView = true;
                isExteriorView = false;
                m_isEngineInteriorSoundPlayed = false;

            }
            else
            {
                interiorCamera.enabled = false;
                exteriorCamera.enabled = true;
                isInteriorView = false;
                isExteriorView = true;
                m_isEngineExteriorSoundPlayed = false;

            }
        }
    }
 
    void IExplosible.Explode()
    {
        
        m_originalVersion.SetActive(false);
        m_originalVersion.transform.parent = null;
        Destroy(m_originalVersion);
        Destroy(m_rigidbidy);
        Destroy(m_collider);
        //the script in the first child of the Explosion gameobject is the one that has the explosion script on it we should give it the position of the explosion
        m_explodedVersion.transform.GetChild(0).GetComponent<Explosion>().m_explosionPosition = m_explosionLocation;
        m_explodedVersion.SetActive(true);
        Destroy(gameObject,2);
    }

    void IExplosible.TakeDamage(int damage, float armorPiercing, Vector3 explosionLocation)
    {
        //If the armor piercing is more than armor, the damage is applied directly to the hitpoint
        //If the armor piercing is less than the armor, there is no damage applied to the hitpoint
        if(armorPiercing > m_armor)
        {
            m_hitPoint -= damage;
        }

        //if the hitpoint is less than or equal to 0, the vehicle is destroyed
        if(m_hitPoint <= 0)
        {
            GameManager.Instance.PlayerDie();
            ((IExplosible)this).Explode();
            m_explosionLocation = explosionLocation;
           
        }

    }

    private void ExitTheHelo()
    {
        if (!m_isGrounded) 
        {
            return;
        }
        StopEngine();
        GameManager.Instance.ExitTheHelo();
    }
}

