using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    private class Node
    {
        public Vector2Int position;
        public int gCost; // Distance from start
        public int hCost; // Estimated distance to goal (heuristic)
        public Node parent;

        public int fCost
        {
            get { return gCost + hCost; }
        }

        public Node(Vector2Int pos)
        {
            position = pos;
        }
    }

    // Direction vectors for 4 directions (up, right, down, left)
    private static readonly Vector2Int[] directions = {
        new Vector2Int(0, 1),  // Up
        new Vector2Int(1, 0),  // Right
        new Vector2Int(0, -1), // Down
        new Vector2Int(-1, 0)  // Left
    };

    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int goalPos, int[,] gridValue)
    {


        Node startNode = new Node(startPos);
        Node goalNode = new Node(goalPos);

        List<Node> openSet = new List<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Get node with lowest F cost
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.position);

            // Found the goal
            if (currentNode.position == goalPos)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = currentNode.position + dir;

                if (neighborPos.x < 0 || neighborPos.x >= gridValue.GetLength(0) ||
                    neighborPos.y < 0 || neighborPos.y >= gridValue.GetLength(1) ||
                    gridValue[neighborPos.x, neighborPos.y] == (int)ETypeFloor.Wall ||
                    closedSet.Contains(neighborPos))
                {
                    continue;
                }

                int newGCost = currentNode.gCost + 10;

                Node neighborNode = openSet.Find(n => n.position == neighborPos);

                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos);
                    neighborNode.gCost = newGCost;
                    neighborNode.hCost = GetDistance(neighborPos, goalPos);
                    neighborNode.parent = currentNode;
                    openSet.Add(neighborNode);
                }
                else if (newGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = newGCost;
                    neighborNode.parent = currentNode;
                }
            }
        }

        // No path found
        Debug.LogWarning("No path found to goal!");
        return new List<Vector2Int>();
    }

    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode.position != startNode.position)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        int dstX = Mathf.Abs(a.x - b.x);
        int dstY = Mathf.Abs(a.y - b.y);

        // Manhattan distance for 4-direction movement
        return 10 * (dstX + dstY);
    }
}