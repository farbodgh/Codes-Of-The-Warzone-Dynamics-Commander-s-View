using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelContainer : MonoBehaviour,IExplosible
{
    private int m_hitPoints = 100;

    private GameObject m_explodedVersion;
    private GameObject m_originalVersion;

    private void Awake()
    {
        m_explodedVersion = transform.Find("Explosion").gameObject;
        m_originalVersion = transform.Find("Regular").gameObject;
    }


    void IExplosible.Explode()
    {
        m_originalVersion.SetActive(false);
        m_explodedVersion.SetActive(true);
        Destroy(m_originalVersion);
    }

    void IExplosible.TakeDamage(int damage, float armorPiercing, Vector3 explosionLocation)
    {
        m_hitPoints -= damage;
        if (m_hitPoints <= 0)
        {
            ((IExplosible)this).Explode();
        }
    }
}
