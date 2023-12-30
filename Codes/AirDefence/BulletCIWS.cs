using UnityEngine;

public class BulletCIWS : MonoBehaviour
{

    private Rigidbody m_rb;
    //The following used for the bullet to return to its initial position after being reactivated in its 
    private Vector3 m_initialePostion;


    public Vector3 targetPosition { private get; set; }
    private float m_timeSinceShoot;
    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        m_initialePostion = transform.position;
        m_timeSinceShoot = Time.time;
    }

    private void Update()
    {
        //Debug.Log($"Bullet position: {transform.position}");
        //Debug.Log($"Bullet velocity: {m_rb.velocity}");
        //Debug.Log($"Bullet angular velocity: {m_rb.angularVelocity}");
        //Debug.Log($"Bullet rotation: {transform.rotation}");
        if(m_timeSinceShoot + 3 < Time.time)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Vector3 shootDirection = (targetPosition - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(shootDirection);
        //m_rb.AddForce(shootDirection * 4096);
        m_rb.velocity = shootDirection * 1024;
        m_timeSinceShoot = Time.time;
    }

    private void OnDisable()
    {
        m_rb.useGravity = true;
        transform.position = m_initialePostion;
        m_rb.angularVelocity = Vector3.zero;
        m_rb.velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject rootObject = collision.gameObject.transform.root.gameObject;
        var explosible = rootObject.GetComponent<IExplosible>();

        if (explosible != null) 
        {
            explosible.TakeDamage(50,.6f, transform.position);
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("CruiseMissile"))
        {
            
        }


    }
}
