using UnityEngine;

namespace AStar_2D
{
    /// <summary>
    /// Represents a game object that can obstruct the pathfinding grid using a <see cref="Collider2D"/> to define the bounds of the object. 
    /// </summary>
    public class DynamicObstacle : MonoBehaviour
    {
        // Private
        private Vector3 lastPosition = Vector3.zero;
        private Vector3 lastRotation = Vector3.zero;
        private Vector3 lastScale = Vector3.one;

        private Collider2D colliderObstacle = null;
        private bool isDirty = false;

        // Protected
        /// <summary>
        /// Returns true if the obstacle is enabled for obstruction.
        /// </summary>
        protected bool isObstructing = true;

        // Public
        /// <summary>
        /// The amount of extra area that the collider bounds should expand to.
        /// </summary>
        public const float colliderPadding = 0.25f;

        /// <summary>
        /// The <see cref="AStarGrid"/> that the obstacle belongs to or null if the default grid should be used. 
        /// </summary>
        public AStarGrid searchGrid;
        /// <summary>
        /// The amount that the obstacle needs to move in order to be updated.
        /// This allows obstacles that move infrequently to be cached more efficiently.
        /// </summary>
        public float updateThreshold = 0.05f;        

        // Properties
        /// <summary>
        /// Returns true if the obstacle needs to be rebuilt.
        /// </summary>
        public bool IsDirty
        {
            get { return isDirty; }
        }

        /// <summary>
        /// Returns true if this obstacle is enabled for obstruction.
        /// </summary>
        public bool IsObstructing
        {
            get { return isObstructing; }
            set { isObstructing = value; }
        }

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void Start()
        {
            // CHeck if we have a grid
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


            // Try to get a collider
            colliderObstacle = GetComponent<Collider2D>();

            // Check for collider
            if (colliderObstacle == null)
            {
                Debug.LogWarningFormat("DynamicObstacle '{0}' does not have a Collider2D attached!", gameObject.name);
            }

            lastPosition = transform.position;
            lastRotation = transform.eulerAngles;
            lastScale = transform.localScale;

            // Register obstacle
            if (searchGrid != null && colliderObstacle != null)
                searchGrid.registerObstacle(this);

            // Trigger an update for the obstacle
            invalidate();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void OnEnable()
        {
            // Register obstacle
            if (searchGrid != null && colliderObstacle != null)
                searchGrid.registerObstacle(this);

            // Trigger an update for the obstacle
            invalidate();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void OnDisable()
        {
            // Unregister obstacle
            if (searchGrid != null && colliderObstacle != null)
                searchGrid.unregisterObstacle(this);
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void Update()
        {
            // The object has moved
            if(Vector3.Distance(transform.position, lastPosition) > updateThreshold ||
                Vector3.Distance(transform.eulerAngles, lastRotation) > updateThreshold ||
                Vector3.Distance(transform.localScale, lastScale) > updateThreshold)
            {
                // Update the current position
                lastPosition = transform.position;
                lastRotation = transform.eulerAngles;
                lastScale = transform.localScale;

                // Invalidate the object
                invalidate();
            }
        }

        /// <summary>
        /// Causes the dynamic obstacle to becode invalid forcing it to be updated in the search space.
        /// Note that the update may not be immediate as it must be handled by the managing <see cref="AStarGrid"/>. 
        /// </summary>
        public void invalidate()
        {
            // Set the flag
            isDirty = true;
        }

        /// <summary>
        /// Called by the pathfinding system when this obstacle has just been updated in the search space.
        /// </summary>
        public void onObstacleUpdated()
        {
            // Reset the flag
            isDirty = false;
        }

        /// <summary>
        /// Checks whether this obstacle is occupying the specified position.
        /// </summary>
        /// <param name="position">The world position to check</param>
        /// <returns>True if the obstacle is occupying the position of false if not</returns>
        public virtual bool isOccupiedByObstacle(Vector3 position)
        {
            // Check for obstruction
            if (isObstructing == false ||
                colliderObstacle == null)
            {
                // We dont need to consider this obstacle for pathfinding
                return false;
            }
            
            // Check for point - collider collision
            return colliderObstacle.OverlapPoint(position);
        } 

        /// <summary>
        /// Get the bounding box for the dynamic obstacle.
        /// This helps to reduce the performance imapct by working on a portion of the search space that the bounding box overlaps.
        /// </summary>
        /// <returns>The bounding box for the obstacles collider</returns>
        public virtual Bounds getObstacleBounds()
        {
            // Check for collider
            if (colliderObstacle == null)
                return new Bounds();

            // Get the collider bounds
            Bounds result = colliderObstacle.bounds;

            result.min -= new Vector3(colliderPadding, colliderPadding, 0);
            result.max += new Vector3(colliderPadding, colliderPadding, 0);

            return result;
        }
    }
}
