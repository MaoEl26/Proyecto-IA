using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AStar_2D.Demo
{
    /// <summary>
    /// Used by the demo scene to spawn and despawn various numbers of agents.
    /// </summary>
    public sealed class AgentSpawner : MonoBehaviour
    {
        // Private
        private Queue<GameObject> spawned = new Queue<GameObject>();
        private GameObject root = null;

        // Public
        /// <summary>
        /// The agent prefab that should be spawned.
        /// </summary>
        public GameObject agentPrefab;
        /// <summary>
        /// How many agents can be spawned per frame. Spawn requests are spread over multiple frames to ease lag spikes on the main thread.
        /// </summary>
        public int spawnPerFrame = 5;

        // Methods
        /// <summary>
        /// Spawns the specified number of agents into the scene.
        /// </summary>
        /// <param name="amount">The number of agents to spawn</param>
        public void onAddClicked(int amount)
        {
            // Start spawning
            StartCoroutine(spawnRoutine(amount));
        }
        
        /// <summary>
        /// Despawns the specified number of agents from the scene.
        /// </summary>
        /// <param name="amount">The number of agents to despawn</param>
        public void onRemoveClicked(int amount)
        {
            for(int i = 0; i < amount; i++)
            {
                // No agents left to destroy
                if (spawned.Count == 0)
                    break;

                // Get the front agent
                GameObject go = spawned.Dequeue();

                // Destory the object
                Destroy(go);
            }
        }

        private IEnumerator spawnRoutine(int amount)
        {
            int counter = 0;

            for(int i = 0; i < amount; i++, counter++)
            {
                // Spawn an agent
                GameObject go = Instantiate(agentPrefab, randomLocation(), Quaternion.identity) as GameObject;

                // Check for root object
                if(root == null)
                    root = new GameObject("Agents");

                // Add as child
                go.transform.SetParent(root.transform);

                // Register
                spawned.Enqueue(go);

                // Update counter
                counter++;

                // Check for yeild condition
                if (counter > spawnPerFrame)
                {
                    // Reset counter
                    counter = 0;

                    // Wait for next frame
                    yield return null;
                }
            }
        }

        private Vector3 randomLocation()
        {
            float x = Random.Range(-20, 20);
            float y = Random.Range(-20, 20);

            return new Vector3(x, y, 0);
        }
    }
}
