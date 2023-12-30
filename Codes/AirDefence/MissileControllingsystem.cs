using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissileControllingsystem : MonoBehaviour
{

    public List<GameObject> cruiseMissiles = new List<GameObject>();
    public List<GameObject> ballisticMissiles = new List<GameObject>();
    private Queue<Transform> m_cruiseMissilesTargets = new Queue<Transform>();
    private Queue<GameObject> m_ballisticMissilesTargets = new Queue<GameObject>();

    private Queue<GameObject> m_readyToLaunchCruiseMissiles = new Queue<GameObject>();
    private Queue<GameObject> m_readyToLaunchBallisticMissiles = new Queue<GameObject>();

    private bool m_isMissilleControllingSystemActive = false;

    //this list store all the target definers on the map
    //this way we can delete them after the missiles are launched
    private Queue<GameObject> m_targetDefiners = new Queue<GameObject>();

    bool m_isCruiseMissileSelected = false;

    public static MissileControllingsystem Instance;

    [SerializeField]
    private LayerMask m_targetables;

    [SerializeField]
    private Camera m_RTSCamera;

    //mark the point that the missile will hit
    [SerializeField]
    private GameObject m_targetDefiner;

    [SerializeField]
    private TMP_Text  m_availableCruiseMissilesText;

    [SerializeField]
    private TMP_Text m_availableBallisticMissilesText;

    private int m_cruiseMissilewithTarget;
    private int m_ballisticMissilewithTarget;
    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

    }


    // Update is called once per frame
    void Update()
    {
        m_availableBallisticMissilesText.text = "Available: " + ballisticMissiles.Count.ToString();
        m_availableCruiseMissilesText.text = "Available: " + cruiseMissiles.Count.ToString();
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    LaunchMissiles();
        //}

        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    CancelLaunch();
        //}

        if (cruiseMissiles.Count <= 0 && ballisticMissiles.Count <= 0)
        {
            return;
        }



        if (m_isMissilleControllingSystemActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse Clicked");
                if (m_isCruiseMissileSelected && cruiseMissiles.Count > 0)
                {
                    Debug.Log("Cruise Missile Selected");
                    SelectCruiseMissileTargets();
                    return;
                }

                if (!m_isCruiseMissileSelected && ballisticMissiles.Count > 0)
                {
                    Debug.Log("Ballistic Missile Selected");
                    SelectBallisticMissileTargets();
                    return;
                }
            }
        }
    }


    private void SelectCruiseMissileTargets()
    {
        Ray ray = m_RTSCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000, m_targetables))
        {
            m_cruiseMissilesTargets.Enqueue(hit.transform);
            m_targetDefiners.Enqueue(Instantiate(m_targetDefiner, hit.point, Quaternion.identity));
            m_readyToLaunchCruiseMissiles.Enqueue(cruiseMissiles[0]);
            cruiseMissiles.RemoveAt(0);
        }
    }

    private void SelectBallisticMissileTargets()
    {
        Ray ray = m_RTSCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000, m_targetables))
        {
            m_ballisticMissilesTargets.Enqueue(hit.transform.gameObject);
            m_targetDefiners.Enqueue(Instantiate(m_targetDefiner, hit.point, Quaternion.identity));
            m_readyToLaunchBallisticMissiles.Enqueue(ballisticMissiles[0]);
            ballisticMissiles.RemoveAt(0);
        }
    }


    public void SelectCruiseMissiles()
    {
        m_isCruiseMissileSelected = true;
    }

    public void SelectBallisticMissiles()
    {
        m_isCruiseMissileSelected = false;
    }



    //This method is called when the player clicks the launch button
    //It will launch all the missiles in the queue
    //It will also destroy all the target definers
    public void LaunchMissiles()
    {
        while (m_targetDefiners.Count > 0)
        {
            Destroy(m_targetDefiners.Dequeue());
        }

        while (m_readyToLaunchCruiseMissiles.Count > 0)
        {
            GameObject missile = m_readyToLaunchCruiseMissiles.Dequeue();
            Transform target = m_cruiseMissilesTargets.Dequeue();
            missile.GetComponent<CruiseMissile>().FireToTarget(target);
        }

        while (m_readyToLaunchBallisticMissiles.Count > 0)
        {
            GameObject missile = m_readyToLaunchBallisticMissiles.Dequeue();
            GameObject target = m_ballisticMissilesTargets.Dequeue();
            missile.GetComponent<BallisticMissile>().FireToTarget(target);
        }
    }

    //If the player cancels the launch, we need to put the missiles back to the queue
    public void CancelLaunch()
    {
        while (m_targetDefiners.Count > 0)
        {
            Destroy(m_targetDefiners.Dequeue());
        }

        while (m_readyToLaunchCruiseMissiles.Count > 0)
        {
            cruiseMissiles.Add(m_readyToLaunchCruiseMissiles.Dequeue());
        }

        while (m_readyToLaunchBallisticMissiles.Count > 0)
        {
            ballisticMissiles.Add(m_readyToLaunchBallisticMissiles.Dequeue());
        }

        // Clearing the target queues
        m_cruiseMissilesTargets.Clear();
        m_ballisticMissilesTargets.Clear();
    }

    public void ActivateMissileControllingSystem()
    {
        m_isMissilleControllingSystemActive = true;
    }

    public void DeactivateMissileControllingSystem()
    {
        m_isMissilleControllingSystemActive = false;
    }
}
