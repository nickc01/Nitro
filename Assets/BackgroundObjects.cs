using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundObjects : MonoBehaviour
{
    [SerializeField]
    List<GameObject> roadPrefabs;

    [SerializeField]
    List<GameObject> buildingPrefabs;

    [SerializeField]
    Camera mainCamera;

    [SerializeField]
    int startingRoadLength = 20;

    [SerializeField]
    float roadMovementSpeed = 1f;


    List<GameObject> roads = new List<GameObject>();
    List<GameObject> buildings = new List<GameObject>();
    List<GameObject> buildingsBackrow = new List<GameObject>();

    public bool Running { get; private set; } = true;

    private void Awake()
    {
        for (int i = -startingRoadLength / 2; i < startingRoadLength; i++)
        {
            {
                var instance = GameObject.Instantiate(roadPrefabs[UnityEngine.Random.Range(0, roadPrefabs.Count)], default, Quaternion.identity);
                instance.transform.SetParent(transform);
                instance.transform.localPosition = new Vector3(i, 0, 0);
                roads.Add(instance);
            }

            {
                var instance = GameObject.Instantiate(buildingPrefabs[UnityEngine.Random.Range(0, buildingPrefabs.Count)], default, Quaternion.identity);
                instance.transform.SetParent(transform);
                instance.transform.localPosition = new Vector3(i, 0, 1);
                buildings.Add(instance);
            }

            {
                var instance = GameObject.Instantiate(buildingPrefabs[UnityEngine.Random.Range(0, buildingPrefabs.Count)], default, Quaternion.identity);
                instance.transform.SetParent(transform);
                instance.transform.localPosition = new Vector3(i + 0.5f, 0, 2);
                buildingsBackrow.Add(instance);
            }
        }
    }

    private void Update()
    {
        if (Running)
        {
            foreach (var road in roads)
            {
                road.transform.localPosition -= new Vector3(roadMovementSpeed * Time.deltaTime, 0f, 0f);
            }

            for (int i = 0; i < roads.Count; i++)
            {
                var road = roads[i];

                if (road.transform.localPosition.x < -startingRoadLength / 2)
                {
                    roads.RemoveAt(i);
                    road.transform.localPosition = roads[^1].transform.localPosition + new Vector3(1f, 0, 0);
                    roads.Add(road);
                }
                else
                {
                    break;
                }
            }


            foreach (var building in buildings)
            {
                building.transform.localPosition -= new Vector3(roadMovementSpeed * Time.deltaTime, 0f, 0f);
            }

            for (int i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];

                if (building.transform.localPosition.x < -startingRoadLength / 2)
                {
                    buildings.RemoveAt(i);
                    building.transform.localPosition = buildings[^1].transform.localPosition + new Vector3(1f, 0, 0);
                    buildings.Add(building);
                }
                else
                {
                    break;
                }
            }


            foreach (var building in buildingsBackrow)
            {
                building.transform.localPosition -= new Vector3(roadMovementSpeed * Time.deltaTime, 0f, 0f);
            }

            for (int i = 0; i < buildingsBackrow.Count; i++)
            {
                var building = buildingsBackrow[i];

                if (building.transform.localPosition.x < -startingRoadLength / 2)
                {
                    buildingsBackrow.RemoveAt(i);
                    building.transform.localPosition = buildingsBackrow[^1].transform.localPosition + new Vector3(1f, 0, 0);
                    buildingsBackrow.Add(building);
                }
                else
                {
                    break;
                }
            }
        }
    }

    public void Stop()
    {
        Running = false;
        foreach (var road in roads)
        {
            GameObject.Destroy(road.gameObject);
        }

        foreach (var building in buildings)
        {
            GameObject.Destroy(building.gameObject);
        }

        foreach (var building in buildingsBackrow)
        {
            GameObject.Destroy(building.gameObject);
        }
    }
}
