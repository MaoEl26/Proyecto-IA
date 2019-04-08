using UnityEngine;
using System.Collections;

namespace AStar_2D.Demo
{
    /// <summary>
    /// An example script that shows how to make use of mono delegates rather than C# delegates.
    /// Notes:  Mono delegate methods must accept a single argument of type 'MonoDelegateEvent'.
    ///         Mono delegates instances can be cached for reuse at a later date.
    ///         Mono delegates make use of the 'SendMessage' method found in the Unity API and as a result can carry a performance cost.
    ///         If the method specified by the mono delegate is not found then the callback will fail silently.
    ///         The method name specified by the mono delegate is case sensitive.
    /// </summary>
    public sealed class EX1_MonoDelegate : MonoBehaviour
    {
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // This example assumes that there is a valid AStar grid component in the scene.
            // The included TileManager script can be used in this case.
            AStarGrid grid = AStarGridManager.DefaultGrid;

            // Create our mono delegate targeting the 'onPathFound' method and specifying 'this' mono behaviour as the receiver.
            // Once created, the delegate can be stored and reused in multiple pathfinding requests.
            MonoDelegate callback = new MonoDelegate(this, "onPathFound");

            // We are also able to add additional listeners to the mono delegate which will also be called.
            callback.addListener(this, "onPathFoundAdditional");

            // Issue a pathfinding request using our mono delegate as the callback
            grid.findPath(new Index(0, 0), new Index(1, 1), callback);
        }
        
        /// <summary>
        /// Called by AStar2D when the algorithm has completed the request.
        /// Note that the methods can private or public.
        /// </summary>
        /// <param name="e">Contains the Path and PathRequestStatus for the request</param>
        private void onPathFound(MonoDelegateEvent e)
        {
            Debug.Log("The mono delegate listener was called with result - " + e.status);
        }

        private void onPathFoundAdditional(MonoDelegateEvent e)
        {
            Debug.Log("The additional mono delegate listener was also called");
        }
    }
}
