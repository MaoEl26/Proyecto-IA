using UnityEngine;
using System.Collections;

namespace AStar_2D.Demo
{
    /// <summary>
    /// An example script that shows the additional functionality that the Path class is able to offer.
    /// </summary>
    public sealed class EX2_Path : MonoBehaviour
    {
        /// <summary>
        /// Called by unity.
        /// </summary>
        public void Start()
        {
            // This example assumes that there is a valid AStar grid component in the scene.
            // The included TileManager script can be used in this case.
            AStarGrid grid = AStarGridManager.DefaultGrid;

            // Issue a pathfinding request
            grid.findPath(new Index(0, 0), new Index(1, 1), onPathFound);
        }

        /// <summary>
        /// Called by AStar2D when the algorithm has completed the request.
        /// Note that the methods can private or public.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="status"></param>
        private void onPathFound(Path path, PathRequestStatus status)
        {
            // Check if the path contains any nodes. Typically this will only be true if the path has not yet been initialized or if a pathfinding request failed.
            if (path.IsEmpty == true)
                Debug.Log("The path is empty path");

            // Check if the path is reachable. A path is defined as reachable when the following conditions are true:
            //      The path contains one or more nodes. 
            //      Every node within the path is walkable.
            if (path.IsReachable == true)
                Debug.Log("The path is reachable");           
        }
    }
}