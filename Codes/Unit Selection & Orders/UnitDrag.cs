using UnityEngine;

public class UnitDrag : MonoBehaviour
{
    //As the game has multiple cameras we must give a reference to the RTS phase camera to the unit drag system
    [SerializeField]
    Camera myCam;

    [SerializeField]
    private RectTransform m_boxVisual;

    Rect selectionBox;

    private Vector2 m_startPos;
    private Vector2 m_endPos;

    // Start is called before the first frame update
    void Start()
    {
        DrawVisual();
    }

    // Update is called once per frame
    void Update()
    {
        //when mouse button is pressed
        if(Input.GetMouseButtonDown(0))
        {
            m_startPos = Input.mousePosition;
            selectionBox = new Rect();
        }

        //when dragging
        if(Input.GetMouseButton(0))
        {
            m_endPos = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }

        //when mouse button is released
        if(Input.GetMouseButtonUp(0))
        {
            SelectUnits();
            m_startPos = Vector2.zero;
            m_endPos = Vector2.zero;
            DrawVisual();
        }
    }
    
    void DrawVisual()
    {
        Vector2 boxStart = m_startPos;
        Vector2 boxEnd = m_endPos;  

        Vector2 boxCenter = (boxStart + boxEnd) / 2f;
        m_boxVisual.position = boxCenter;
        
        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));  

        m_boxVisual.sizeDelta = boxSize;
    }

    void DrawSelection()
    {
        //if the start position is to the left of the end position then the xMin is the start position
        //and the xMax is the end position
        if(Input.mousePosition.x < m_startPos.x)
        {
            selectionBox.xMin = Input.mousePosition.x;
            selectionBox.xMax = m_startPos.x;
        }
        //if the start position is to the right of the end position then the xMin is the end position then the xMax is the start position
        //and the xMax is the start position
        else
        {
            selectionBox.xMin = m_startPos.x;
            selectionBox.xMax = Input.mousePosition.x;
        }

        //if the start position is below the end position then the yMin is the start position then the yMax is the end position
        //and the yMax is the end position
        if(Input.mousePosition.y < m_startPos.y)
        {
            selectionBox.yMin = Input.mousePosition.y;
            selectionBox.yMax = m_startPos.y;
        }
        //if the start position is above the end position then the yMin is the end position then the yMax is the start position then the yMax is the start position
        //and the yMax is the start position
        else
        {
            selectionBox.yMin = m_startPos.y;
            selectionBox.yMax = Input.mousePosition.y;
        }
    }

    void SelectUnits()
    {
        UnitRegisterer.Instance.RemoveNullSoldiers();
        //select all units in the scene that are within the selection box
        foreach(var unit in UnitRegisterer.Instance.allSoldiers)
        {
            if(unit == null)
            {
                continue;
            }
            if(selectionBox.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
            {
                Debug.Log($"name of the unit in the box selected is : {unit.name}");
                UnitRegisterer.Instance.DragSelect(unit);
                unit.transform.Find("SelectedElement").gameObject.SetActive(true);
            }
        }
    }
      
}