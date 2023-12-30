using UnityEngine;

public class Radar : MonoBehaviour
{
    [SerializeField]
    private GameObject m_missileControllingSystem;

    private AirDefenceSystem m_airDefenceSystem;
    private void Awake()
    {
        m_airDefenceSystem = m_missileControllingSystem.GetComponent<AirDefenceSystem>();
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Radar is detecting: " + other.name);
        if (other.gameObject.layer == LayerMask.NameToLayer("BallisticMissile") ||
            other.gameObject.layer == LayerMask.NameToLayer("Aircraft") ||
            other.gameObject.layer == LayerMask.NameToLayer("CruiseMissile"))
        {
            m_airDefenceSystem.GetUpdateFromRadar(other.gameObject);
        }
        //However, the game uses layering the avoid any extra overhead of colliding radar collider with other objects that are not missiles or aircrafts.
        //But, in case, if the detected object is not a threat, do nothing
        else
        return;

    }
}
