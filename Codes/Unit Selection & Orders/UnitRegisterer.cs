using System.Collections.Generic;
using UnityEngine;

public class UnitRegisterer : MonoBehaviour
{
    //Soldiers that are in the scene
    public List<GameObject> allSoldiers;
    public List<GameObject> selectedSoldiers;

    //Missile Lunching controller
    public GameObject missileLaunchingSystem;

    public static UnitRegisterer Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
           Destroy(gameObject);
        }
    }
    
    public void ClickSelect(GameObject unitToAdd)
    {
        DeselectAll();
        //if the missile controller is selected it should deselect all the soldiers
        selectedSoldiers.Add(unitToAdd);
        //The element that shows the unit is sleceted should be shown
        unitToAdd.transform.Find("SelectedElement").gameObject.SetActive(true);
    }

    public void ShiftClickSelect(GameObject unitToAdd)
    {
        //if the unit is not slected add it to the list
        if (!selectedSoldiers.Contains(unitToAdd))
        {
            Debug.Log("Add unit to list");
            selectedSoldiers.Add(unitToAdd);
        }
        //if the unit is selected remove it from the list
        else
        {
            Debug.Log("After else Add unit to list");
            selectedSoldiers.Remove(unitToAdd);
        }
    }

    public void DragSelect(GameObject unitToAdd)
    {
        if (!selectedSoldiers.Contains(unitToAdd))
        {
            selectedSoldiers.Add(unitToAdd);
        }
    }
    
    public void DeselectAll()
    {
        //the element that shows the unit is sleceted should be hidden
        foreach (GameObject soldier in selectedSoldiers)
        {
            if(soldier != null)
            soldier.transform.Find("SelectedElement").gameObject.SetActive(false);
        }
        selectedSoldiers.Clear();
    }

    //when the missile launching controller is selected all the soldiers should be deselected
    public void SelectMissileLaunchingController(GameObject missileLaunchingController)
    {
        DeselectAll();
        missileLaunchingSystem = missileLaunchingController;
    }

    public void RemoveNullSoldiers()
    {
        for(int i = 0; i < allSoldiers.Count; i++)
        {
            if (allSoldiers[i] == null)
            {
                allSoldiers.RemoveAt(i);
            }
        }
    }
}
