using UnityEngine;
using System.Collections;

namespace AStar_2D
{
    /// <summary>
    /// The diagonal mode to use for pathfinding.
    /// This value is specified on a per node basis.
    /// </summary>
    public enum PathNodeDiagonalMode
    {
        /// <summary>
        /// Diagonal movement should not be allowed.
        /// </summary>
        NoDiagonal,
        /// <summary>
        /// Diagonal movement is allowed.
        /// </summary>
        Diagonal,
        /// <summary>
        /// Diagonal movement is allowed but corner cutting of non-walkable nodes is not allowed.
        /// </summary>
        DiagonalNoCutting,
        /// <summary>
        /// Use the global diagonal mode specified in the <see cref="AStarGrid"/>. 
        /// </summary>
        UseGlobal,
    }

    /// <summary>
    /// The interface that must be implemented by the game in order for pathfinding functionality to be available.
    /// </summary>
    public interface IPathNode
    {
        // Properties
        /// <summary>
        /// Can the node be traversed.
        /// </summary>
        bool IsWalkable { get; }

        /// <summary>
        /// A normalized weighting value used to give specific paths higher costs, making them less likley to be selected.
        /// </summary>
        float Weighting { get; }

        /// <summary>
        /// The position of the node in the world. Used by agents for path traversal.
        /// </summary>
        Vector3 WorldPosition { get; }

        /// <summary>
        /// The diagonal mode used for this path node. This allows each node to define how it connects to other nodes.
        /// In most cases it will be OK to simply return <see cref="PathNodeDiagonalMode.UseGlobal"/>, in which case the diagonal settings of the pathfinding grid will be used. 
        /// </summary>
        PathNodeDiagonalMode DiagonalMode { get; }
    }

    internal interface IPathNodeIndex
    {
        // Properties
        Index Index { get; }

        int NodeIndex { get; set; }
    }
}
