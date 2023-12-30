using UnityEngine;

public class HeloMiniGun : MonoBehaviour
{
    //Refrences
    private Rigidbody m_rb;
    private Collider m_collider;
    private AudioSource m_audioSource;
    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();
        m_audioSource = GetComponent<AudioSource>();
        m_rb.velocity = transform.forward * 2;
        m_rb.AddForce(transform.forward * 5000);   
    }

    private void OnCollisionEnter(Collision collision)
    {
        m_rb.AddExplosionForce(200, transform.position, 2);
        m_audioSource.Play();
        var destroyable = collision.transform.GetComponent<IDestroyable>();
        if (destroyable != null)
        {
            destroyable.GetHit(100, 1);
        }
        var explosible = collision.transform.GetComponent<IExplosible>();
        if (explosible != null)
        {
            explosible.TakeDamage(100, .6f, transform.position);
        }
        Destroy(gameObject, 1);
        m_collider.enabled = false;
    }
}
