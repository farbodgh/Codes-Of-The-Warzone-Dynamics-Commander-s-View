using System.Collections.Generic;
using UnityEngine;

public class AirDefenceSystem : MonoBehaviour
{

    //the bool value indicates whether the missile is intercepted or not
    private Queue<GameObject> m_ballisticMissiles = new Queue<GameObject>();
    private Queue<GameObject> m_CruiseMissiles = new Queue<GameObject>();
    private Queue<GameObject> m_enemyAirwings = new Queue<GameObject>();
    //The bool value indicates whether the ABM system is available or not
    private List<(GameObject, bool)> m_CIWSs = new List<(GameObject, bool)>();
    //The bool value indicates whether the CIWS system is available or not
    private List<(GameObject, bool)> m_ABMSystems = new List<(GameObject, bool)>();

    [SerializeField]
    private List<GameObject> m_allCIWS = new List<GameObject>();
    [SerializeField]
    private List<GameObject> m_allABMSystems = new List<GameObject>();

    [SerializeField]
    private GameObject m_radarStation;
    //private float m_timeSinceLastUpdate;
    //private float m_periodBeforeNextUpdate = 0.5f;
    // Start is called before the first frame update
    private void Awake()
    {
        for (int i = 0; i < m_allCIWS.Count; i++)
        {
            m_CIWSs.Add(new(m_allCIWS[i], true));
        }
        for (int i = 0; i < m_allABMSystems.Count; i++)
        {
            m_ABMSystems.Add(new(m_allABMSystems[i], true));
        }
    }
    void Update()
    {

        //Check if the ABM and CIWS systems are available each 0.5 seconds
        CheckWhichSystemsAreAvailable();
        DecisionMaking();
    }




     public void GetUpdateFromRadar(GameObject detectedObject)
    {
        //Debug.Log("MissileControllingSystem: " + detectedObject.name + " is detected");
        if(detectedObject.gameObject.layer == LayerMask.NameToLayer("BallisticMissile"))
        {
            m_ballisticMissiles.Enqueue(detectedObject);
            return;
        }
        if(detectedObject.gameObject.layer == LayerMask.NameToLayer("CruiseMissile"))
        {
            m_CruiseMissiles.Enqueue(detectedObject);
            return;
        }
        if(detectedObject.gameObject.layer == LayerMask.NameToLayer("Aircraft"))
        {
            m_enemyAirwings.Enqueue(detectedObject);
            return;
        }
        if(detectedObject.gameObject.layer == LayerMask.NameToLayer("Helicopter"))
        {
            m_enemyAirwings.Enqueue(detectedObject);
            return;
        }
    }

    void DecisionMaking()
    {
        ABMDecisionMaking();
        CIWSDecisionMaking();
        AirWingsDecisionMaking();
    }

    private void ABMDecisionMaking()
    {
        if (m_ballisticMissiles.Count > 0)
        {
            //Finding the closest ABM system to the ballistic missile and launch a missile to intercept it
            float distanceToTarget = float.PositiveInfinity;
            float distanceToCurrentABMSystem = 0;
            GameObject firstTarget = m_ballisticMissiles.Peek();
            GameObject closestABMSystem = null;
            int indexOfClosestABMSystem = 0;
            for (int i = 0; i < m_ABMSystems.Count; i++)
            {
                distanceToCurrentABMSystem = Vector3.Distance(m_ABMSystems[i].Item1.transform.position, firstTarget.transform.position);
                if (m_ABMSystems[i].Item2)
                {
                    if (distanceToTarget > distanceToCurrentABMSystem)
                    {
                        distanceToTarget = distanceToCurrentABMSystem;
                        closestABMSystem = m_ABMSystems[i].Item1;
                        indexOfClosestABMSystem = i;
                    }
                }
            }
            if (closestABMSystem != null)
            {
                closestABMSystem.GetComponent<ABMMissileLauncher>().GetTarget(m_ballisticMissiles.Dequeue());
                m_ABMSystems[indexOfClosestABMSystem] = new(closestABMSystem, false);
            }
        }
    }
    
    private void CIWSDecisionMaking()
    {
        if (m_CruiseMissiles.Count > 0)
        {
            float distanceToTarget = float.PositiveInfinity;
            float distanceToCurrentCIWSSystem = 0;
            GameObject firstTarget = m_CruiseMissiles.Peek();
            GameObject closestCIWS = null;
            int indexOfClosestCIWS = 0;
            for (int i = 0; i < m_CIWSs.Count; i++)
            {
                if (m_CIWSs[i].Item1 == null)
                {
                    m_CIWSs.Remove(m_CIWSs[i]);
                    continue;
                }
                if(firstTarget == null)
                {
                    m_CruiseMissiles.Dequeue();
                    continue;
                }
                distanceToCurrentCIWSSystem = Vector3.Distance(m_CIWSs[i].Item1.transform.position, firstTarget.transform.position);
                if (m_CIWSs[i].Item2)
                {
                    if (distanceToTarget > distanceToCurrentCIWSSystem)
                    {
                        distanceToTarget = distanceToCurrentCIWSSystem;
                        closestCIWS = m_CIWSs[i].Item1;
                        indexOfClosestCIWS = i;
                    }
                }
            }
            if (closestCIWS != null)
            {
                closestCIWS.GetComponent<CIWS>().GetNewTarget(m_CruiseMissiles.Dequeue());
                m_CIWSs[indexOfClosestCIWS] = new(closestCIWS, false);
            }
        }
    }

    private void AirWingsDecisionMaking()
    {
        if(m_enemyAirwings.Count > 0 && m_enemyAirwings.Peek().gameObject.layer == LayerMask.NameToLayer("Aircraft"))
        {
            float distanceToTarget = float.PositiveInfinity;
            float distanceToCurrentABMSystem = 0;
            GameObject firstTarget = m_enemyAirwings.Peek();
            GameObject closestABMSystem = null;
            int indexOfClosestABMSystem = 0;
            for (int i = 0; i < m_ABMSystems.Count; i++)
            {
                distanceToCurrentABMSystem = Vector3.Distance(m_ABMSystems[i].Item1.transform.position, firstTarget.transform.position);
                if (m_ABMSystems[i].Item2)
                {
                    if (distanceToTarget > distanceToCurrentABMSystem)
                    {
                        distanceToTarget = distanceToCurrentABMSystem;
                        closestABMSystem = m_ABMSystems[i].Item1;
                        indexOfClosestABMSystem = i;
                    }
                }
            }
            if (closestABMSystem != null && m_enemyAirwings.Peek().gameObject.layer == LayerMask.NameToLayer("Aircraft"))
            {
                closestABMSystem.GetComponent<ABMMissileLauncher>().GetTarget(m_enemyAirwings.Dequeue());
                m_ABMSystems[indexOfClosestABMSystem] = new(closestABMSystem, false);
            }
        }

        if(m_enemyAirwings.Count > 0)
        {
            float distanceToTarget = float.PositiveInfinity;
            float distanceToCurrentCIWSSystem = 0;
            GameObject firstTarget = m_enemyAirwings.Peek();
            GameObject closestCIWS = null;
            int closestCIWSIndex = 0;
            for (int i = 0; i < m_CIWSs.Count; i++)
            {
                distanceToCurrentCIWSSystem = Vector3.Distance(m_CIWSs[i].Item1.transform.position, firstTarget.transform.position);
                if (m_CIWSs[i].Item2)
                {
                    if (distanceToTarget > distanceToCurrentCIWSSystem)
                    {
                        distanceToTarget = distanceToCurrentCIWSSystem;
                        closestCIWS = m_CIWSs[i].Item1;
                        closestCIWSIndex = i;
                    }
                }
            }
            if (closestCIWS != null && m_enemyAirwings.Peek().gameObject.layer == LayerMask.NameToLayer("Helicopter"))
            {
                closestCIWS.GetComponent<CIWS>().GetNewTarget(m_enemyAirwings.Dequeue());
                m_CIWSs[closestCIWSIndex] = new(closestCIWS, false);
            }
        }
        
    }
    void CheckWhichSystemsAreAvailable()
    {
        //Removing the the anti-aircraft system if it is destroyed
        //For the case that the system is destroyed by the enemy
        for (int i = 0; i< m_ABMSystems.Count; i++)
        {
            if (m_ABMSystems[i].Item1 == null)
            {
                m_ABMSystems.Remove(m_ABMSystems[i]);
                continue;
            }
            //Debug.Log($"{m_ABMSystems[i].Item1.name} is available: {m_ABMSystems[i].Item2}");
            m_ABMSystems[i] = new(m_ABMSystems[i].Item1, m_ABMSystems[i].Item1.GetComponent<ABMMissileLauncher>().CanShootMissile());

        }

        for(int i = 0; i < m_CIWSs.Count; i++)
        {
            //Removing the the anti-aircraft system if it is destroyed
            //For the case that the system is destroyed by the enemy
            if (m_CIWSs[i].Item1 == null)
            {
                m_CIWSs.Remove(m_CIWSs[i]);
                continue;
            }
            //Debug.Log($"{m_CIWSs[i].Item1.name} is available: {m_CIWSs[i].Item2}");
            m_CIWSs[i] = new(m_CIWSs[i].Item1, m_CIWSs[i].Item1.GetComponent<CIWS>().IsSystemReady());
        }
    }
}




