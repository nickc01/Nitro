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
using UnityEditor.Experimental.GraphView;
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

    private void Awake()
    {
        //StartGenerating(Seed);
    }



    public void GenerateMap(uint seed)
    {
        if (CurrentState != State.Idle)
        {
            return;
        }
        CurrentState = State.Generating;

        var randomizer = Unity.Mathematics.Random.CreateFromIndex(seed);

        //var mapPiecesCopy = GameObject.Instantiate(mapPiecesContainer);
        //var startingRoadCopy = GameObject.Instantiate(StartingRoad);



        /*var pointCount = randomizer.NextInt(circlePointsMinMax.x, circlePointsMinMax.y);

        float2[] targets = new float2[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            var percentage = i / (float)pointCount;
            var radianAmount = radians(percentage * 360f);
            var pointAlongCircle = float2(cos(radianAmount) * circleSize.x,sin(radianAmount) * circleSize.y);

            //Debug.DrawLine(new Vector3(pointAlongCircle.x, 0f, pointAlongCircle.y), new Vector3(pointAlongCircle.x,2f,pointAlongCircle.y),Color.red,10f);

            var offsetAngle = randomizer.NextFloat(0f, 360f);
            var offsetLength = randomizer.NextFloat(circlePointVarianceMinMax.x, circlePointVarianceMinMax.y);

            var offsetPoint = float2(cos(radians(offsetAngle)), sin(radians(offsetAngle))) * offsetLength;

            targets[i] = pointAlongCircle + offsetPoint + new Unity.Mathematics.float2(200,200);

            Color color = Color.blue;
            float height = 2f;
            if (i == 0)
            {
                height = 5f;
                color = Color.cyan;
            }
            else if (i == 1)
            {
                height = 4f;
                color = Color.white;
            }
            //Debug.DrawLine(new Vector3(targets[i].x, 0f, targets[i].y), new Vector3(targets[i].x, height, targets[i].y),color,10f);
        }*/

        List<RoadPiece> roads = new List<RoadPiece>();

        List<RoadPiece> upRoads = new List<RoadPiece>();
        List<RoadPiece> downRoads = new List<RoadPiece>();
        List<RoadPiece> neutralRoads = new List<RoadPiece>();

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

        RoadPiece FindRandomPiece(RoadPiece previous, Transform outputOfPrevious, ref Unity.Mathematics.Random randomizer, out RoadPiece connectedPiece, out Transform connectedPieceOutput)
        {
            //while (true)
            //{
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


                //while (true)
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
                        //var selectedPiece = downRoadCopy[randomizer.NextInt(0, downRoadCopy.Count)];
                        Debug.Log("1 - Selected Piece = " + selectedPiece);

                        roadMap.RoadHasExitAtPoint(selectedPiece, selectedPiece.GetPrimaryInput(), outputOfPrevious, out connectedPiece, out connectedPieceOutput, true);

                        Debug.Log("1 - Connected Piece = " + connectedPiece);
                        Debug.Log("1 - Output Piece = " + outputOfPrevious.parent);

                        roadMap.GetRoadPosAndRotAtPoint(selectedPiece.GetPrimaryInput(), outputOfPrevious, out var testPos, out var testRot);


                        /*var nextRoadPos = roadMap.GetNextPointAtOutputPos(selectedPiece.GetPrimaryOutput(), testPos, testRot, 1.4f);

                        Debug.DrawLine(nextRoadPos, testPos, Color.cyan, 10f);

                        if (roadMap[roadMap.AlignToGrid(nextRoadPos)] != null)
                        {
                            Debug.Log("2 - ROAD BLOCKED");
                            if (downRoadCopy.Count > 1)
                            {
                                downRoadCopy.Remove(selectedPiece);
                                continue;
                            }
                        }*/

                        /*var localOutput = selectedPiece.GetPrimaryOutput().transform.localPosition;

                        if (roadMap.OutputBlockedAtPoint(selectedPiece.GetPrimaryOutput(), testPos, testRot, out _))
                        {
                            Debug.Log("1 - ROAD BLOCKED");
                            if (downRoadCopy.Count > 1)
                            {
                                continue;
                            }
                        }*/

                        //roadMap.RoadHasExitAtPoint(selectedPiece, selectedPiece.GetPrimaryInput(), outputOfPrevious, out connectedPiece, out connectedPieceOutput, true);

                        RoadMap.DebugDrawCube(selectedPiece.GetOccupiedWorldBounds(testPos, testRot), Color.magenta, 10f);

                        foreach (var point in selectedPiece.GetOccupiedWorldPositions(testPos, testRot))
                        {
                            Debug.DrawLine(point, point + float3(0, 0.1f, 0), Color.yellow, 10f);
                            //Debug.Log("Point = " + point);
                        }

                        return selectedPiece;
                    }
                }
            }

            var upRoadCopy = new List<RoadPiece>(upRoads);

            upRoadCopy.AddRange(neutralRoads);
            //Remove all roads that cannot be placed at the output of previous
            Debug.Log("R1");
            RemoveAll(upRoadCopy, r => !roadMap.CanPlaceRoadAtPoint(r, r.GetPrimaryInput(), outputOfPrevious));
            //Remove all roads that that leave no space at the output when placed
            Debug.Log("R2");
            RemoveAll(upRoadCopy, r => !roadMap.RoadHasExitAtPoint(r, r.GetPrimaryInput(), outputOfPrevious, out var _, out var _));
            Debug.Log("R3");
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

            //while (true)
            {
                var selectedPiece = BucketRandomizer.PickRandomRoadPiece(upRoadCopy, ref randomizer);

                if (selectedPiece == null)
                {
                    connectedPiece = null;
                    connectedPieceOutput = null;
                    return null;
                }
                //var selectedPiece = upRoadCopy[randomizer.NextInt(0, upRoadCopy.Count)];
                Debug.Log("2 - Selected Piece = " + selectedPiece);

                roadMap.RoadHasExitAtPoint(selectedPiece, selectedPiece.GetPrimaryInput(), outputOfPrevious, out connectedPiece, out connectedPieceOutput, true);

                Debug.Log("2 - Connected Piece = " + connectedPiece);
                Debug.Log("2 - Connected Output = " + connectedPieceOutput);
                Debug.Log("2 - Output Piece = " + outputOfPrevious.parent);

                roadMap.GetRoadPosAndRotAtPoint(selectedPiece.GetPrimaryInput(), outputOfPrevious, out var testPos, out var testRot);

                /*var nextRoadPos = roadMap.GetNextPointAtOutputPos(selectedPiece.GetPrimaryOutput(), testPos, testRot, 1.4f);

                Debug.DrawRay(nextRoadPos, new Vector3(0f, 0.05f, 0f), Color.cyan, 10f);

                if (roadMap[roadMap.AlignToGrid(nextRoadPos)] != null)
                {
                    Debug.Log("2 - ROAD BLOCKED");
                    if (upRoadCopy.Count > 1)
                    {
                        upRoadCopy.Remove(selectedPiece);
                        continue;
                    }
                }*/

                RoadMap.DebugDrawCube(selectedPiece.GetOccupiedWorldBounds(testPos, testRot), Color.magenta, 10f);

                foreach (var point in selectedPiece.GetOccupiedWorldPositions(testPos, testRot))
                {
                    Debug.DrawLine(point, point + float3(0, 0.1f, 0), Color.yellow, 10f);
                    //Debug.Log("Point = " + point);
                }

                return selectedPiece;
            }

            /*var copy = new List<RoadPiece>(roads);
            Debug.Log("STEP 1");
            //RemoveAll(copy, r => r.MaxBounds.y > 0f);

            Debug.Log("STEP 2");
            RemoveAll(copy, r => !roadMap.CanPlaceRoadAtPoint(r, r.GetPrimaryInput(), outputOfPrevious));

            //copy.RemoveAll(r => !map.CanPlaceRoadAtPoint(r,r.GetPrimaryInput(),outputOfPrevious));
            Debug.Log("STEP 3");
            RemoveAll(copy, r => !roadMap.RoadHasExitAtPoint(r, r.GetPrimaryInput(), outputOfPrevious, out var connectedPiece));

            Debug.Log("Copy Size = " + copy.Count);

            return copy[randomizer.NextInt(0, copy.Count)];*/
            //}
        }



        roadMap.AddRoad(StartingRoad, int3(400, 0, 400), Unity.Mathematics.quaternion.Euler(0f,radians(-90f),0f),out var startingPiece);

        //TODO TODO TODO
        //roadMap.OutputBlockedAtPoint(startingPiece.GetPrimaryOutput(), startingPiece.transform.position, startingPiece.transform.rotation);

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

        GameObject.Instantiate(startingLine, outputs[outputs.Count - 1],false);

        

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

        //RoadPiece previousPiece = startingPiece;
        //Transform previousPieceOutput = startingPiece.GetPrimaryOutput();
        int retractAmount = 20;

        int retryTimes = 5;
        int retryCounter = 0;

        for (int i = 0; i < RoadLength; i++)
        {
            var randomPiece = FindRandomPiece(pieces[pieces.Count - 1], outputs[outputs.Count - 1], ref randomizer,out var connectedPiece, out var connectedPieceOutput);
            if (randomPiece == null)
            {
                var prevPiece = pieces[pieces.Count - 1];
                //If there is more than one instance of this piece in the list, don't delete it yet
                //Debug.Log("COUNT = " + pieces.Count(p => p == prevPiece));
                if (pieces.Count(p => p == prevPiece) == 1)
                {
                    //Debug.Log("1 - REMOVING = " + prevPiece);
                    roadMap.RemoveRoad(prevPiece);
                    GameObject.Destroy(prevPiece.gameObject);

                    pieces.RemoveAt(pieces.Count - 1);
                    outputs.RemoveAt(outputs.Count - 1);
                    //GameObject.Destroy(prevPiece.gameObject);
                }
                else
                {
                    //Debug.Log("COUNT = " + pieces.Count(p => p == prevPiece));
                    pieces.RemoveAt(pieces.Count - 1);
                    outputs.RemoveAt(outputs.Count - 1);

                    //Debug.Log("2 - REMOVING = " + pieces[pieces.Count - 1]);
                    roadMap.RemoveRoad(pieces[pieces.Count - 1]);
                    GameObject.Destroy(pieces[pieces.Count - 1].gameObject);

                    pieces.RemoveAt(pieces.Count - 1);
                    outputs.RemoveAt(outputs.Count - 1);
                }
                i--;
                retryCounter++;
                if (retryCounter >= retryTimes)
                {
                    //Debug.Log("LARGE BACKTRACK");
                    retryCounter = 0;
                    int backAmount = min(retractAmount, pieces.Count - 1);
                    i -= backAmount - 1;
                    for (int j = backAmount - 1; j >= 0; j--)
                    {
                        //Debug.Log("3 - REMOVING = " + pieces[pieces.Count - 1]);
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

            //SpawnSpecialPieces(newPiece, ref randomizer);

            //previousPieceOutput = previousPiece.GetPrimaryOutput();

            if (connectedPiece == null)
            {
                pieces.Add(newPiece);
                outputs.Add(newPiece.GetPrimaryOutput());
                //previousPieceOutput = previousPiece.GetPrimaryOutput();
            }
            else
            {
                pieces.Add(newPiece);
                outputs.Add(newPiece.GetPrimaryOutput());

                pieces.Add(connectedPiece);
                outputs.Add(connectedPieceOutput);
                //previousPiece = connectedPiece;
                //previousPieceOutput = connectedPieceOutput;

                //Debug.DrawRay(previousPieceOutput.position, new Vector3(0f, 5f, 0f), Color.black, 10f);

                /*while (true)
                {
                    roadMap.RoadHasExitAtPoint(previousPiece, selectedPiece.GetPrimaryInput(), outputOfPrevious, out connectedPiece, out connectedPieceOutput, true);
                }*/
                
            }
        }

        GeneratedRoads = pieces;

        void ReplaceBlankRoads()
        {
            Debug.Log("REPLACING BLACK ROADS");
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

                    Debug.Log("CHECKING = " + piece.gameObject);
                    if (piece.Replacements.Count > 0 && !(roadMap.OutputBlockedAtPoint(pair.OutPoint, piece.transform.position,piece.transform.rotation,out var outputRoad) && all(roadMap.AlignToGrid(outputRoad.GetPrimaryInput().transform.position) == roadMap.AlignToGrid(pair.OutPoint.transform.position))))
                    {

                        roadMap.RemoveRoad(piece);
                        GameObject.Destroy(piece.gameObject);
                        //Change piece with replacements
                        //TODO TODO TODO TODO TODO TODO
                        //pieces.inSert

                        Debug.Log("REPLACING = " + piece.gameObject);
                        var previousPiece = pieces[i - 1];
                        var previousOutput = outputs[i - 1];
                        List<RoadPiece> newPieces = new List<RoadPiece>();
                        List<Transform> newOutputs = new List<Transform>();
                        for (int j = 0; j < piece.Replacements.Count; j++)
                        {
                            roadMap.AddRoadAtPoint(piece.Replacements[j], piece.Replacements[j].GetPrimaryInput(), previousOutput, out var newPiece);
                            //SpawnSpecialPieces(newPiece, ref randomizer);
                            //pieces.Add(newPiece);
                            //outputs.Add(newPiece.GetPrimaryOutput());
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

        GameObject.Instantiate(finishLine, outputs[outputs.Count - 4], false);

        CheckPoints = outputs.Select(o => o.Find("Checkpoint")).Where(c => c != null).ToList();

        FinalizeSpecials(ref randomizer);

        GenerateBuildings(roadMap, ref randomizer);

        OnMapGenerated?.Invoke();

        //AddLODRenderers();

        //yield break;
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
                    //var specialSpawnPoint = child.GetChild(0);

                    var prefab = BucketRandomizer.PickRandomPiece(SpecialPieces, static p => p.RandomAmount, ref randomizer);
                    if (prefab != null && NetworkServer.active)
                    {
                        var instance = GameObject.Instantiate(prefab.Prefab, specialSpawnPoint, false);

                        instance.transform.SetParent(null, true);

                        NetworkServer.Spawn(instance.gameObject, PlayerManager.OwnedManager.gameObject);
                        /*if (instance.GetComponent<NetworkIdentity>() != null)
                        {
                            if (NetworkServer.active)
                            {
                                NetworkServer.Spawn(instance.gameObject, PlayerManager.OwnedManager.gameObject);
                            }
                            else
                            {
                                Destroy(instance.gameObject);
                            }
                        }*/
                    }
                    //specialSpaces.Add(child);
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

            /*for (float x = -TileSize.x; x <= TileSize.x * 10f; x = -x)
            {
                for (float z = -TileSize.z; z <= TileSize.z * 10f; z = -z)
                {
                    for (float y = 0; y <= TileSize.y * 5f; y++)
                    {

                    }

                    if (z > 0)
                    {
                        z += TileSize.z;
                        z = -z;
                    }
                }

                if (x > 0)
                {
                    x += TileSize.x;
                    x = -x;
                }
            }*/
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

                /*for (float y = 0; y < TileSize.y * 5; y += TileSize.y)
                {
                    if (map[map.AlignToGrid(new float3(x, y, z))] != null)
                    {
                        occupied = true;
                        break;
                    }
                }*/
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
                        //GameObject.Instantiate(prefab, new Vector3(x, 0f, z), Quaternion.identity);
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

    /*static float ClampToCardinalDirection()
    {

    }*/

    Bounds GetRoadBounds()
    {
        FindMaxAndMin(GeneratedRoads.Select(static r => (float3)r.transform.position),out var min, out var max);

        return new Bounds
        {
            min = min,
            max = max
        };
    }

    /*void SpawnSpecialPieces(RoadPiece spawnedPiece, ref Unity.Mathematics.Random randomizer)
    {
        List<Transform> specialSpaces = new List<Transform>();

        for (int i = 0; i < spawnedPiece.transform.childCount; i++)
        {
            var child = spawnedPiece.transform.GetChild(i);
            if (child.name.Contains("Special"))
            {
                specialSpaces.Add(child);
            }
        }

        foreach (var space in specialSpaces)
        {
            var prefab = BucketRandomizer.PickRandomPiece(SpecialPieces,static p => p.RandomAmount,ref randomizer);

            if (prefab != null)
            {
                GameObject.Instantiate(prefab.Prefab, space, false);
            }
        }
    }*/

    static int RemoveAll(List<RoadPiece> pieces, Predicate<RoadPiece> predicate)
    {
        foreach (var piece in pieces)
        {
            if (predicate(piece))
            {
                //Debug.Log("Removing Piece = " + piece.name);
            }
        }
        return pieces.RemoveAll(predicate);
    }

    /*void PlaceRoadAtOutput(RoadPiece prefab, Transform outputPoint)
    {
        var pairs = prefab.GetInOutPairs().ToList();

        var inPoint = pairs[0].InPoint;
    }*/

    void AddLODRenderers()
    {
        List<Renderer> renderers = new List<Renderer>();
        foreach (var piece in GeneratedRoads)
        {
            renderers.Add(piece.GetComponent<Renderer>());

            foreach (var pillar in piece.GetPillars())
            {
                renderers.Add(pillar.GetComponent<Renderer>());
            }
        }

        var lods = LOD.GetLODs(); //[0].renderers = renderers.ToArray();

        lods[0].renderers = renderers.ToArray();

        LOD.SetLODs(lods);
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
