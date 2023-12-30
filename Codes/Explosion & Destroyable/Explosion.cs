using Unity.Jobs;
using UnityEngine;


//This script is attached to a prefab that handles the explosion of any object
//All objects that could get exploded are consists of two separate prefabs: the object itself and the exploded version of it
public class Explosion : MonoBehaviour,IJob
{

    //this variable is used to store all the rigidbodies that are in the explosion radius
    private Rigidbody[] m_rigidbodies;

    //this variable store a reference to the explosion part of the prefab, which is inactive at the start; and it will be activated when the object is destroyed
    private GameObject m_explosion;

    public Vector3 m_explosionPosition { set; private get; }


    private void Awake()
    {
        m_rigidbodies = GetComponentsInChildren<Rigidbody>();
        //As the explosion is component of the explosion gameobject, we need to get the parent of the explosion gameobject and deactivate it
        transform.parent.gameObject.SetActive(false);
    }

    private void Start()
    {
        ((IJob)this).Execute();
    }


    void IJob.Execute()
    {
        for(int i = 0; i < m_rigidbodies.Length; i++)
        {
            if (m_rigidbodies[i] != null)
            {
                m_rigidbodies[i].transform.parent = null;
                m_rigidbodies[i].AddExplosionForce(45000, m_explosionPosition + new Vector3(0,5,0), 100);
            }
        }
    }    
}
