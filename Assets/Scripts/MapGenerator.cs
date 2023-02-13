using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using System.Linq;
using System;
using UnityEngine.UIElements;
using Assets;
using System.Net.NetworkInformation;
using Mirror;
using UnityEngine.Serialization;
using UnityEngine.Events;

public class MapGenerator : MonoBehaviour
{
    [field: Header("Generator")]
    [field: SerializeField]
    public Vector3 TileSize { get; private set; }

    [SerializeField]
    Transform mapPiecesContainer;

    [field: SerializeField]
    public RoadPiece StartingRoad { get; private set; }

    [field: SerializeField]
    public uint Seed { get; private set; } = 1005;

    [field: SerializeField]
    public LODGroup LOD;

    [field: SerializeField]
    public int RoadLength = 100;

    [SerializeField]
    GameObject finishLine;

    [SerializeField]
    GameObject startingLine;

    public UnityEvent OnMapGenerated;

    public List<RoadPiece> GeneratedRoads { get; private set; }

    public List<Transform> GeneratedStartingPoints { get; private set; }

    public List<Transform> CheckPoints { get; private set; }

    public List<RandomizedPiece> SpecialPieces;

    public List<StructurePiece> Structures;

    static MapGenerator _instance;
    public static MapGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MapGenerator>();
            }
            return _instance;
        }
    }

    public enum State
    {
        Idle,
        Generating,
        Done
    }

    public State CurrentState { get; private set; } = State.Idle;

    public void GenerateMap(uint seed)
    {
        if (CurrentState != State.Idle)
        {
            return;
        }
        CurrentState = State.Generating;

        var randomizer = Unity.Mathematics.Random.CreateFromIndex(seed);
        List<RoadPiece> roads = new List<RoadPiece>();

        List<RoadPiece> upRoads = new List<RoadPiece>();
        List<RoadPiece> downRoads = new List<RoadPiece>();
        List<RoadPiece> neutralRoads = new List<RoadPiece>();

        GameObject startingLineInstance = null;
        GameObject finishLineInstance = null;

        mapPiecesContainer.GetComponentsInChildren(true, roads);

        foreach (var road in roads)
        {
            road.SourceGenerator = this;
        }

        StartingRoad.SourceGenerator = this;

        upRoads.AddRange(roads.Where(r => r.GetPrimaryOutput().transform.localPosition.y - r.GetPrimaryInput().transform.localPosition.y > 0f));
        downRoads.AddRange(roads.Where(r => r.GetPrimaryOutput().transform.localPosition.y - r.GetPrimaryInput().transform.localPosition.y < 0f));
        neutralRoads.AddRange(roads.Where(r => r.GetPrimaryOutput().transform.localPosition.y - r.GetPrimaryInput().transform.localPosition.y == 0f));

        var roadMap = new RoadMap(this);

        for (int retryCount = 0; retryCount < 5; retryCount++)
        {
            try
            {
                RoadPiece FindRandomPiece(RoadPiece previous, Transform outputOfPrevious, ref Unity.Mathematics.Random randomizer, out RoadPiece connectedPiece, out Transform connectedPieceOutput)
                {
                    if (outputOfPrevious.transform.position.y > 0f)
                    {
                        var downRoadCopy = new List<RoadPiece>(downRoads);
                        downRoadCopy.AddRange(neutralRoads);
                        //Remove all roads that cannot be placed at the output of previous
                        RemoveAll(downRoadCopy, r => !roadMap.CanPlaceRoadAtPoint(r, r.GetPrimaryInput(), outputOfPrevious));
                        //Remove all roads that that leave no space at the output when placed
                        RemoveAll(downRoadCopy, r => !roadMap.RoadHasExitAtPoint(r, r.GetPrimaryInput(), outputOfPrevious, out var _, out var _));

                        RemoveAll(downRoadCopy, r =>
                        {
                            var height = roadMap.GetRoadPlacementHeightAtPoint(r.GetPrimaryInput(), outputOfPrevious);
                            if (height == 0 && (r.ValidState & RoadPiece.PlacementState.Ground) != RoadPiece.PlacementState.Ground)
                            {
                                return true;
                            }
                            else if (height > 0 && (r.ValidState & RoadPiece.PlacementState.Air) != RoadPiece.PlacementState.Air)
                            {
                                return true;
                            }
                            return false;
                        });

                        {
                            if (downRoadCopy.Count > 0)
                            {
                                var selectedPiece = BucketRandomizer.PickRandomRoadPiece(downRoadCopy, ref randomizer);
                                if (selectedPiece == null)
                                {
                                    connectedPiece = null;
                                    connectedPieceOutput = null;
                                    return null;
                                }
                                roadMap.RoadHasExitAtPoint(selectedPiece, selectedPiece.GetPrimaryInput(), outputOfPrevious, out connectedPiece, out connectedPieceOutput, true);

                                roadMap.GetRoadPosAndRotAtPoint(selectedPiece.GetPrimaryInput(), outputOfPrevious, out var testPos, out var testRot);

                                RoadMap.DebugDrawCube(selectedPiece.GetOccupiedWorldBounds(testPos, testRot), Color.magenta, 10f);

                                foreach (var point in selectedPiece.GetOccupiedWorldPositions(testPos, testRot))
                                {
                                    Debug.DrawLine(point, point + float3(0, 0.1f, 0), Color.yellow, 10f);
                                }

                                return selectedPiece;
                            }
                        }
                    }

                    var upRoadCopy = new List<RoadPiece>(upRoads);

                    upRoadCopy.AddRange(neutralRoads);
                    //Remove all roads that cannot be placed at the output of previous
                    RemoveAll(upRoadCopy, r => !roadMap.CanPlaceRoadAtPoint(r, r.GetPrimaryInput(), outputOfPrevious));
                    //Remove all roads that that leave no space at the output when placed
                    RemoveAll(upRoadCopy, r => !roadMap.RoadHasExitAtPoint(r, r.GetPrimaryInput(), outputOfPrevious, out var _, out var _));
                    RemoveAll(upRoadCopy, r =>
                    {
                        var height = roadMap.GetRoadPlacementHeightAtPoint(r.GetPrimaryInput(), outputOfPrevious);
                        if (height == 0 && (r.ValidState & RoadPiece.PlacementState.Ground) != RoadPiece.PlacementState.Ground)
                        {
                            return true;
                        }
                        else if (height > 0 && (r.ValidState & RoadPiece.PlacementState.Air) != RoadPiece.PlacementState.Air)
                        {
                            return true;
                        }
                        return false;
                    });

                    {
                        var selectedPiece = BucketRandomizer.PickRandomRoadPiece(upRoadCopy, ref randomizer);

                        if (selectedPiece == null)
                        {
                            connectedPiece = null;
                            connectedPieceOutput = null;
                            return null;
                        }
                        roadMap.RoadHasExitAtPoint(selectedPiece, selectedPiece.GetPrimaryInput(), outputOfPrevious, out connectedPiece, out connectedPieceOutput, true);

                        roadMap.GetRoadPosAndRotAtPoint(selectedPiece.GetPrimaryInput(), outputOfPrevious, out var testPos, out var testRot);

                        RoadMap.DebugDrawCube(selectedPiece.GetOccupiedWorldBounds(testPos, testRot), Color.magenta, 10f);

                        return selectedPiece;
                    }
                }



                roadMap.AddRoad(StartingRoad, int3(400, 0, 400), Unity.Mathematics.quaternion.Euler(0f, radians(-90f), 0f), out var startingPiece);

                List<RoadPiece> pieces = new List<RoadPiece>();
                List<Transform> outputs = new List<Transform>();

                pieces.Add(startingPiece);
                outputs.Add(startingPiece.GetPrimaryOutput());

                for (int i = 0; i < 5; i++)
                {
                    roadMap.AddRoadAtPoint(StartingRoad, StartingRoad.GetPrimaryInput(), outputs[outputs.Count - 1], out var newPiece);
                    pieces.Add(newPiece);
                    outputs.Add(newPiece.GetPrimaryOutput());
                }

                startingLineInstance = GameObject.Instantiate(startingLine, outputs[outputs.Count - 1], false);



                GeneratedStartingPoints = new List<Transform>();

                foreach (var piece in pieces)
                {
                    for (int i = 0; i < piece.transform.childCount; i++)
                    {
                        var child = piece.transform.GetChild(i);
                        if (child.name.StartsWith("Start"))
                        {
                            GeneratedStartingPoints.Add(child);
                        }
                    }
                }

                int retractAmount = 20;

                int retryTimes = 5;
                int retryCounter = 0;

                for (int i = 0; i < RoadLength; i++)
                {
                    var randomPiece = FindRandomPiece(pieces[pieces.Count - 1], outputs[outputs.Count - 1], ref randomizer, out var connectedPiece, out var connectedPieceOutput);
                    if (randomPiece == null)
                    {
                        var prevPiece = pieces[pieces.Count - 1];
                        //If there is more than one instance of this piece in the list, don't delete it yet
                        if (pieces.Count(p => p == prevPiece) == 1)
                        {
                            roadMap.RemoveRoad(prevPiece);
                            GameObject.Destroy(prevPiece.gameObject);

                            pieces.RemoveAt(pieces.Count - 1);
                            outputs.RemoveAt(outputs.Count - 1);
                        }
                        else
                        {
                            pieces.RemoveAt(pieces.Count - 1);
                            outputs.RemoveAt(outputs.Count - 1);

                            roadMap.RemoveRoad(pieces[pieces.Count - 1]);
                            GameObject.Destroy(pieces[pieces.Count - 1].gameObject);

                            pieces.RemoveAt(pieces.Count - 1);
                            outputs.RemoveAt(outputs.Count - 1);
                        }
                        i--;
                        retryCounter++;
                        if (retryCounter >= retryTimes)
                        {
                            retryCounter = 0;
                            int backAmount = min(retractAmount, pieces.Count - 1);
                            i -= backAmount - 1;
                            for (int j = backAmount - 1; j >= 0; j--)
                            {
                                if (pieces.Count(p => p == pieces[pieces.Count - 1]) == 1)
                                {
                                    roadMap.RemoveRoad(pieces[pieces.Count - 1]);
                                    GameObject.Destroy(pieces[pieces.Count - 1].gameObject);
                                }
                                pieces.RemoveAt(pieces.Count - 1);
                                outputs.RemoveAt(outputs.Count - 1);
                            }
                        }
                        continue;
                    }
                    roadMap.AddRoadAtPoint(randomPiece, randomPiece.GetPrimaryInput(), outputs[outputs.Count - 1], out var newPiece);

                    if (connectedPiece == null)
                    {
                        pieces.Add(newPiece);
                        outputs.Add(newPiece.GetPrimaryOutput());
                    }
                    else
                    {
                        pieces.Add(newPiece);
                        outputs.Add(newPiece.GetPrimaryOutput());

                        pieces.Add(connectedPiece);
                        outputs.Add(connectedPieceOutput);
                    }
                }

                GeneratedRoads = pieces;

                void ReplaceBlankRoads()
                {
                    List<RoadPiece> uncheckedRoads = new List<RoadPiece>(pieces);

                    for (int i = 0; i < pieces.Count; i++)
                    {
                        var piece = pieces[i];
                        int pairCount = 0;
                        foreach (var pair in pieces[i].GetInOutPairs())
                        {
                            if (pairCount == 0)
                            {
                                pairCount++;
                                continue;
                            }

                            if (piece.Replacements.Count > 0 && !(roadMap.OutputBlockedAtPoint(pair.OutPoint, piece.transform.position, piece.transform.rotation, out var outputRoad) && all(roadMap.AlignToGrid(outputRoad.GetPrimaryInput().transform.position) == roadMap.AlignToGrid(pair.OutPoint.transform.position))))
                            {

                                roadMap.RemoveRoad(piece);
                                GameObject.Destroy(piece.gameObject);

                                var previousPiece = pieces[i - 1];
                                var previousOutput = outputs[i - 1];
                                List<RoadPiece> newPieces = new List<RoadPiece>();
                                List<Transform> newOutputs = new List<Transform>();
                                for (int j = 0; j < piece.Replacements.Count; j++)
                                {
                                    roadMap.AddRoadAtPoint(piece.Replacements[j], piece.Replacements[j].GetPrimaryInput(), previousOutput, out var newPiece);
                                    newPieces.Add(newPiece);
                                    newOutputs.Add(newPiece.GetPrimaryOutput());

                                    previousPiece = newPiece;
                                    previousOutput = newPiece.GetPrimaryOutput();
                                }

                                pieces.RemoveAt(i);
                                outputs.RemoveAt(i);

                                for (int k = 0; k < newPieces.Count; k++)
                                {
                                    pieces.Insert(i + k, newPieces[k]);
                                    outputs.Insert(i + k, newOutputs[k]);
                                }
                            }
                        }
                    }
                }

                ReplaceBlankRoads();

                finishLineInstance = GameObject.Instantiate(finishLine, outputs[outputs.Count - 4], false);

                CheckPoints = outputs.Select(o => o.Find("Checkpoint")).Where(c => c != null).ToList();

                FinalizeSpecials(ref randomizer);

                GenerateBuildings(roadMap, ref randomizer);

                OnMapGenerated?.Invoke();

                break;

            }
            catch (Exception e)
            {
                Debug.LogException(e);

                roadMap.ClearAllRoads();

                if (startingLineInstance != null)
                {
                    GameObject.Destroy(startingLineInstance);
                    startingLineInstance = null;
                }

                if (finishLineInstance != null)
                {
                    GameObject.Destroy(finishLineInstance);
                    finishLineInstance = null;
                }
            }
        }
    }

    void FinalizeSpecials(ref Unity.Mathematics.Random randomizer)
    {
        foreach (var road in GeneratedRoads)
        {
            for (int i = 0; i < road.transform.childCount; i++)
            {
                var specialSpawnPoint = road.transform.GetChild(i);
                if (specialSpawnPoint.name.Contains("Special"))
                {
                    var prefab = BucketRandomizer.PickRandomPiece(SpecialPieces, static p => p.RandomAmount, ref randomizer);
                    if (prefab != null && prefab.Prefab != null)
                    {
                        if (prefab.Prefab.GetComponent<NetworkIdentity>() != null)
                        {
                            if (NetworkServer.active)
                            {
                                var instance = GameObject.Instantiate(prefab.Prefab, specialSpawnPoint, false);

                                instance.transform.SetParent(null, true);

                                NetworkServer.Spawn(instance.gameObject, PlayerManager.OwnedManager.gameObject);
                            }
                        }
                        else
                        {
                            var instance = GameObject.Instantiate(prefab.Prefab, specialSpawnPoint, false);

                            instance.transform.SetParent(null, true);
                        }
                    }
                }
            }
        }
    }

    void GenerateBuildings(RoadMap map, ref Unity.Mathematics.Random randomizer)
    {
        bool OccupiedByRoad(float3 worldPosition)
        {
            const float padding = 0f;

            if (padding == 0f)
            {
                var alignedPos = map.AlignToGrid(worldPosition);
                if (map[alignedPos] != null)
                {
                    return true;
                }
            }
            else
            {
                for (float x = -padding; x <= padding; x += 0.5f)
                {
                    for (float y = -padding; y <= padding; y += 0.5f)
                    {
                        for (float z = -padding; z <= padding; z += 0.5f)
                        {
                            var alignedPos = map.AlignToGrid(worldPosition);
                            if (map[alignedPos] != null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        RoadPiece FindNearestRoad(RoadMap map, float3 position)
        {
            //float xLimit = TileSize.x;
            //float yLimit = TileSize.z;

            for (float limit = TileSize.x; limit < TileSize.x * 10f; limit++)
            {
                for (float x = -limit; x <= limit; x += TileSize.x)
                {
                    for (float z = -limit; z <= limit; z += TileSize.z)
                    {
                        for (float y = 0; y <= TileSize.y * 5f; y++)
                        {
                            var pos = map.AlignToGrid(position + new float3(x,y,z));
                            var road = map[pos];
                            if (road != null)
                            {
                                return road;
                            }
                        }
                    }
                }
            }

            return null;
        }

        HashSet<float3> occupiedBuildingPositions = new HashSet<float3>();

        var roadBounds = GetRoadBounds();

        roadBounds.min -= new Vector3(10f * TileSize.x,0f, 10f * TileSize.z);
        roadBounds.max += new Vector3(10f * TileSize.x, 0f,10f * TileSize.z);

        for (float x = roadBounds.min.x; x <= roadBounds.max.x; x++)
        {
            for (float z = roadBounds.min.z; z <= roadBounds.max.z; z++)
            {
                var randX = randomizer.NextInt(0, (int)(roadBounds.max.x - roadBounds.min.x)) + roadBounds.min.x;
                var randZ = randomizer.NextInt(0, (int)(roadBounds.max.z - roadBounds.min.z)) + roadBounds.min.z;

                var nearestRoad = FindNearestRoad(map, new float3(randX, 0f, randZ));

                Quaternion direction = Quaternion.identity;

                if (nearestRoad != null)
                {
                    direction = Quaternion.LookRotation(normalize(GetDirectionTo(new float3(randX, 0f, randZ), nearestRoad.transform.position)));

                    var degrees = direction.eulerAngles;
                    degrees.y = (degrees.y + 360f) % 360f;

                    if (degrees.y >= 45f && degrees.y < 90f + 45f)
                    {
                        degrees.y = 90f;
                    }
                    else if (degrees.y >= 90f + 45f && degrees.y < 180f + 45f)
                    {
                        degrees.y = 180f;
                    }
                    else if (degrees.y >= 180f + 45f && degrees.y < 270f + 45f)
                    {
                        degrees.y = 270f;
                    }
                    else
                    {
                        degrees.y = 0f;
                    }

                    degrees.y += 180f;

                    degrees.x = 0f;
                    degrees.z = 0f;

                    direction = Quaternion.Euler(degrees);
                }

                List<StructurePiece> possibleStructures = new List<StructurePiece>(Structures);
                while (true)
                {
                    var prefab = BucketRandomizer.PickRandomPiece(possibleStructures, static p => p.RandomChance, ref randomizer);
                    if (prefab != null)
                    {
                        bool occupied = false;
                        var worldPositions = prefab.GetOccupiedWorldPositions(new float3(randX, 0f, randZ) + (float3)(direction * prefab.Center), direction, TileSize).ToArray();
                        foreach (var worldPos in worldPositions)
                        {
                            var alignedPos = map.AlignToGrid(worldPos);
                            if (OccupiedByRoad(worldPos) || occupiedBuildingPositions.TryGetValue(alignedPos, out var _))
                            {
                                occupied = true;
                                break;
                            }
                        }
                        if (occupied)
                        {
                            possibleStructures.Remove(prefab);
                            if (possibleStructures.Count == 0)
                            {
                                break;
                            }
                        }
                        else
                        {
                            foreach (var worldPos in worldPositions)
                            {
                                occupiedBuildingPositions.Add(worldPos);
                            }
                            GameObject.Instantiate(prefab, new float3(randX, 0f, randZ) + (float3)(direction * prefab.Center), direction);
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    static float3 GetDirectionTo(float3 from, float3 to)
    {
        from.y = 0;
        to.y = 0;
        return to - from;
    }

    Bounds GetRoadBounds()
    {
        FindMaxAndMin(GeneratedRoads.Select(static r => (float3)r.transform.position),out var min, out var max);

        return new Bounds
        {
            min = min,
            max = max
        };
    }

    static int RemoveAll(List<RoadPiece> pieces, Predicate<RoadPiece> predicate)
    {
        return pieces.RemoveAll(predicate);
    }

    [Server]
    public void SpawnNewPowerup(Vector3 position, Quaternion rotation, float time)
    {
        IEnumerator Routine()
        {
            yield return new WaitForSeconds(time);
            var powerups = SpecialPieces.Where(s => s.Prefab != null && s.Prefab.name.ToLower().Contains("powerup")).ToList();
            var randomPowerup = powerups[UnityEngine.Random.Range(0, powerups.Count)].Prefab;

            var instance = GameObject.Instantiate(randomPowerup, position, rotation);

            NetworkServer.Spawn(instance.gameObject, PlayerManager.OwnedManager.gameObject);
        }

        StartCoroutine(Routine());
    }


    void FindMaxAndMin(IEnumerable<float3> points, out float3 min, out float3 max)
    {
        min = float3(float.PositiveInfinity);
        max = float3(float.NegativeInfinity);

        foreach (var point in points)
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
    }

}
