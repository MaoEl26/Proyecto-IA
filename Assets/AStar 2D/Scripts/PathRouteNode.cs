using UnityEngine;

namespace AStar_2D
{
    /// <summary>
    /// Represents a single node in a <see cref="Path"/>.
    /// Used by an agent to move towards this nodes position.
    /// </summary>
    public sealed class PathRouteNode : IPathNode
    {
        // Private
        private IPathNode reference = null;
        private Index index = Index.zero;

        // Properties
        /// <summary>
        /// The <see cref="Index"/> that this node is located at.
        /// </summary>
        public Index Index
        {
            get { return index; }
        }

        /// <summary>
        /// Can this path node be traversed.
        /// </summary>
        public bool IsWalkable
        {
            get { return reference.IsWalkable; }
        }

        /// <summary>
        /// The weighting value for this node.
        /// </summary>
        public float Weighting
        {
            get { return reference.Weighting; }
        }

        /// <summary>
        /// The position in world space of this node.
        /// </summary>
        public Vector3 WorldPosition
        {
            get { return reference.WorldPosition; }
        }

        /// <summary>
        /// The diagonal mode used to connect to adjacent nodes.
        /// </summary>
        public PathNodeDiagonalMode DiagonalMode
        {
            get { return reference.DiagonalMode; }
        }

        // Constructor
        /// <summary>
        /// Parameter constructor.
        /// </summary>
        /// <param name="reference">The underlying <see cref="IPathNode"/> that this node is wrapping</param>
        /// <param name="index">The <see cref="Index"/> location of this node</param>
        public PathRouteNode(IPathNode reference, Index index)
        {
            this.reference = reference;
            this.index = index;
        }
    }
}
