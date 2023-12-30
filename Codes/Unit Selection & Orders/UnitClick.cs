using UnityEngine;

public class UnitClick : MonoBehaviour
{
    [SerializeField]
    private Camera m_RTSCamera;

    [SerializeField]
    private LayerMask m_clickableLayer;

    [SerializeField]
    private LayerMask m_groundLayer;

    [SerializeField]
    private LayerMask m_enemyLayer;

    [SerializeField]
    private Canvas m_missileControllingSystemCanvas;

    [SerializeField]
    Texture2D m_cursorTexture;

    private bool m_isMissilleControllingSystemActive = false;

    private GameObject m_missileControllingSystem;



    private void Update()
    {
        RaycastHit hit;
        Ray ray = m_RTSCamera.ScreenPointToRay(Input.mousePosition);
        //if the player is in the missile controlling system then it should not be able to select any other unit
        if (m_isMissilleControllingSystemActive)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {




            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_clickableLayer))
            {
                //if it is missile launching controller then it must be the only one selected
                if (hit.collider.gameObject.tag == "MissileLaunchingController")
                {
                    m_isMissilleControllingSystemActive = true;
                    m_missileControllingSystemCanvas.gameObject.SetActive(true);
                    Debug.Log("Missile Controller");
                    m_missileControllingSystem = hit.collider.gameObject;
                    UnitRegisterer.Instance.SelectMissileLaunchingController(hit.collider.gameObject);
                    hit.collider.gameObject.GetComponent<MissileControllingsystem>().ActivateMissileControllingSystem();
                    return;
                }
                //Debug.Log("We hit " + hit.collider.name + " " + hit.point);
                //if we click on a unit while holding the shift key it adds the unit to the selected units
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    UnitRegisterer.Instance.ShiftClickSelect(hit.collider.gameObject);
                }
                //if the player click on a unit without holding the shift key it selects the unit
                //In simpler words, normal click
                else
                {
                    Debug.Log(hit.transform.name + "Name in else");

                    UnitRegisterer.Instance.ClickSelect(hit.collider.gameObject);
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    UnitRegisterer.Instance.DeselectAll();
                }
            }
        }

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_enemyLayer))
        {
            
            Cursor.SetCursor(m_cursorTexture, new Vector2(m_cursorTexture.width/2, m_cursorTexture.width), CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (m_isMissilleControllingSystemActive)
            {
                return;
            }

            //Debug.Log("button (1)");
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_groundLayer))
            {
                if (UnitRegisterer.Instance.selectedSoldiers.Count == 1)
                {
                    UnitRegisterer.Instance.selectedSoldiers[0].GetComponent<FriendlyAgent>().GetOrder(hit.point);
                }
                else
                {
                    FormationMove(hit.point);
                }
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_enemyLayer))
            {
                for (int i = 0; i < UnitRegisterer.Instance.selectedSoldiers.Count; i++)
                {
                    if (UnitRegisterer.Instance.selectedSoldiers[i] == null)
                    {
                        continue;
                    }
                    UnitRegisterer.Instance.selectedSoldiers[i].GetComponent<FriendlyAgent>().GetOrderToKill(hit.collider.gameObject);
                }
            }

        }
    }

    private void FormationMove(Vector3 destination)
    {
        float soldierSpacing = 4.0f; // Adjust this value for a larger distance between soldiers

        if (UnitRegisterer.Instance.selectedSoldiers.Count <= 10)
        {
            for (int i = 0; i < UnitRegisterer.Instance.selectedSoldiers.Count; i++)
            {
                if (UnitRegisterer.Instance.selectedSoldiers[i] == null)
                {
                    continue;
                }
                float row = i / 3; // 3 soldiers per row
                float col = i % 3; // 3 soldiers per column
                Vector3 offset = new Vector3(col * soldierSpacing, 0, row * soldierSpacing);

                UnitRegisterer.Instance.selectedSoldiers[i].GetComponent<FriendlyAgent>().GetOrder(destination + offset);
            }
        }
        else if (UnitRegisterer.Instance.selectedSoldiers.Count > 10)
        {

            for (int i = 0; i < UnitRegisterer.Instance.selectedSoldiers.Count; i++)
            {
                if (UnitRegisterer.Instance.selectedSoldiers[i] == null)
                {
                    continue;
                }

                UnitRegisterer.Instance.selectedSoldiers[i].GetComponent<FriendlyAgent>().GetOrder(destination);
            }
        }

    }
    private void OnDisable()
    {
        m_missileControllingSystemCanvas.gameObject.SetActive(false);
        m_isMissilleControllingSystemActive = false;
    }

    public void MissileControllingSystemDeactivated()
    {
        m_isMissilleControllingSystemActive = false;
        m_missileControllingSystemCanvas.gameObject.SetActive(false);
        m_missileControllingSystem.GetComponent<MissileControllingsystem>().DeactivateMissileControllingSystem();
    }
}
