using UnityEngine;

interface IExplosible
{
    public void Explode();
    public void TakeDamage(int damage, float armorPiercing, Vector3 explosionLocation);

}