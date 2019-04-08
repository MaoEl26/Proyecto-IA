using System;
using UnityEngine;

namespace AStar_2D
{
    /// <summary>
    /// A <see cref="PathSession"/> is used to easily move along a path without having to manage indexes. 
    /// </summary>
    public sealed class PathSession : IPathNode
    {
        // Private
        private Path path = null;
        private PathRouteNode currentNode = null;
        private int currentIndex = 0;

        // Public
        /// <summary>
        /// The maxiumum distance that an agent can be from a node and still be considered as 'at' the node.
        /// </summary>
        public const float distanceTolerance = 0.1f;

        // Properties
        /// <summary>
        /// Get the <see cref="Path"/> that this session is using. 
        /// </summary>
        public Path CurrentPath
        {
            get { return path; }
        }

        /// <summary>
        /// Get the current index into the path that this session is at.
        /// </summary>
        public int CurrentIndex
        {
            get { return currentIndex; }
        }

        /// <summary>
        /// Returns tru if the sessions current index is the same as the last path node.
        /// </summary>
        public bool IsLastNode
        {
            get { return currentNode == path.LastNode; }
        }

        /// <summary>
        /// Returns the number of nodes in the path.
        /// </summary>
        public int PathLength
        {
            get
            {
                if (path == null)
                    return 0;

                // Get the number of nodes
                return path.NodeCount;
            }
        }

        /// <summary>
        /// Retruns the number of nodes that are remaining before we reach the end of the path.
        /// </summary>
        public int RemainingPathLength
        {
            get { return PathLength - currentIndex; }
        }

        /// <summary>
        /// Get the index of the current node in the session.
        /// </summary>
        public Index Index
        {
            get
            {
                if (currentNode == null)
                    return Index.zero;

                // Get index
                return currentNode.Index;
            }
        }

        /// <summary>
        /// Get the walkable status of the current node in the session.
        /// </summary>
        public bool IsWalkable
        {
            get
            {
                if (currentNode == null)
                    return false;

                // Get walkable
                return currentNode.IsWalkable;
            }
        }

        /// <summary>
        /// Get the weighting value of the current node in the session.
        /// </summary>
        public float Weighting
        {
            get
            {
                if (currentNode == null)
                    return 0;

                // Get weighting
                return currentNode.Weighting;
            }
        }

        /// <summary>
        /// Get the world position of the current node in the session.
        /// </summary>
        public Vector3 WorldPosition
        {
            get
            {
                if (currentNode == null)
                    return Vector3.zero;

                // Get world position
                return currentNode.WorldPosition;
            }
        }

        /// <summary>
        /// Get the diagonal mode of the current node in the session.
        /// </summary>
        public PathNodeDiagonalMode DiagonalMode
        {
            get
            {
                if (currentNode == null)
                    return 0;

                // Get diagonal mode
                return currentNode.DiagonalMode;
            }
        }

        // Constructor
        /// <summary>
        /// Create a <see cref="PathSession"/> from the specified <see cref="Path"/>.  
        /// </summary>
        /// <param name="path"></param>
        public PathSession(Path path)
        {
            // Make the path active
            usePath(path);
        }

        // Methods
        /// <summary>
        /// Switch this <see cref="PathSession"/> to use the specified <see cref="Path"/>.  
        /// </summary>
        /// <param name="path"></param>
        public void usePath(Path path)
        {
            this.path = path;
            this.currentIndex = 0;
            this.currentNode = path.StartNode;
        }

        /// <summary>
        /// Advance the session to the next node in the path.
        /// </summary>
        /// <returns>True if the session was advanced successfully or false if the session is at the end of the path</returns>
        public bool advancePath()
        {
            if (currentIndex < path.NodeCount - 1)
            {
                // Increase index
                currentIndex++;

                // Update current
                currentNode = path.getNode(currentIndex);

                // Check if we advanced successfully
                return currentNode != null;
            }

            return false;
        }

        /// <summary>
        /// Check whether the specified transform has reached the current node.
        /// The <see cref="distanceTolerance"/> determines how far the transform can be from the current node and still be considered as 'arrived'. 
        /// </summary>
        /// <param name="transform">The transform to check</param>
        /// <returns>True if the transform has reached the current session node or false if not</returns>
        public bool hasReachedCurrentNode(Transform transform)
        {
            // Find the distance to the current node
            float distance = Vector3.Distance(transform.position, currentNode.WorldPosition);

            // Check if we are within tolerance
            return distance < distanceTolerance;
        }
    }
}
