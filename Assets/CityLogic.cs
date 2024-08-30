using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keep track what buildings are in the city
// and where
// Keep track of city status
// Creature status
public class CityLogic : MonoBehaviour
{
    int buildingAmount = 0;

    float electricityDemand = 0.0f;
    float housingDemand = 0.0f;
    float workDemand = 0.0f;
    float leisureDemand = 0.0f;

    // Attracts
    float dogAttraction = 0.0f;

    public GameObject dogPrefab;


    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("Builder").GetComponent<BuildController>().HousePlacedEvent += OnBuildingAdded;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called when a building is added to the city
    void OnBuildingAdded(int buildingType)
    {
        Debug.Log($"CityLogic: got building type {buildingType}");
        BuildingType bt = (BuildingType)buildingType;
        if (bt == BuildingType.BlueHouse)
        {
            dogAttraction += 1.0f;
        }
        buildingAmount += 1;

        if (dogAttraction > 0.0f)
        {
            Instantiate(dogPrefab, new Vector3(1.0f, -1.0f, 0.0f), Quaternion.identity);
        }
    }

	private void OnGUI()
	{
        MenuCreator statusMenu = new MenuCreator(Screen.width - 140, 10, 100, 20);
        statusMenu.Label($"Buildings {buildingAmount}");

        statusMenu.Label("Demands");
        statusMenu.Label($"Electricity: {electricityDemand}");
        statusMenu.Label($"Housing: {housingDemand}");
        statusMenu.Label($"Work: {workDemand}");
        statusMenu.Label($"Leisure: {leisureDemand}");

        statusMenu.Label("Attraction");
        statusMenu.Label($"Dog: {dogAttraction}");
    }
}
