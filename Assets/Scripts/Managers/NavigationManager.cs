using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationManager
{
    #region Internal nav classes
    internal class NavNodeComparar : IComparer<NavNode>
    {
        int IComparer<NavNode>.Compare(NavNode x, NavNode y)
        {
            if (x.Data == y.Data)
            {
                return 0;
            }
            else
            {
                int comparison = x.FScore.CompareTo(y.FScore);
                if (comparison == 0)
                {
                    return -1;
                }
                else
                {
                    return comparison;
                }
            }
        }
    }

    internal class NavNode
    {
        public int FScore
        {
            get { return GScore + HScore; }
        }

        public int GScore { get; set; }
        public int HScore { get; set; }

        public NavNode Parent { get; set; }
        public Vector2Int Data { get; set; }

        public NavNode(int hScore, int gScore, Vector2Int data)
        {
            HScore = hScore;
            GScore = gScore;
            Data = data;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (this.GetType() != obj.GetType())
            {
                return false;
            }

            NavNode other = (NavNode)obj;

            return Data == other.Data;
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public static bool operator ==(NavNode a, NavNode b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return object.ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(NavNode a, NavNode b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return !object.ReferenceEquals(b, null);
            }

            return !a.Equals(b);
        }
    }

    internal class NavigationNode<T>
    {
        public int index;
        public int FScore
        {
            get { return GScore + HScore; }
        }

        public int GScore { get; set; }
        public int HScore { get; set; }

        public NavigationNode<T> Parent { get; set; }
        public T Data { get; set; }

        public NavigationNode(int hScore, int gScore, T data)
        {
            HScore = hScore;
            GScore = gScore;
            Data = data;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (this.GetType() != obj.GetType())
            {
                return false;
            }

            NavigationNode<T> other = (NavigationNode<T>)obj;

            return Data.Equals(other.Data);
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public static bool operator ==(NavigationNode<T> a, NavigationNode<T> b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return object.ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(NavigationNode<T> a, NavigationNode<T> b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return !object.ReferenceEquals(b, null);
            }

            return !a.Equals(b);
        }
    }
    #endregion

    public List<MapNode> AStar(MapNode start, MapNode target, out float distance)
    {
        distance = 0;
        if (start == target)
        {
            return new List<MapNode>();
        }

        List<Vector2Int> result = new List<Vector2Int>();
        HashSet<NavigationNode<MapNode>> closedSet = new HashSet<NavigationNode<MapNode>>();
        HashSet<NavigationNode<MapNode>> openSet = new HashSet<NavigationNode<MapNode>>() {
            new NavigationNode<MapNode>((int)(target.Cell.center - start.Cell.center).sqrMagnitude, 0, start) };

        while (openSet.Count > 0)
        {
            NavigationNode<MapNode> current = openSet.Aggregate((x, y) => x.FScore < y.FScore ? x : y);
            if (current.Data.Equals(target))
            {
                return UnwrapPath(current, out distance);
            }

            openSet.Remove(current);
            closedSet.Add(current);
            List<Tuple<MapNode, int>> neighbours = current.Data.Corridors.Aggregate(
                new List<Tuple<MapNode, int>>(), (list, y) =>
            {
                if (!y.Point1.Data.Equals(current.Data))
                {
                    list.Add(new Tuple<MapNode, int>(y.Point1.Data, (int)y.DistanceSquared));
                }

                if (!y.Point2.Data.Equals(current.Data))
                {
                    list.Add(new Tuple<MapNode, int>(y.Point2.Data, (int)y.DistanceSquared));
                }

                return list;
            });

            neighbours.ForEach(x =>
            {
                if (!closedSet.Any(closed => closed.Data.Equals(x.Item1)))
                {
                    if (!openSet.Any(open => open.Data.Equals(x.Item1)))
                    {
                        NavigationNode<MapNode> node = new NavigationNode<MapNode>((int)(
                            target.Cell.center - x.Item1.Cell.center).sqrMagnitude, x.Item2, x.Item1);
                        node.Parent = current;
                        openSet.Add(node);
                    }
                }
            });
        }

        return null;
    }

    public List<Vector2Int> AStar(Vector2Int origin, Vector2Int destination, in Map map, out float distance)
    {
        Vector2Int start = origin;
        Vector2Int target = destination;

        distance = float.NegativeInfinity;
        if (start == target)
        {
            distance = 0.0f;
            return new List<Vector2Int>();
        }

        int collisionIndex = map.GetCollisionIndex(target.x, target.y);
        if (collisionIndex != 0)
        {
            return new List<Vector2Int>();
        }

        List<Vector2Int> result = new List<Vector2Int>();

        HashSet<NavNode> closedSet = new HashSet<NavNode>();
        SortedSet<NavNode> openSet = new SortedSet<NavNode>(new NavNodeComparar()) {
            new NavNode(Utility.ManhattanDistance(target, start), 1, start) };

        while (openSet.Count > 0)
        {
            NavNode current = openSet.ElementAt(0);

            if (current.Data == target)
            {
                result = UnwrapPath(current, out distance);
                result.Reverse();
                break;
            }

            openSet.Remove(current);
            closedSet.Add(current);
            List<NavNode> neighbours = GetNeighbours(current, false, map);

            for (int i = 0; i < neighbours.Count; i++)
            {
                if (closedSet.Contains(neighbours[i]))
                {
                    continue;
                }

                if (openSet.Contains(neighbours[i]))
                {
                    continue;
                }

                neighbours[i].Parent = current;
                neighbours[i].HScore = Utility.ManhattanDistance(target, neighbours[i].Data);
                neighbours[i].GScore = current.GScore + 1;
                openSet.Add(neighbours[i]);
            }
        }

        return result;
    }

    private List<NavNode> GetNeighbours(NavNode node, bool allowDiagonal, in Map map)
    {
        List<NavNode> result = new List<NavNode>();
        if (allowDiagonal)
        {
            for (int x = node.Data.x - 1; x <= node.Data.x + 1; x++)
            {
                for (int y = node.Data.y - 1; y <= node.Data.y + 1; y++)
                {
                    if (new Vector2Int(x, y) == node.Data || map.GetCollisionIndex(x, y) != 0)
                    {
                        continue;
                    }
        
                    result.Add(new NavNode(x, y, new Vector2Int(x, y)));
                }
            }
        }
        else
        {
            if (map.GetCollisionIndex(node.Data.x - 1, node.Data.y) <= 0)
            {
                result.Add(new NavNode(0, 0, new Vector2Int(node.Data.x - 1, node.Data.y)));
            }

            if (map.GetCollisionIndex(node.Data.x + 1, node.Data.y) <= 0)
            {
                result.Add(new NavNode(0, 0, new Vector2Int(node.Data.x + 1, node.Data.y)));
            }

            if (map.GetCollisionIndex(node.Data.x, node.Data.y - 1) <= 0)
            {
                result.Add(new NavNode(0, 0, new Vector2Int(node.Data.x, node.Data.y - 1)));
            }

            if (map.GetCollisionIndex(node.Data.x, node.Data.y + 1) <= 0)
            {
                result.Add(new NavNode(0, 0, new Vector2Int(node.Data.x, node.Data.y + 1)));
            }
        }

        return result;
    }


    private List<MapNode> UnwrapPath(NavigationNode<MapNode> node, out float distance)
    {
        distance = 0;
        List<MapNode> result = new List<MapNode>();
        while (node != null)
        {
            distance += node.FScore;
            result.Add(node.Data);
            node = node.Parent;
        }
        return result;
    }

    private List<Vector2Int> UnwrapPath(NavNode node, out float distance)
    {
        distance = 0.0f;
        List<Vector2Int> result = new List<Vector2Int>();
        while (node != null)
        {
            distance += node.GScore;
            result.Add(node.Data);
            node = node.Parent;
        }

        return result;
    }
}
