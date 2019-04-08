using UnityEngine;
using System.Collections;

namespace AStar_2D.Demo
{
    /// <summary>
    /// Used in the dynamic obstacle demo.
    /// Represents a basic spinning dynamic obstacle.
    /// </summary>
    public class AnimatedObstacle : MonoBehaviour
    {
        // Public
        /// <summary>
        /// The speed that the obstacle rotates at.
        /// </summary>
        public float rotateSpeed = 1.5f;

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Update()
        {
            // Rotate the obstacle
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }
    }
}
