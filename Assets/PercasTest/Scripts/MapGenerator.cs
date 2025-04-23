using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private GameObject floorPrefabs;
    [SerializeField] private Transform mapItemTransform;
    private FloorControl[,] gridFloor;//arrayfloor
    private int[,] gridValue;//array value
    List<GameObject> listFloor = new List<GameObject>();
    public Pathfinder pathfinder;

    List<Vector2Int> pathWay;

    [Header("MAP PROPERTIES")]
    public int width = 20;
    public int height = 20;

    [Header("NPC POSITION")]
    [SerializeField] private bool setNPCPosition = false;
    [SerializeField, ShowIf("setNPCPosition")]
    public Vector2Int currentNPCPosition = Vector2Int.zero;

    [Header("TARGET POSITION")]
    [SerializeField] private bool setTargetPosition = false;
    [SerializeField, ShowIf("setTargetPosition"), MinValue(0), MaxValue("@height - 1")]
    public Vector2Int currentTargetPosition = Vector2Int.zero;

    [PropertySpace(20, 20)]
    public ETypeCreatePath typeGuaranteedPath;

    [Range(0.1f, 0.9f)]
    public float wallPercentage = 0.3f; //wall appearance rate

    [Button("Generate Map"), PropertySpace(20)]
    public void GenerateMap()
    {
        if (listFloor != null || listFloor.Count > 0)
            ClearMap();
        pathWay = new List<Vector2Int>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = width;

        InitGrid();

        GenMapValue();

        RenderMap();
    }


    public void InitGrid()
    {
        gridFloor = new FloorControl[height, width];
        gridValue = new int[height, width];

        for (int x = 0; x < height; x++) // row
        {
            for (int y = 0; y < width; y++) // column
            {
                var go = Instantiate(floorPrefabs, mapItemTransform);
                gridFloor[x, y] = go.GetComponent<FloorControl>();
                gridValue[x, y] = 0;
                listFloor.Add(go);
            }
        }
    }

    public void ClearMap()
    {
        for (int i = 0; i < listFloor.Count; i++)
        {
            DestroyImmediate(listFloor[i]);
        }
        gridFloor = null;
        gridValue = null;
        listFloor.Clear();
    }

    public void GenMapValue()
    {
        if (gridFloor == null || gridFloor.Length == 0) return;

        // First, randomize tiles to create walls
        if (typeGuaranteedPath != ETypeCreatePath.Disable)
        {
            RandomizeTiles();
        }

        // Set NPC position in a non-wall position
        if (!setNPCPosition)
        {
            do
            {
                currentNPCPosition = new Vector2Int(Random.Range(0, height), Random.Range(0, width));
            }
            while (gridValue[currentNPCPosition.x, currentNPCPosition.y] == (int)ETypeFloor.Wall);
        }
        gridValue[currentNPCPosition.x, currentNPCPosition.y] = (int)ETypeFloor.NPC;

        // Set target position in a non-wall position
        if (!setTargetPosition || isAdjacentPosition())
        {
            do
            {
                currentTargetPosition = new Vector2Int(Random.Range(0, height), Random.Range(0, width));
            }
            while (isAdjacentPosition() || gridValue[currentTargetPosition.x, currentTargetPosition.y] == (int)ETypeFloor.Wall);
        }
        gridValue[currentTargetPosition.x, currentTargetPosition.y] = (int)ETypeFloor.Target;

        // Find path based on selected type
        switch (typeGuaranteedPath)
        {
            case ETypeCreatePath.SimplePath:
                CreateGuaranteedPath();
                break;
            case ETypeCreatePath.RandomPath:
                CreateRandomPath();
                break;
            case ETypeCreatePath.Disable:
                break;
        }
        ClearPath();
    }

    bool isAdjacentPosition()
    {
        return currentTargetPosition == currentNPCPosition ||//avoid duplicate
                Mathf.Abs(currentTargetPosition.x - currentNPCPosition.x) <= 1 &&//avoid 2 adjacent positions
                Mathf.Abs(currentTargetPosition.y - currentNPCPosition.y) <= 1;
    }

    void CreateGuaranteedPath()
    {

        Vector2Int current = currentNPCPosition;

        while (current.x != currentTargetPosition.x)
        {
            if (current.x < currentTargetPosition.x)
                current.x++;
            else
                current.x--;

            if (current != currentTargetPosition && current != currentNPCPosition)
            {
                gridValue[current.x, current.y] = (int)ETypeFloor.Path;
                pathWay.Add(current);
            }
        }

        while (current.y != currentTargetPosition.y)
        {
            if (current.y < currentTargetPosition.y)
                current.y++;
            else
                current.y--;
            if (current != currentTargetPosition && current != currentNPCPosition)
            {
                gridValue[current.x, current.y] = (int)ETypeFloor.Path;
                pathWay.Add(current);
            }
        }
    }

    void CreateRandomPath()
    {
        Vector2Int current = currentNPCPosition;
        pathWay = new List<Vector2Int>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1)
        };

        int maxSteps = (width + height) * 2;
        int steps = 0;

        while (current != currentTargetPosition && steps < maxSteps)
        {
            Vector2Int nextDirection;

            if (Random.value < 0.7f)
            {
                int xDir = (currentTargetPosition.x > current.x) ? 0 : ((currentTargetPosition.x < current.x) ? 2 : -1);
                int yDir = (currentTargetPosition.y > current.y) ? 1 : ((currentTargetPosition.y < current.y) ? 3 : -1);

                if (xDir >= 0 && yDir >= 0)
                {
                    nextDirection = (Random.value < 0.5f) ? directions[xDir] : directions[yDir];
                }
                else if (xDir >= 0)
                {
                    nextDirection = directions[xDir];
                }
                else if (yDir >= 0)
                {
                    nextDirection = directions[yDir];
                }
                else
                {
                    nextDirection = directions[Random.Range(0, directions.Length)];
                }
            }
            else
            {
                nextDirection = directions[Random.Range(0, directions.Length)];
            }

            Vector2Int next = current + nextDirection;
            if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height)
            {
                current = next;
                if (gridValue[current.x, current.y] != 2 && gridValue[current.x, current.y] != 3)
                {
                    gridValue[current.x, current.y] = (int)ETypeFloor.Path;
                    pathWay.Add(current);
                }
            }

            steps++;
        }
    }

    void ClearPath()
    {
        if (pathWay.Count > 0)
        {
            for (int i = 0; i < pathWay.Count; i++)
            {
                gridValue[pathWay[i].x, pathWay[i].y] = 0;
            }
        }
        pathWay.Clear();
    }

    [Button("Find And Show Path")]
    public void Test()
    {
        FindAndShowPath();
        RenderMap();
    }

    void FindAndShowPath()
    {
        if (pathfinder == null) return;
        ClearPath();
        pathWay = pathfinder.FindPath(currentNPCPosition, currentTargetPosition, gridValue);

        // If path is found, show it
        if (pathWay.Count > 0)
        {
            foreach (var pos in pathWay)
            {
                if (pos != currentTargetPosition && pos != currentNPCPosition)
                    gridValue[pos.x, pos.y] = (int)ETypeFloor.Path;
            }
        }
        else
        {
            Debug.LogError("No valid path found between NPC and Target!");
        }
    }

    void RandomizeTiles()
    {
        int wallCount = Mathf.FloorToInt((width * height - pathWay.Count) * wallPercentage);
        int placedWalls = 0;

        while (placedWalls < wallCount)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            if (gridValue[x, y] == 0)
            {
                gridValue[x, y] = (int)ETypeFloor.Wall;
                placedWalls++;
            }
        }
    }

    public void RenderMap()
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                gridFloor[x, y].SetTypeFloor((ETypeFloor)gridValue[x, y]);
                gridFloor[x, y].SetPosFloor(x, y);
            }
        }
    }
}

public enum ETypeCreatePath
{
    SimplePath, //shortest path from npc to target
    RandomPath, //path random to target
    Disable //generate random path, may not have path to tagert
}