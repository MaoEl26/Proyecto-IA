using UnityEngine;
using System.Collections;

namespace AStar_2D.Demo
{ 
    /// <summary>
    /// Example class that shows how the agent class can be inherited from and expanded.
    /// </summary>
    public class AnimatedAgent : Agent
    {
        // Private
        private Animator anim = null;
        private float timer = 0;
        private bool canBob = true;

        // Public
        /// <summary>
        /// Should the agent bob up and down when walking.
        /// </summary>
        public bool bob = true;
        /// <summary>
        /// The speed that the agent will bob up and down.
        /// </summary>
        public float bobSpeed = 35f;
        /// <summary>
        /// How far the agent will bob up and down.
        /// </summary>
        public float bobAmount = 1.4f;

        // Methods
        /// <summary>
        /// Called by unity.
        /// Note that the base method is called. This is essential to initialize the base class.
        /// </summary>
        public override void Start()
        {
            // Make sure we call start on the agent class
            base.Start();

            // Find the animator controller
            anim = GetComponent<Animator>();
        }

        /// <summary>
        /// Called by Unity.
        /// Note that the base method is called. This is essential to update the base class.
        /// </summary>
        public override void Update()
        {
            // Make sure we update our agents movement
            base.Update();

            // Update the sprite animation so the character is facing the correct direction
            updateAnimation();

            // Make our agent bob up and down as he walks
            updateHeadBob();
        }

        /// <summary>
        /// Called when the agent is unable to reach a target destination.
        /// </summary>
        public override void onDestinationUnreachable()
        {
            Debug.LogWarning(string.Format("Agent [{0}]: I can't reach that target", gameObject.name));
        }

        private void updateAnimation()
        {
            // Update our character sprite animation
            anim.SetBool("MoveForward", (direction & AgentDirection.Forward) != 0);
        }

        private void updateHeadBob()
        {
            // Only allow character bobbing when moving
            canBob = state == (AgentState.FollowingPath);

            // Make sure head bob is enabled
            if (bob == false || canBob == false)
                return;

            // Add to timer
            float wave = Mathf.Sin(timer);

            // Add the bob speed
            timer += bobSpeed * Time.deltaTime;

            // Clamp bob
            if (timer > Mathf.PI * 2)
                timer = timer - (Mathf.PI * 2);

            // Calcualte the change amount
            float change = wave * bobAmount * Time.deltaTime;

            // Update the position
            transform.position += new Vector3(0, change, 0);
        }
    }
}
