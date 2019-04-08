using UnityEngine;
using System.Collections;

namespace AStar_2D.Demo
{
    /// <summary>
    /// A simple chase agent that will attempt to follow a target object where possible using A*.
    /// The class inherits from the Demo script 'AnimatedAgent' but this can be changed to inherit from 'Agent' if animation is not required or if you implement your own system.
    /// </summary>
    public class FollowAgent : AnimatedAgent
    {
        private float lastTime = 0;

        // Public
        /// <summary>
        /// The transform the object object that should be followed.
        /// </summary>
        public Transform target;
        /// <summary>
        /// How often the agent will update its path.
        /// </summary>
        public float targetUpdateTime = 0.2f; // only update the pathfinding every 200 milliseconds

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public override void Start()
        {
            // Make sure we start the animated agent
            base.Start();

            // Check for a valid target
            try
            {
                // Try to find a tagged object if a target has not been assigned
                if (target == null)
                    target = GameObject.FindWithTag("Player").transform;
            }
            catch { Debug.LogWarning("No target found!"); }
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public override void Update()
        {
            // Make sure we update the animated agent
            base.Update();

            // Make sure we are ready to update
            if (waitForUpdate() == false)
                return;

            // Make sure we have a target
            if (target == null)
                return;

            // Find the position of the target using findNearestIndex
            // This method is very expensive so we need to take care not to call it too often
            Index targetIndex = searchGrid.findNearestIndex(target.position);

            // Set the agents target index
            // This is the main method that starts the agent moving towards a location using A*
            setDestination(targetIndex);
        }

        /// <summary>
        /// Returns true when the amount of time passed is greater than the targetUpdateTime.
        /// This allows for a lazy update behaviour where the pathfinding is only updated every X amount of seconds because findNearestIndex is an expensive method.
        /// </summary>
        /// <returns></returns>
        private bool waitForUpdate()
        {
            if (Time.time > (lastTime + targetUpdateTime))
            {
                // Update timer
                lastTime = Time.time;

                // Signal for udpate
                return true;
            }

            // Not time for an update
            return false;
        }
    }
}
