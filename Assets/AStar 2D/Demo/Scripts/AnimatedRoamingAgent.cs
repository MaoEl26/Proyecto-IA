using UnityEngine;
using System.Collections;

namespace AStar_2D.Demo
{
    /// <summary>
    /// Example class that allows the agent to roam around the grid automatcially.
    /// The agent will select a random location on the grid, move to the target and repeat the process.
    /// </summary>
    public class AnimatedRoamingAgent : AnimatedAgent
    {
        // Private
        private int width = 0;
        private int height = 0;

        // Methods
        /// <summary>
        /// Called by Unity.
        /// Note that the base method is called. This is essential to initialize the base class.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Store the grid size
            width = searchGrid.Width;
            height = searchGrid.Height;

            // Trigger start
            onDestinationReached();
        }

        /// <summary>
        /// Called when the agent has reached its last set destination.
        /// </summary>
        public override void onDestinationReached()
        {
            int x = Random.Range(0, width - 1);
            int y = Random.Range(0, height - 1);

            // Random destination
            setDestination(new Index(x, y));
        }
    }
}
