using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public class RoadMap
{
    Dictionary<float3, RoadPiece> posToPiece = new Dictionary<float3, RoadPiece>();
    Dictionary<RoadPiece, List<float3>> pieceToPos = new Dictionary<RoadPiece, List<float3>>();

    public MapGenerator Generator { get; private set; }

    public RoadMap(MapGenerator generator)
    {
        Generator = generator;
    }

    public RoadPiece this[float x, float y, float z]
    {
        get
        {
            if (posToPiece.TryGetValue(float3(x,y,z),out var piece))
            {
                return piece;
            }
            return null;
        }
    }

    public RoadPiece this[float3 position]
    {
        get => this[position.x, position.y, position.z];
    }

    public static float3 AlignToGrid(float3 position, float3 tileSize)
    {
        return round(position / tileSize) * tileSize;
    }

    public float3 AlignToGrid(float3 position)
    {
        return AlignToGrid(position, Generator.TileSize);
    }

    public bool CanPlaceRoad(RoadPiece prefab, float3 position, Quaternion rotation)
    {
        foreach (var point in prefab.GetOccupiedWorldPositions(position, rotation))
        {
            if (posToPiece.ContainsKey(AlignToGrid(point)))
            {
                return false;
            }
        }

        return true;
    }

    public static void DebugDrawCube(Bounds bounds,Color color, float time)
    {
        var min = bounds.min - new Vector3(0.0f,0.0f,0.0f);
        var max = bounds.max + new Vector3(0.0f, 0.0f, 0.0f);

        Debug.DrawLine(min, new Vector3(min.x, min.y, max.z), color, time);
        Debug.DrawLine(min, new Vector3(min.x, max.y, min.z), color, time);
        Debug.DrawLine(min, new Vector3(max.x, min.y, min.z), color, time);

        Debug.DrawLine(max, new Vector3(max.x, max.y, min.z), color, time);
        Debug.DrawLine(max, new Vector3(max.x, min.y, max.z), color, time);
        Debug.DrawLine(max, new Vector3(min.x, max.y, max.z), color, time);

        Debug.DrawLine(min,max,color,time);
    }

    public bool CanPlaceRoadAtPoint(RoadPiece prefab, Transform inPointOnPrefab, Transform outPointOnDest)
    {
        GetRoadPosAndRotAtPoint(inPointOnPrefab, outPointOnDest, out var destPosition, out var destRotation);

        return CanPlaceRoad(prefab, destPosition, destRotation);
    }

    public bool RoadHasExitAtPoint(RoadPiece prefab, Transform inPointOnPrefab, Transform outPointOnDest, out RoadPiece connectedPiece, out Transform connectedPieceOutput, bool print = false)
    {
        GetRoadPosAndRotAtPoint(inPointOnPrefab, outPointOnDest, out var destPosition, out var destRotation);

        return RoadHasExit(prefab, destPosition, destRotation, out connectedPiece, out connectedPieceOutput, print);
    }

    public float GetRoadPlacementHeightAtPoint(Transform inPointOnPrefab, Transform outPointOnDest)
    {
        GetRoadPosAndRotAtPoint(inPointOnPrefab, outPointOnDest, out var destPosition, out var destRotation);
        return destPosition.y;
    }

    public (Vector3 outPos, Quaternion outRot) GetOutputPos(Transform prefabOutput, float3 roadPosition, Quaternion roadRotation)
    {
        var trueOutputPos = roadPosition + mul(roadRotation, prefabOutput.localPosition);
        var trueOutputRotation = roadRotation * prefabOutput.localRotation;
        return (trueOutputPos,trueOutputRotation);
    }

    public Vector3 GetNextPointAtOutputPos(Transform prefabOutput, float3 roadPosition, Quaternion roadRotation, float multiplier = 0.5f)
    {
        var trueOutputPos = roadPosition + mul(roadRotation, prefabOutput.localPosition);
        var trueOutputRotation = roadRotation * prefabOutput.localRotation;

        return round((trueOutputPos + mul(trueOutputRotation, new float3(0f, 0f, multiplier))) / Generator.TileSize) * Generator.TileSize;
    }

    public bool RoadHasExit(RoadPiece prefab, float3 position, Quaternion rotation, out RoadPiece connectedPiece, out Transform connectedPieceOutput, bool print = false)
    {
        bool testVar = false;
        void Log(string message)
        {
            if (testVar)
            {
                Debug.Log(message);
            }
        }

        void Ray(float3 pos, float3 offset, Color color, float time)
        {
            if (testVar)
            {
                Debug.DrawRay(pos, offset, color, time);
            }
        }

        position = AlignToGrid(position);
        var prefabOutput = prefab.GetPrimaryOutput();

        Vector3 trueOutputPos = position + mul(rotation, prefabOutput.localPosition);
        if (OutputBlockedAtPoint(prefabOutput, position,rotation,out var piece))
        {
            if (piece.name.Contains("road_crossroad") || piece.name.Contains("road_bridge") || piece.name.Contains("road_roundabout"))
            {
                testVar = true;
            }
            foreach (var pair in piece.GetInOutPairs())
            {
                Ray(pair.InPoint.transform.position,float3(0f,1f,0f),Color.green,10f);
                if (all(AlignToGrid(pair.InPoint.transform.position) == AlignToGrid(trueOutputPos)))
                {
                    connectedPiece = piece;
                    connectedPieceOutput = pair.OutPoint;
                    return true;
                }
            }

            foreach (var pair in piece.GetInOutPairs())
            {
                if (all(AlignToGrid(pair.OutPoint.transform.position) == AlignToGrid(trueOutputPos)) && !InputBlockedAtPoint(pair.InPoint,piece.transform.position,piece.transform.rotation,out _))
                {
                    var oldOutPos = pair.OutPoint.transform.position;
                    var oldOutRot = pair.OutPoint.transform.rotation;

                    pair.OutPoint.transform.position = pair.InPoint.transform.position;
                    pair.OutPoint.transform.rotation = pair.InPoint.transform.rotation;

                    pair.InPoint.transform.position = oldOutPos;
                    pair.InPoint.transform.rotation = oldOutRot;

                    var flipRotation = Unity.Mathematics.quaternion.Euler(0f,radians(180f),0f);

                    pair.InPoint.transform.rotation *= flipRotation;
                    pair.OutPoint.transform.rotation *= flipRotation;

                    connectedPiece = piece;
                    connectedPieceOutput = pair.OutPoint;
                    return true;
                }
            }

            connectedPiece = null;
            connectedPieceOutput = null;
            return false;
        }
        else
        {
            connectedPiece = null;
            connectedPieceOutput = null;
            return true;
        }
    }

    public void GetRoadPosAndRotAtPoint(Transform inPointOnPrefab, Transform outPointOnDest, out Vector3 destPos, out Quaternion destRot)
    {
        var inPointLocalPos = inPointOnPrefab.localPosition;
        var inPointLocalRot = inPointOnPrefab.localRotation;

        var outPointRotation = outPointOnDest.rotation;

        var ninetyRotation = Unity.Mathematics.quaternion.Euler(0f, radians(-inPointLocalRot.eulerAngles.y), 0f);

        destPos = AlignToGrid(outPointOnDest.position + (outPointRotation * ninetyRotation * -inPointLocalPos));
        destRot = outPointRotation * ninetyRotation;
    }

    public bool AddRoadAtPoint(RoadPiece prefab, Transform inPointOnPrefab, Transform outPointOnDest, out RoadPiece piece)
    {
        GetRoadPosAndRotAtPoint(inPointOnPrefab, outPointOnDest, out var destPosition, out var destRotation);


        return AddRoad(prefab, destPosition, destRotation, out piece);
    }

    public bool OutputBlockedAtPoint(Transform prefabOutput, float3 position, quaternion rotation, out RoadPiece roadAtOutput)
    {
        var trueOutputPos = position + mul(rotation,prefabOutput.localPosition);
        var trueOutputRotation = rotation * prefabOutput.localRotation;

        var nextPoint = round((trueOutputPos + mul(trueOutputRotation, new float3(0f, 0f, 0.5f))) / Generator.TileSize) * Generator.TileSize;

        return posToPiece.TryGetValue(AlignToGrid(nextPoint), out roadAtOutput);
    }

    public bool InputBlockedAtPoint(Transform prefabInput, float3 position, quaternion rotation, out RoadPiece roadAtInput)
    {
        var trueInputPos = position + mul(rotation, prefabInput.localPosition);
        var trueInputRotation = rotation * prefabInput.localRotation;

        var nextPoint = round(trueInputPos + mul(trueInputRotation, new float3(0f, 0f, -0.5f)));

        return posToPiece.TryGetValue(AlignToGrid(nextPoint), out roadAtInput);
    }

    public bool RoadsAreConnected(RoadPiece a, RoadPiece b)
    {
        foreach (var aPair in a.GetInOutPairs())
        {
            foreach (var bPair in b.GetInOutPairs())
            {
                if (aPair.InPoint.transform.position == bPair.OutPoint.transform.position)
                {
                    return true;
                }
                else if (aPair.OutPoint.transform.position == bPair.InPoint.transform.position)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ClearAllRoads()
    {
        foreach ((float3 key, RoadPiece value) in posToPiece)
        {
            GameObject.Destroy(value.gameObject);
        }

        posToPiece.Clear();
        pieceToPos.Clear();
    }

    public bool AddRoad(RoadPiece prefab, float3 position, Quaternion rotation,out RoadPiece placedPiece)
    {
        position = AlignToGrid(position);
        if (!CanPlaceRoad(prefab,position,rotation))
        {
            placedPiece = null;
            return false;
        }
        placedPiece = GameObject.Instantiate(prefab, new Vector3(position.x,position.y,position.z), rotation);
        placedPiece.SourceGenerator = prefab.SourceGenerator;


        foreach (var pillar in placedPiece.GetPillars())
        {
            if (position.y <= 0)
            {
                pillar.gameObject.SetActive(false);
            }
        }



        placedPiece.name += $" - {pieceToPos.Count}";
        List<float3> points = new List<float3>();
        foreach (var point in prefab.GetOccupiedWorldPositions(position,rotation))
        {
            points.Add(AlignToGrid(point));
        }

        pieceToPos.Add(placedPiece, points);

        foreach (var point in points)
        {
            posToPiece.Add(point, placedPiece);
        }

        return true;
    }

    public bool RemoveRoad(RoadPiece instance)
    {
        if (pieceToPos.Remove(instance,out var positions))
        {
            foreach (var pos in positions)
            {
                posToPiece.Remove(pos);
            }
            return true;
        }
        return false;
    }

    public bool RemoveRoadAt(float3 position)
    {
        if (posToPiece.TryGetValue(position,out var roadInstance))
        {
            return RemoveRoad(roadInstance);
        }
        return false;
    }
}
