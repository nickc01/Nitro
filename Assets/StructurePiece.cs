using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public class StructurePiece : MonoBehaviour
{
    [field: SerializeField]
    public float3 MinBounds { get; private set; } = new int3(0, 0, 0);

    [field: SerializeField]
    public float3 MaxBounds { get; private set; } = new int3(0, 0, 0);

    [field: SerializeField]
    public float3 Center { get; private set; } = new float3(0, 0, 0);

    [field: SerializeField]
    public float RandomChance { get; private set; } = 1f;

    [field: SerializeField]
    public float3 TileSize = new float3(0.5f);

    private void OnDrawGizmosSelected()
    {
        var bounds = GetOccupiedWorldBounds(transform.position, transform.rotation, TileSize);
        bounds.min -= new Vector3(0.1f, 0.1f, 0.1f);
        bounds.max += new Vector3(0.1f, 0.1f, 0.1f);
        Gizmos.color = Color.Lerp(Color.grey, Color.clear, 0.5f);
        Gizmos.DrawCube(bounds.center, bounds.size);

        Gizmos.color = Color.Lerp(Color.green, Color.clear, 0.5f);
        Gizmos.DrawCube((Vector3)Center + transform.position, Vector3.one / 3f);

        /*int pointCount = 0;
        foreach (var localPos in GetOccupiedLocalPositions())
        {
            var worldPos = transform.TransformPoint(localPos);
            pointCount++;
        }

        foreach (var worldPos in GetOccupiedWorldPositions(transform.position, transform.rotation))
        {
            pointCount++;
        }*/

        var testBounds = GetOccupiedWorldBounds(transform.position, transform.rotation, TileSize);
        testBounds.max += new Vector3(0.5f, 0.1f, 0.5f);
        testBounds.min -= new Vector3(0.5f, 0.1f, 0.5f);

        RoadMap.DebugDrawCube(testBounds, Color.magenta, 0.01f);
        //Debug.Log("POINTS = " + pointCount);
    }

    public Bounds GetOccupiedLocalBounds()
    {
        return new Bounds(Vector3.zero, Vector3.zero)
        {
            min = MinBounds + Center,
            max = MaxBounds + Center
        };
    }

    public Bounds GetOccupiedWorldBounds(float3 worldpos, quaternion worldRotation, float3 tileSize)
    {
        float3 min = new float3(float.PositiveInfinity);
        float3 max = new float3(float.NegativeInfinity);

        foreach (var point in GetOccupiedWorldPositions(worldpos, worldRotation, tileSize))
        {
            if (point.x < min.x)
            {
                min.x = point.x;
            }

            if (point.y < min.y)
            {
                min.y = point.y;
            }

            if (point.z < min.z)
            {
                min.z = point.z;
            }

            if (point.x > max.x)
            {
                max.x = point.x;
            }

            if (point.y > max.y)
            {
                max.y = point.y;
            }

            if (point.z > max.z)
            {
                max.z = point.z;
            }
        }

        return new Bounds(Vector3.zero, Vector3.zero)
        {
            min = min,
            max = max
        };
    }

    public IEnumerable<float3> GetOccupiedLocalPositions(float3 tileSize)
    {
        var bounds = GetOccupiedLocalBounds();

        for (float x = bounds.min.x; x <= bounds.max.x; x += tileSize.x)
        {
            for (float y = bounds.min.y; y <= bounds.max.y; y += tileSize.y)
            {
                for (float z = bounds.min.z; z <= bounds.max.z; z += tileSize.z)
                {
                    yield return new float3(x, y, z);
                }
            }
        }
    }

    public IEnumerable<float3> GetOccupiedWorldPositions(float3 worldPos, quaternion worldRotation, float3 tileSize)
    {
        Matrix4x4 matrix2 = Matrix4x4.TRS(worldPos, worldRotation, Vector3.one);
        //float4x4 matrix = new float4x4(worldRotation, worldPos);
        foreach (var point in GetOccupiedLocalPositions(tileSize))
        {
            //yield return matrix2.MultiplyVector(point);
            yield return worldPos + (mul(worldRotation, point));
        }
    }
}
