using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System;

using AStar_2D.Pathfinding;

namespace AStar_2D
{
    /// <summary>
    /// Represents a series of nodes that make up a path to the destination.
    /// Provides additional helper methods for path traversal.
    /// </summary>
    public sealed class Path : IEnumerable<PathRouteNode>
    {
        // Private       
        private SearchGrid grid = null;
        private List<PathRouteNode> nodes = new List<PathRouteNode>();
        private PathRouteNode lastNode = null;
        private PathRouteNode startNode = null;

        private Vector3[] cachedVectorArray = null;
        private Index[] cachedIndexArray = null;

        // Properties
        /// <summary>
        /// Can the path be reached.
        /// </summary>
        public bool IsReachable
        {
            get { return allNodesWalkable(); }
        }

        /// <summary>
        /// Can the path be reached.
        /// This property will also ensure that no <see cref="DynamicObstacle"/> are blocking the path 
        /// </summary>
        public bool IsFullyReachable
        {
            get { return allNodesWalkable(true); }
        }

        /// <summary>
        /// Does the path contain any nodes.
        /// </summary>
        public bool IsEmpty
        {
            get { return nodes.Count == 0; }
        }

        /// <summary>
        /// The number of nodes that make up this path.
        /// </summary>
        public int NodeCount
        {
            get { return nodes.Count; }
        }

        /// <summary>
        /// The first node in the path.
        /// </summary>
        public PathRouteNode StartNode
        {
            get { return startNode; }
        }

        /// <summary>
        /// The last node in the path.
        /// </summary>
        public PathRouteNode LastNode
        {
            get { return lastNode; }
        }

        // Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        internal Path(SearchGrid grid)
        {
            this.grid = grid;
        }

        // Methods
        internal void push(IPathNode node, Index index)
        {
            // COnstruct the route node
            PathRouteNode route = new PathRouteNode(node, index);

            // Set the start node
            if (IsEmpty == true)
                startNode = route;

            // Add the node
            lastNode = route;

            // Push a node to the back
            nodes.Add(route);
        }

        /// <summary>
        /// Attempts to get the <see cref="PathRouteNode"/> at the specified index. 
        /// </summary>
        /// <param name="index">The index of the node</param>
        /// <returns>A <see cref="PathRouteNode"/> or null if the index was invalid</returns>
        public PathRouteNode getNode(int index)
        {
            // Validate index
            if (index >= 0 && index < NodeCount)
                return nodes[index];

            // Node not found
            return null;
        }

        /// <summary>
        /// Returns an array of <see cref="Vector3"/> representing each world position waypoint in the path.
        /// The first call will create a cached array which will be returned in subsequent calls.
        /// </summary>
        /// <returns>An array of world position waypoints</returns>
        public Vector3[] ToVectorArray()
        {
            // Check for a cached array
            if (cachedVectorArray == null)
            {
                List<Vector3> list = new List<Vector3>();

                // Add to the list
                foreach (IPathNode node in this)
                    list.Add(node.WorldPosition);

                // Get as array
                cachedVectorArray = list.ToArray();
            }

            // Use the cached version
            return cachedVectorArray;
        }

        /// <summary>
        /// Returns an array of <see cref="Index"/> representing the index of each waypoint node in the path.
        /// The first call will create a cached array which will be returned in subsequent calls.
        /// </summary>
        /// <returns>An array of grid indexes representing this path</returns>
        public Index[] ToIndexArray()
        {
            // Check for a cached array
            if (cachedIndexArray == null)
            {
                List<Index> list = new List<Index>();

                // Add to the list
                foreach (PathRouteNode node in this)
                    list.Add(node.Index);

                // Get as array
                cachedIndexArray = list.ToArray();
            }

            // Use the cached version
            return cachedIndexArray;
        }

        private bool allNodesWalkable(bool checkDynamicObstacles = false)
        {
            // If any path is not walkable then exits early with failure
            foreach (PathRouteNode node in nodes)
            {
                if (node.IsWalkable == false)
                    return false;

                // Check for dynamic obstacles
                if(checkDynamicObstacles == true)
                {
                    // Check for valid grid
                    if(grid != null && grid.CheckIndexOccupied != null)
                    {
                        // Check if the node is obstructed
                        if (grid.CheckIndexOccupied(node.Index) == true)
                            return false;
                    }
                }
            }
            
            // The path can be walked until the end
            return true;
        }

        /// <summary>
        /// Overriden to string method.
        /// </summary>
        /// <returns>This <see cref="Path"/> as a string representation</returns>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Additional to string method.
        /// </summary>
        /// <param name="detailed">Should detailed information for the path be included</param>
        /// <returns>This <see cref="Path"/> as a string representation</returns>
        public string ToString(bool detailed)
        {
            if(detailed == true)
            {
                StringBuilder builder = new StringBuilder();
                
                builder.AppendLine(string.Format("Path: ({0})", nodes.Count));

                foreach (IPathNode node in nodes)
                    builder.AppendLine(string.Format("\tNode: ({0}, {1})", node.WorldPosition.x, node.WorldPosition.y));

                return builder.ToString(); 
            }
            else
            {
                return string.Format("Path: ({0})", nodes.Count);
            }
        }

        /// <summary>
        /// IEnumerator implementation.
        /// </summary>
        /// <returns>The enumerator for the inner collection</returns>
        public IEnumerator<PathRouteNode> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        /// <summary>
        /// IEnumerator implementation.
        /// </summary>
        /// <returns>The enumerator for the inner collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
    }
}
