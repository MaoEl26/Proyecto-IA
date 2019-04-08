using UnityEngine;
using System;
using System.Collections;

using AStar_2D.Pathfinding;
using AStar_2D.Pathfinding.Algorithm;
using AStar_2D.Threading;
using AStar_2D.Visualisation;
using System.Collections.Generic;

namespace AStar_2D
{
    // Delegate
    /// <summary>
    /// Callback delegate used to inform the caller when the pathfinding routine has completed.
    /// </summary>
    /// <param name="path">The <see cref="Path"/> that was found. Depending upon the status this value may be null</param>
    /// <param name="status">The <see cref="PathRequestStatus"/> of the request</param>
    public delegate void PathRequestDelegate(Path path, PathRequestStatus status);

    // Enum
    /// <summary>
    /// Status value used to deternime the outcome of a pathfinding request. 
    /// </summary>
    public enum PathRequestStatus
    {
        /// <summary>
        /// The provided <see cref="Index"/> referenced a node that was outside the bounds of the search grid.
        /// </summary>
        InvalidIndex = 0,
        /// <summary>
        /// The provided start and end <see cref="Index"/> represent the same node in the search space. No path will be generated.
        /// </summary>
        SameStartEnd,
        /// <summary>
        /// The search grid has not been correctly initialized and is unable to handle requests.
        /// </summary>
        GridNotReady,
        /// <summary>
        /// A path to the destination could not be found. The path must be blocked by unwalkable nodes.
        /// </summary>
        PathNotFound,
        /// <summary>
        /// A path to the destination was found.
        /// </summary>
        PathFound,
    }

    internal struct DynamicObstacleData
    {
        // Public
        public float lastUpdate;
        public DynamicObstacle obstacle;
        public List<Index> obstructedIndexes;
    }

    /// <summary>
    /// Represents an AStar search space.
    /// This component must be attached to a game object and can be inherited if requried. See <see cref="AStar_2D.Demo.TileManager"/> for an example.
    /// This component makes use of the default <see cref="SearchGrid"/> implementation. In order to make use of a custom impelemtation you will need to override <see cref="CreateSearchProvider(IPathNode[,])"/>. Take a look at the demo <see cref="AStar_2D.Demo.SelectiveSearchGrid"/> scripts.
    /// The <see cref="SearchGrid"/> class can be used directly if you dont need grid to be a component.
    /// </summary>
    public class AStarGrid : MonoBehaviour
    {
        // Private
        private SearchGrid searchGrid = null;
        private bool isReady = false;
        private float graphUpdateTimer = 0;

        private List<DynamicObstacleData> obstacles = new List<DynamicObstacleData>();

        // Public
        /// <summary>
        /// When enabled, pathfinding requests may be handled by a background worker thread to remove load on the main thread.
        /// </summary>
        public bool allowThreading = true;
        /// <summary>
        /// When enabled, diagonal paths may be considered during the pathfinding operation.
        /// </summary>
        public DiagonalMode diagonalMovement = DiagonalMode.Diagonal;
        /// <summary>
        /// The spacing between the nodes. 
        /// </summary>
        public int nodeSpacing = 1;
        /// <summary>
        /// The maximum number of nodes that a path can contain. Use -1 to specifiy that the path can be of any length.
        /// </summary>
        [Tooltip("[Unlimited = -1] The maximum amount of nodes that a path can contain")]
        public int maxPathLength = -1;
        /// <summary>
        /// The amount of influence that highly weighted nodes will have on the resulting path. Higher values will cause weighted nodes to influence the path more causing more avoidance of undesirable nodes.
        /// </summary>
        public float weightingInfluence = 1;
        /// <summary>
        /// The amount of time in seconds that the grid will wait to update any registered dynamic obstacles.
        /// </summary>
        public float graphUpdateFrequency = 0.5f;

        // Properties
        /// <summary>
        /// Access a node at the specified index.
        /// </summary>
        /// <param name="x">The X component of the index</param>
        /// <param name="y">The Y component fo the index</param>
        /// <returns>The path node at the specified index or null if the grid has not been correctly initialized</returns>
        public IPathNode this[int x, int y]
        {
            get { return (verifyReady() == false) ? null : searchGrid[x, y]; }
        }

        /// <summary>
        /// The heuristic method that is used by the pathfinding algorithm.
        /// The default heuiristic is Euclidean.
        /// </summary>
        public HeuristicProvider Provider
        {
            set { if (verifyReady() == true) searchGrid.provider = value; }
        }

        /// <summary>
        /// The current width of the search space or 0 if it has not been correctly initialized.
        /// </summary>
        public int Width
        {
            get { return (verifyReady() == false) ? 0 : searchGrid.Width; }
        }

        /// <summary>
        /// The current height of the search space or 0 if it has not been correctly initialized.
        /// </summary>
        public int Height
        {
            get { return (verifyReady() == false) ? 0 : searchGrid.Height; }
        }

        /// <summary>
        /// Can the grid accept pathfinding requests.
        /// </summary>
        public bool IsReady
        {
            get { return isReady; }
        }

        // Methods
        /// <summary>
        /// Called by Unity.
        /// Can be overriden but be sure to call base.Awake() to ensure that the grid is correctly initialized.
        /// </summary>
        public virtual void Awake()
        {
            // Register this grid with the grid manager
            AStarGridManager.registerGrid(this);
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void Update()
        {
            // Check for enough time passed
            if(Time.time > (graphUpdateTimer + graphUpdateFrequency))
            {
                // Reset the timer
                graphUpdateTimer = Time.time;
                
                // Rebuild graph
                rebuildGraph();
            }
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void OnValidate()
        {
            if(searchGrid != null)
            {
                // Update the influence value
                searchGrid.WeightingInfluence = weightingInfluence;
            }
        }

        /// <summary>
        /// Called by Unity.
        /// Can be overriden but be sure to call base.OnDestroy() to ensure that the grid is successfully destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
            // Unregister this grid as it is about to be destroyed
            AStarGridManager.unregisterGrid(this);
        }

        /// <summary>
        /// Called when the <see cref="SearchGrid"/> provider needs to be created for the specified input grid.
        /// You should override this method when you wish to use a custom implementation of <see cref="SearchGrid"/>. 
        /// </summary>
        /// <param name="inputGrid">The 2D <see cref="IPathNode"/> array to create the provider from</param>
        /// <returns>A new instance of <see cref="SearchGrid"/></returns>
        public virtual SearchGrid CreateSearchProvider(IPathNode[,] inputGrid)
        {
            // Create the default search grid
            return new SearchGrid(inputGrid);
        }

        /// <summary>
        /// Registers a <see cref="DynamicObstacle"/> with this grid.
        /// </summary>
        /// <param name="obstacle">The <see cref="DynamicObstacle"/> to register</param>
        public void registerObstacle(DynamicObstacle obstacle)
        {
            // Make sure it is not already registered
            foreach(DynamicObstacleData data in obstacles)
            {
                // We have foud the same obstacle already registered
                if (data.obstacle == obstacle)
                    return;
            }

            // Add the obstacle
            obstacles.Add(new DynamicObstacleData
            {
                lastUpdate = 0,
                obstacle = obstacle,
                obstructedIndexes = new List<Index>(),
            });
        }

        /// <summary>
        /// Unregisters the specified <see cref="DynamicObstacle"/> from this grid. 
        /// </summary>
        /// <param name="obstacle">The <see cref="DynamicObstacle"/> to unregister</param>
        public void unregisterObstacle(DynamicObstacle obstacle)
        {
            // Check all obstacles
            for(int i = 0; i < obstacles.Count; i++)
            {
                // Check for matching obstacle
                if(obstacles[i].obstacle == obstacle)
                {
                    // Remove the obstacle
                    obstacles.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// The main method that is used to initialize the grid with user data.
        /// The provided data must be a two dimensional array of class instances that implement the <see cref="IPathNode"/> interface.
        /// The array must be correctly initialized with no null elements.
        /// The array cannot have any dimensions of zero length.
        /// </summary>
        /// <param name="grid">The user input grid</param>
        public void constructGrid(IPathNode[,] grid)
        {
            // Create the search provider
            searchGrid = CreateSearchProvider(grid);

            // Check for invalid provider
            if (searchGrid == null)
                throw new NullReferenceException("Failed to create an instance of 'SearchGrid'. If a custom implementation of 'createSearchProvider' is used, make sure it is not returning null");

            // Register obstacle checker
            searchGrid.CheckIndexOccupied += isIndexOccupied;

            // Set the ready flag
            isReady = true;
        }

        /// <summary>
        /// Attempts to find the corrosponding <see cref="Index"/> from a position in 3D space.
        /// This method is very expensive and performs a distance check for every node in the seach space. 
        /// This method will always return the <see cref="Index"/> that is the shortest distance from the specified position even if this position is well outside the bounds of the grid.
        /// </summary>
        /// <param name="worldPosition">The position in 3D space to try and find an index for</param>
        /// <returns>The closest <see cref="Index"/> to the specified world position</returns>
        public Index findNearestIndex(Vector3 worldPosition)
        {
            // Make sure the grid is ready
            if (verifyReady() == false) return Index.zero;

            // Pass the call through to the raw grid
            return searchGrid.findNearestIndex(worldPosition);
        }

        /// <summary>
        /// Calculates the minumum number of node steps required tbetween the start and end nodes.
        /// This will only ever provide the best possible case and will not take into account non-walkable nodes or obstructions.
        /// </summary>
        /// <param name="start">The start index</param>
        /// <param name="end">The end index</param>
        /// <param name="diagonal">The diagonal mode to use</param>
        /// <returns>The number of nodes required to move between the start and end indexes</returns>
        public static int findNodeDistance(Index start, Index end, DiagonalMode diagonal)
        {
            // Include start node
            int count = 1;

            // Change in x 
            int deltaX = Mathf.Abs(start.X - end.X);

            // Change in y
            int deltaY = Mathf.Abs(start.Y - end.Y);

            if (diagonal == DiagonalMode.NoDiagonal)
            {
                // Add the x and y offset
                count += (deltaX + deltaY);
            }
            else
            {
                // Get the smallest delta
                int smallest = deltaX;

                // Check if y is smaller
                if (deltaY < smallest)
                    smallest = deltaY;

                // Get the number of diagonal steps
                int diagonalSteps = Mathf.Abs(deltaX - deltaY);

                // Add the diagonal offset
                count += (diagonalSteps + smallest);
            }

            return count;
        }

        /// <summary>
        /// Attempts to search this grid for a path between the start and end points.
        /// </summary>
        /// <param name="start">The <see cref="Index"/> into the search space representing the start position</param>
        /// <param name="end">The <see cref="Index"/> into the search space representing the end position</param>
        /// <param name="callback">The <see cref="MonoDelegate"/> to invoke when the algorithm has completed</param>
        public void findPath(Index start, Index end, MonoDelegate callback)
        {
            // Call through
            findPath(start, end, (Path path, PathRequestStatus status) =>
            {
                // Invoke the mono delegate
                callback.invoke(new MonoDelegateEvent(path, status));
            });
        }

        /// <summary>
        /// Attempts to search this grid for a path between the start and end points.
        /// </summary>
        /// <param name="start">The <see cref="Index"/> into the search space representing the start position</param>
        /// <param name="end">The <see cref="Index"/> into the search space representing the end position</param>
        /// <param name="callback">The <see cref="PathRequestDelegate"/> method to call when the algorithm has completed</param>
        public void findPath(Index start, Index end, PathRequestDelegate callback)
        {
            // Call through
            findPath(start, end, diagonalMovement, callback);
        }

        /// <summary>
        /// Attempts to search this grid for a path between the start and end points.
        /// </summary>
        /// <param name="start">The <see cref="Index"/> into the search space representing the start position</param>
        /// <param name="end">The <see cref="Index"/> into the search space representing the end position</param>
        /// <param name="diagonal">The diagonal mode used when finding paths</param>
        /// <param name="callback">The <see cref="MonoDelegate"/> to invoke when the algorithm has completed</param>
        public void findPath(Index start, Index end, DiagonalMode diagonal, MonoDelegate callback)
        {
            // Call through
            findPath(start, end, diagonal, (Path path, PathRequestStatus status) =>
            {
                // Invoke the mono delegate
                callback.invoke(new MonoDelegateEvent(path, status));
            });
        }

        /// <summary>
        /// Attempts to search this grid for a path between the start and end points.
        /// </summary>
        /// <param name="start">The <see cref="Index"/> into the search space representing the start position</param>
        /// <param name="end">The <see cref="Index"/> into the search space representing the end position</param>
        /// <param name="diagonal">The diagonal mode used when finding paths</param>
        /// <param name="callback">The <see cref="PathRequestDelegate"/> method to call whe the algorithm has completed</param>
        public void findPath(Index start, Index end, DiagonalMode diagonal, PathRequestDelegate callback)
        {
            // Make sure the grid is ready
            if (verifyReady() == false)
            {
                callback(null, PathRequestStatus.GridNotReady);
                return;
            }

            // Update max path length
            searchGrid.maxPathLength = maxPathLength;

            // Get the threading value
            bool useThreading = allowThreading;

#if UNITY_WEBGL
            // Threading is not allowed on web gl platform
            useThreading = false;
#endif

            // Check if threading is enabled
            if(useThreading == true)
            {
                // Create a request
                AsyncPathRequest request = new AsyncPathRequest(searchGrid, start, end, diagonal, (Path path, PathRequestStatus status) =>
                {
#if UNITY_EDITOR
                    // Pass the path for rendering before it is used by the caller otherwise nodes may be removed from the path
                    PathView.setRenderPath(this, path);
#endif
                    // Invoke callback
                    callback(path, status);
                });

                // Dispatch the request
                ThreadManager.Active.asyncRequest(request);
            }
            else
            {
                PathRequestStatus status;

                // Run the task immediatley
                Path result = findPathImmediate(start, end, out status, diagonal);

#if UNITY_EDITOR
                // Pass the path for rendering before it is used by the caller otherwise nodes may be removed
                PathView.setRenderPath(this, result);
#endif

                // Trigger callback
                callback(result, status);
            }
        }

        /// <summary>
        /// Attempts to find a <see cref="Path"/> between the start and end points and returns the result on completion.
        /// </summary>
        /// <param name="start">The <see cref="Index"/> into the search space representing the start position</param>
        /// <param name="end">The <see cref="Index"/> into the search space representing the end position</param>
        /// <returns>The <see cref="Path"/> that was found or null if the algorithm failed</returns>
        public Path findPathImmediate(Index start, Index end)
        {
            // Call through
            return findPathImmediate(start, end, diagonalMovement);
        }

        /// <summary>
        /// Attempts to find a <see cref="Path"/> between the start and end points and returns the result on completion.
        /// </summary>
        /// <param name="start">The <see cref="Index"/> into the search space representing the start position</param>
        /// <param name="end">The <see cref="Index"/> into the search space representing the end position</param>
        /// <param name="diagonal">The diagonal mode used when finding paths</param>
        /// <returns>The <see cref="Path"/> that was found or null if the algorithm failed</returns>
        public Path findPathImmediate(Index start, Index end, DiagonalMode diagonal)
        {
            PathRequestStatus status = PathRequestStatus.InvalidIndex;

            // Call through
            return findPathImmediate(start, end, out status, diagonal);
        }

        /// <summary>
        /// Attempts to find a <see cref="Path"/> between the start and end points and returns the result on completion.
        /// </summary>
        /// <param name="start">The <see cref="Index"/> into the search space representing the start position</param>
        /// <param name="end">The <see cref="Index"/> into the search space representing the end position</param>
        /// <param name="status">The <see cref="PathRequestStatus"/> describing the state of the result</param>
        /// <param name="diagonal">The diagonal mode used when finding a path</param>
        /// <returns>The <see cref="Path"/> that was found or null if the algorithm failed</returns>
        public Path findPathImmediate(Index start, Index end, out PathRequestStatus status, DiagonalMode diagonal)
        {
            // Make sure the grid is ready
            if (verifyReady() == false)
            {
                status = PathRequestStatus.GridNotReady;
                return null;
            }

            // Update max path length
            searchGrid.maxPathLength = maxPathLength;

            // Store a temp path
            Path path = null;
            PathRequestStatus temp = PathRequestStatus.InvalidIndex;

            // Find a path
            searchGrid.findPath(start, end, diagonal, (Path result, PathRequestStatus resultStatus) =>
            {
                // Store the status
                temp = resultStatus;

                // Make sure the path was found
                if (resultStatus == PathRequestStatus.PathFound)
                {
                    path = result;

#if UNITY_EDITOR
                    PathView.setRenderPath(this, path);
#endif
                }
            });

            status = temp;
            return path;
        }

        /// <summary>
        /// Forces the grid to rebuild any necessary obstacle data to ensure that the scene and the underlying node structure are in sync.
        /// This method will also be called automatically based on the update rate.
        /// </summary>
        public void rebuildGraph()
        {
            // Process all obstacles
            for(int i = 0; i < obstacles.Count; i++)
            {
                // Get the obstacle data
                DynamicObstacleData data = obstacles[i];
                bool isChanged = false;

                // Check if obstacle is dirty
                if(obstacles[i].obstacle.IsDirty == true)
                {
                    // Set the changed flag
                    isChanged = true;
                    
                    // We are about to rebuild
                    data.lastUpdate = Time.time;

                    // Clear occupied nodes
                    data.obstructedIndexes.Clear();

                    // Check for not obstructing
                    if(data.obstacle.IsObstructing == true)
                    {
                        // Check for occupied nodes
                        Bounds bounds = data.obstacle.getObstacleBounds();

                        // Convert start and end to indexes
                        Index min = searchGrid.findNearestIndex(bounds.min);
                        Index max = searchGrid.findNearestIndex(bounds.max);

                        // Check for occupied indexes
                        for (int x = min.X; x < max.X; x++)
                        {
                            for(int y = min.Y; y < max.Y; y++)
                            {
                                // Get the node position
                                Vector3 pos = searchGrid[x, y].WorldPosition;

                                // Check if the obstacle occupies that position
                                if(data.obstacle.isOccupiedByObstacle(pos) == true)
                                {
                                    // Add to obstructed index list
                                    data.obstructedIndexes.Add(new Index(x, y));
                                }
                            }
                        }
                    }

                    // Call the update method
                    data.obstacle.onObstacleUpdated();
                }

                // Reassign the data
                if (isChanged == true)
                    obstacles[i] = data;
            }
        }

        /// <summary>
        /// Checks whether the specified index is occipied in this grid.
        /// Note that this method only checks if dynamic obstacles are obstructing the grid.
        /// </summary>
        /// <param name="index">The index to check</param>
        /// <returns>True if the node is occupied or false if not</returns>
        public bool isIndexOccupied(Index index)
        {
            lock (obstacles)
            {
                // Check for trivial case
                if (obstacles.Count == 0)
                    return false;

                // Check all obstacles
                for(int i = 0; i < obstacles.Count; i++)
                {
                    for(int j = 0; j < obstacles[i].obstructedIndexes.Count; j++)
                    {
                        if (obstacles[i].obstructedIndexes[j].Equals(index) == true)
                            return true;
                    }
                }

                //foreach (DynamicObstacleData data in obstacles)
                //{
                //    foreach (Index nodeIndex in data.obstructedIndexes)
                //    {
                //        if (nodeIndex.Equals(index) == true)
                //            return true;
                //    }
                //}
            }
            return false;
        }

        private bool verifyReady()
        {
            // Make sure we can accept requests
            if (isReady == false)
            {
                //Debug.LogWarning(string.Format("AStar Grid '{0}' is not ready to receive path requests, Make sure you construct the grid before hand", gameObject.name));
                return false;
            }

            return true;
        }
    }
}
