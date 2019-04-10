using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


namespace AStar_2D
{
    /// <summary>
    /// All possible states that an agent can be in.
    /// </summary>
    public enum AgentState
    {
        /// <summary>
        /// The agent is not doing anything related to pathfinding.
        /// </summary>
        Idle = 0,
        /// <summary>
        /// The agent is waiting for a path to be calcualted.
        /// </summary>
        AwaitingPath,
        /// <summary>
        /// The agent is currently following its path.
        /// </summary>
        FollowingPath,
    }

    /// <summary>
    /// Used by the agent to represent the current facing direction. 
    /// The direction can be used as a bitmask to store nultiple values.
    /// The direction will always contain either <see cref="AgentDirection.Forward"/> or <see cref="AgentDirection.Backward"/>.
    /// </summary>
    [Flags]
    public enum AgentDirection
    {
        /// <summary>
        /// The agent is facing forward.
        /// </summary>
        Forward = 1,
        /// <summary>
        /// The agent is facing backward.
        /// </summary>
        Backward = 2,
        /// <summary>
        /// The agent is facing left. 
        /// </summary>
        Left = 4,
        /// <summary>
        /// The agent is facing right.
        /// </summary>
        Right = 8,

        /// <summary>
        /// The agent is facing forward and left diagonally.
        /// </summary>
        ForwardLeft = Forward | Left,
        /// <summary>
        /// The agent is facing forward and right diagonally.
        /// </summary>
        ForwardtRight = Forward | Right,
        /// <summary>
        /// The agent is facing backward and left diagonally.
        /// </summary>
        BackwardLeft = Backward | Left,
        /// <summary>
        /// The agent is facing backward and right diagonally.
        /// </summary>
        BackwardRight = Backward | Right,

        /// <summary>
        /// The agent is facing its default direction of <see cref="Forward"/>.
        /// </summary>
        Default = Forward,
    }

    /// <summary>
    /// Represents an AI pathfinding agent that is able to request a path from its assigned <see cref="AStarGrid"/> and then proceed to follow the generated path until its has reached the end node.
    /// The class can be inherited to build upon the behaviour as seen in the example script <see cref="AStar_2D.Demo.AnimatedAgent"/>.
    /// </summary>
    public class Agent : MonoBehaviour
    {
        // Private
        
        private PathSession session = null;
        private Index currentIndex = Index.invalid;
        private Vector3 lastPosition = Vector3.zero;
        private bool isMoving = false;
        

        // Protected
        /// <summary>
        /// The current state of the agent.
        /// </summary>
        protected AgentState state = AgentState.Idle;
        /// <summary>
        /// The current direction that the agent is heading in. This can be used as a bitmask and will always contain either <see cref="AgentDirection.Forward"/> or <see cref="AgentDirection.Backward"/>.
        /// </summary>
        protected AgentDirection direction = AgentDirection.Default;   

        // Public
        /// <summary>
        /// The <see cref="AStarGrid"/> that this agent can traverse.
        /// </summary>
        public AStarGrid searchGrid;
        /// <summary>
        /// How fast should the agent move while following its path.
        /// </summary>
        public float moveSpeed = 2.0f;
        /// <summary>
        /// Should the agent find a new path if the old one becomes unreachable.
        /// </summary>
        public bool dynamicRoutes = true;
        /// <summary>
        /// Should the agent be considered as an obstacle. If so, the current node occupied by the agent will be considered unwalkable.
        /// </summary>
        private bool dynamicObstacle = true;

        // Properties
        /// <summary>
        /// Returns true if the agent is currently moving along a path.
        /// </summary>
        /// 


        public bool IsMoving
        {
            get { return isMoving; }
        }

        /// <summary>
        /// Returns true if pathfinding can be used by this agent. The agent must have a valid <see cref="AStarGrid"/> assigned or be able to retrieve a valid grid from the <see cref="AStarGridManager"/> at startup.
        /// </summary>
        public bool IsPathfindingAvailable
        {
            get { return searchGrid != null; }
        }

        /// <summary>
        /// <see cref="DynamicObstacle"/> implementation. 
        /// </summary>
        public bool IsObstructing
        {
            get { return dynamicObstacle; }
        }

        // Methods     
        /// <summary>
        /// Event method that is called when the agent has reached its last set destination.
        /// </summary>
        public virtual void onDestinationReached() { }

        /// <summary>
        /// Event method that is called when the agent is not able to find a path to the target location.
        /// This can be caused by too many obstacles blocking the way.
        /// </summary>
        public virtual void onDestinationUnreachable() { }

        /// <summary>
        /// Called by Unity.
        /// Can be overriden but be sure to call base.Start() to allow the agent to initialize properly.
        /// </summary>
        public virtual void Start()
        {
            if (searchGrid == null)
            {
                // Try to access a default grid
                searchGrid = AStarGridManager.DefaultGrid;

                // Check for error
               if (searchGrid == null)
               {
                    // Print a warning
                Debug.LogWarning(string.Format("Agent [{0}]: The are no AStar Grids in the scene. Pathfinding is not possible", gameObject.name));
               }
            }
        }

       
            // CHeck if we have a grid                                          

        /// <summary>
        /// Called by Unity.
        /// Can be overriden but be sure to call base.Update() to allow the agent to update properly.
        /// </summary>
        public virtual void Update()
        {
            // Reset flag
            isMoving = false;

            switch(state)
            {
                default:
                case AgentState.Idle:
                case AgentState.AwaitingPath:
                    {
                        // Store the last position
                        lastPosition = transform.position;
                    }
                    return;

                case AgentState.FollowingPath:
                    {
                        // Make sure our path session is valid
                        if(session == null)
                        {
                            // Go back to the idle state and do nothing
                            changeState(AgentState.Idle);
                            return;
                        }
                        
                        // Check if we have arrived
                        if(session.hasReachedCurrentNode(transform) == true)
                        {
                            // Update our agents target index
                            currentIndex = session.Index;

                            // Check if we have arrived at the last node
                            if(session.IsLastNode == true)
                            {
                                // Snap to destination
                                transform.position = session.WorldPosition;

                                // We have completed the path
                                session = null;

                                // Trigger message
                                onDestinationReached();

                                // Change to idle state
                                changeState(AgentState.Idle);
                            }
                            else
                            {
                                // Make sure we can still reach the target
                                if(session.CurrentPath.IsFullyReachable == false)
                                {
                                    if (dynamicRoutes == true)
                                    {
                                        // Update the destination
                                        setDestination(session.CurrentPath.LastNode.Index);
                                        return;
                                    }
                                }

                                // Get the next node in the path
                                session.advancePath();

                                // Make sure our target is walkable
                                if(dynamicRoutes == false)
                                {
                                    //if(target.IsWalkable == false)
                                    if (session.CurrentPath.IsFullyReachable == false)
                                    {
                                        Debug.LogWarning(string.Format("Agent [{0}]: I cannot reach that destination anymore. The path has been blocked", gameObject.name));

                                        // Stop walking
                                        changeState(AgentState.Idle);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Move towards our target
                            moveTowards();
                        }
                    } break;
            }
        }

        /// <summary>
        /// Places the agent at the specified index.
        /// When called before the agent is first updated, this will greatly reduce the time spent initializing the agent by avoiding a call to <see cref="AStar_2D.Pathfinding.SearchGrid.findNearestIndex(Vector3)"/>.
        /// Requires that a valid grid reference is assigned or found at startup.
        /// </summary>
        /// <param name="index">The index to place the agent at</param>
        public void placeAt(Index index)
        {
            // Make sure we have a grid
            if (IsPathfindingAvailable == true)
            {
                // Find the node
                IPathNode node = searchGrid[index.X, index.Y];

                currentIndex = index;
                transform.position = node.WorldPosition;
            }
        }

        /// <summary>
        /// Provides the agent with a new destination target which in turn calls the path finding algorithm using the assigned <see cref="AStarGrid"/>.
        /// </summary>
        /// <param name="target">The <see cref="Index"/> into the assigned <see cref="AStarGrid"/> that the agent should move to</param>
        public void setDestination(Index target)
        {
            // Make sure we have a grid for pathfinding requests
            if (IsPathfindingAvailable == true)
            {
                // Request a path and change state
                changeState(AgentState.AwaitingPath);

                // Find the current index if it is not already set
                if (currentIndex == Index.invalid)
                    currentIndex = searchGrid.findNearestIndex(transform.position);

                // Search for a path
                searchGrid.findPath(currentIndex, target, onPathFound);
            }
        }

        /// <summary>
        /// Provides the agent with a new destination target as a point in 3D space.
        /// The world position provided will be approximated into an index and a path to the exact location may not be found.
        /// The target point will have and accuracy that is equivilent to the spacing of grid nodes.
        /// This method is expensive to run continuously and you should use <see cref="setDestination(Index)"/> as an alternative where possible.
        /// </summary>
        /// <param name="worldPosition">The world position to find a path to</param>
        public void setDestination(Vector3 worldPosition)
        {
            // Make sure we have a grid for pathfinding requests
            if(IsPathfindingAvailable == true)
            {
                // Get the nearest index
                Index index = searchGrid.findNearestIndex(worldPosition);

                // Call through
                setDestination(index);
            }
        }

        /// <summary>
        /// Allows the state of the agent to be modified in order to change its current behaviour.
        /// Can be used to halt path following if another task with more priority should be carried out.
        /// </summary>
        /// <param name="state"></param>
        public void changeState(AgentState state)
        {
            this.state = state;
        }

        private void onPathFound(Path path, PathRequestStatus status)
        {
            if (status == PathRequestStatus.PathFound)
            {
                // Create the path session
                this.session = new PathSession(path);

                // Check if we are at the starting node - if so then skip
                if(currentIndex == session.Index)
                {
                    // Skip the starting node
                    session.advancePath();
                }

                changeState(AgentState.FollowingPath);
            }
            else if(status == PathRequestStatus.SameStartEnd)
            {
                // Make sure the agent moves to the current location (The agent may have already started moving away)
                Path temp = new Path(null);
                temp.push(searchGrid[currentIndex.X, currentIndex.Y], currentIndex);

                // Change state
                changeState(AgentState.FollowingPath);
            }
            else if (status == PathRequestStatus.PathNotFound)
            {
                // Trigger the message
                onDestinationUnreachable();

                // Go back to idle mode
                changeState(AgentState.Idle);
            }
            else
            { 
                // Go back to idle mode
                changeState(AgentState.Idle);
            }
        }

        private void moveTowards()
        {
            // Move the agent towards the node
            Vector3 update = Vector3.MoveTowards(lastPosition, session.WorldPosition, moveSpeed * Time.deltaTime);

            // Get the movement vector only
            Vector3 temp = update - lastPosition;
            
            // Make sure the vector is not zero
            if (temp.sqrMagnitude != 0)
            {
                // Check if the movement is up or down
                if (temp.y <= 0.002f)
                {
                    // Moving downwards - Default to forward where possible
                    direction = AgentDirection.Forward;
                }
                else
                {
                    // Moving upwards
                    direction = AgentDirection.Backward;
                }

                if (temp.x > 0)
                {
                    // Moiving right
                    direction |= AgentDirection.Right;
                }
                else if (temp.x < 0)
                {
                    // Moving left
                    direction |= AgentDirection.Left;
                }
            }

            // Set the moving flag
            isMoving = (update != transform.position);

            // Update the position            
            transform.position = update;
            lastPosition = transform.position;
        }                
    }    
}
