using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class SpaceOutChildren : MonoBehaviour
{
    [SerializeField]
    Vector3 startPos;

    [SerializeField]
    Vector3 spacing = new Vector3(2,0,2);

    [SerializeField]
    int width = 5;

    private void OnEnable()
    {
        Vector3 currentPos = Vector3.zero;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            child.localPosition = startPos + new Vector3(currentPos.x * spacing.x,currentPos.y * spacing.y, currentPos.z * spacing.z);

            currentPos.x += 1;

            if (currentPos.x >= width)
            {
                currentPos.x -= width;
                currentPos.z += 1;
            }
        }
    }
}
