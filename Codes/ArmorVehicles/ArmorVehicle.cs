using UnityEngine;

public abstract class ArmorVehicle : MonoBehaviour, IExplosible
{
    //armor is the value that is used to show armor thoughness of the vehicle
    protected float armor;
    protected int hitPoint;

    private GameObject m_explodedVersion;
    private GameObject m_originalVersion;

    private Vector3 m_explosionLocation;
    private void Awake()
    {
        m_explodedVersion = transform.Find("Explosion").gameObject;
        m_originalVersion = transform.Find("Regular").gameObject;
    }
    private void Start()
    {
    }
    void IExplosible.Explode()
    {
        m_originalVersion.SetActive(false);
        //the script in the first child of the Explosion gameobject is the one that has the explosion script on it we should give it the position of the explosion
        m_explodedVersion.transform.GetChild(0).GetComponent<Explosion>().m_explosionPosition = m_explosionLocation;
        m_explodedVersion.SetActive(true);
    }

    protected virtual void Update()
    {

    }

    void IExplosible.TakeDamage(int damage, float armorPiercing, Vector3 explosionLocation)
    {
        //if the armor piercing value is greater than the armor value, the damage is applied directly to the hitpoint
        //if the armor piercing value is less than the armor value, there is no damage applied to the hitpoint
        if (armorPiercing > armor)
        {
            hitPoint -= damage;
        }


        if(hitPoint <= 0)
        {
            m_explosionLocation = explosionLocation;
            ((IExplosible)this).Explode();
        }
    }
}
