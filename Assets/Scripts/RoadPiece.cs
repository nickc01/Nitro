using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public class RoadPiece : MonoBehaviour
{
    [Flags]
    public enum PlacementState
    {
        None,
        Ground = 1,
        Air = 2,
    }

    public enum SupportsNeeded
    {
        No,
        Yes,
        OnlyInAir
    }

    public struct InOutPair
    {
        public Transform InPoint;
        public Transform OutPoint;

        public InOutPair(Transform inPoint, Transform outPoint)
        {
            InPoint = inPoint;
            OutPoint = outPoint;
        }
    }


    public PlacementState ValidState = PlacementState.Ground;

    public SupportsNeeded SupportState = SupportsNeeded.OnlyInAir;

    [field: SerializeField]
    public float3 MinBounds { get; private set; } = new int3(0,0,0);

    [field: SerializeField]
    public float3 MaxBounds { get; private set; } = new int3(0, 0, 0);

    [field: SerializeField]
    public float3 Center { get; private set; } = new float3(0,0,0);

    [field: NonSerialized]
    public MapGenerator SourceGenerator { get; set; }

    [field: SerializeField]
    public float RandomChance { get; private set; } = 1f;

    [field: SerializeField]
    public List<RoadPiece> Replacements { get; private set; } = new List<RoadPiece>();


    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("In"))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(child.transform.position, child.transform.forward);
                Gizmos.DrawSphere(child.transform.position, 0.1f);
            }
            else if (child.name.StartsWith("Out"))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(child.transform.position, child.transform.forward);
                Gizmos.DrawSphere(child.transform.position, 0.1f);
            }
        }

        var bounds = GetOccupiedWorldBounds(transform.position,transform.rotation);
        bounds.min -= new Vector3(0.1f,0.1f,0.1f);
        bounds.max += new Vector3(0.1f, 0.1f, 0.1f);

        Gizmos.color = Color.Lerp(Color.grey,Color.clear,0.5f);
        Gizmos.DrawCube(bounds.center, bounds.size);

        Gizmos.color = Color.Lerp(Color.green, Color.clear, 0.5f);
        Gizmos.DrawCube((Vector3)Center + transform.position, Vector3.one / 3f);

        int pointCount = 0;
        foreach (var localPos in GetOccupiedLocalPositions())
        {
            var worldPos = transform.TransformPoint(localPos);
            pointCount++;
        }

        foreach (var worldPos in GetOccupiedWorldPositions(transform.position,transform.rotation))
        {
            pointCount++;
        }

        var testBounds = GetOccupiedWorldBounds(transform.position, transform.rotation);
        testBounds.max += new Vector3(0.1f,0.1f,0.1f);
        testBounds.min -= new Vector3(0.1f,0.1f,0.1f);

        RoadMap.DebugDrawCube(testBounds, Color.magenta,0.01f);
    }


    public IEnumerable<InOutPair> GetInOutPairs()
    {
        Transform inPiece = null;
        Transform outPiece = null;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (inPiece == null)
            {
                if (child.name.Contains("In"))
                {
                    inPiece = child;
                }
            }
            else if (outPiece == null)
            {
                if (child.name.Contains("Out"))
                {
                    outPiece = child;
                    yield return new InOutPair(inPiece,outPiece);
                    inPiece = null;
                    outPiece = null;
                }
            }
        }
    }

    public IEnumerable<Transform> GetInputs()
    {
        foreach (var pair in GetInOutPairs())
        {
            yield return pair.InPoint;
        }
    }

    public IEnumerable<Transform> GetOutputs()
    {
        foreach (var pair in GetInOutPairs())
        {
            yield return pair.OutPoint;
        }
    }

    public Transform GetPrimaryInput()
    {
        return GetInOutPairs().First().InPoint;
    }

    public Transform GetPrimaryOutput()
    {
        return GetInOutPairs().First().OutPoint;
    }

    public IEnumerable<Transform> GetPillars()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.Contains("pillar"))
            {
                yield return child;
            }
        }
    }

    public Bounds GetOccupiedLocalBounds()
    {
        return new Bounds(Vector3.zero, Vector3.zero)
        {
            min = MinBounds + Center,
            max = MaxBounds + Center
        };
    }

    public Bounds GetOccupiedWorldBounds(float3 worldpos, quaternion worldRotation)
    {
        float3 min = new float3(float.PositiveInfinity);
        float3 max = new float3(float.NegativeInfinity);

        foreach (var point in GetOccupiedWorldPositions(worldpos,worldRotation))
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

    public IEnumerable<float3> GetOccupiedLocalPositions()
    {
        var bounds = GetOccupiedLocalBounds();

        for (float x = bounds.min.x; x <= bounds.max.x; x += SourceGenerator.TileSize.x)
        {
            for (float y = bounds.min.y; y <= bounds.max.y; y += SourceGenerator.TileSize.y)
            {
                for (float z = bounds.min.z; z <= bounds.max.z; z += SourceGenerator.TileSize.z)
                {
                    yield return new float3(x,y,z);
                }
            }
        }
    }

    public IEnumerable<float3> GetOccupiedWorldPositions(float3 worldPos, quaternion worldRotation)
    {
        foreach (var point in GetOccupiedLocalPositions())
        {
            yield return worldPos + (mul(worldRotation,point));
        }
    }
}
