//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic; // For linked list access

//using AStar_2D;

//// Namespace
//namespace AStar_2D.Demo
//{
//	public class Selection : MonoBehaviour 
//	{
//		// Private
//        private bool placing = false;
//        private bool start = true;
//        private GameObject startInstance = null;
//        private GameObject endInstance = null;

//        private Index startIndex;
//        private Index endIndex;
//        private TileManager manager;

//		// Public
//        public GameObject startNode;
//        public GameObject endNode;

//		// Methods
//        public void onPathFound(Path nodes)
//        {
//            if (nodes != null && nodes.IsEmpty == false)
//                Debug.Log("Found a valid path");
//            else
//                Debug.Log("No path");
//        }

//		void Start () 
//		{
//            manager = Component.FindObjectOfType<TileManager>();
//		}
		
//		void Update () 
//		{
//            // Check for placing
//            if(placing == true)
//            {
//                // Check for place accepted
//                if (Input.GetMouseButtonDown(0) == true)
//                {
//                    // When the end index is placed
//                    if (start == false)
//                    {
//                        // Rebuild the path
//                        manager.grid.requestPath(startIndex, endIndex, onPathFound);
//                    }

//                    placing = false;
//                    return;
//                }

//                // Get the start tile of the drag
//                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//                // Cast the ray
//                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

//                // Check for a hit
//                if (hit.collider != null)
//                {
//                    // Tags not supported ???
//                    if (hit.collider.name.Contains("Tile") || hit.collider.name.Contains("StartNode") || hit.collider.name.Contains("EndNode"))
//                    {
//                        // Get the tile component
//                        Tile tile = hit.collider.GetComponent<Tile>();

//                        // Check for valid tile
//                        if (tile != null)
//                        {
//                            // Move to location
//                            if(start == true)
//                            {
//                                startInstance.transform.position = tile.WorldPosition;
//                                startIndex = tile.index;
//                            }
//                            else
//                            {
//                                endInstance.transform.position = tile.WorldPosition;
//                                endIndex = tile.index;
//                            }
//                        }
//                    }
//                }
//            }


//            // Dont modify tiles when placing
//            if (placing == true)
//                return;

//            // Change the state of the tiles
//            if (Input.GetMouseButtonDown(0) == true)
//            {
//                // Get the start tile of the drag
//                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//                // Cast the ray
//                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

//                // Check for a hit
//                if (hit.collider != null)
//                {
//                    if (hit.collider.name.Contains("Tile") || hit.collider.name.Contains("StartNode") || hit.collider.name.Contains("EndNode"))
//                    {
//                        // Get the tile component
//                        Tile tile = hit.collider.GetComponent<Tile>();

//                        // Check for valid tile
//                        if (tile != null)
//                        {
//                            tile.toggleWalkable();
//                        }
//                    }
//                }
//            }
//		}

//        public void placeStartNode()
//        {
//            // Check for object
//            if (startInstance == null)
//                startInstance = Instantiate(startNode) as GameObject;

//            // Set the flag
//            placing = true;
//            start = true;
//        }

//        public void placeEndNode()
//        {
//            // Check for object
//            if (endInstance == null)
//                endInstance = Instantiate(endNode) as GameObject;

//            // Set the flag
//            placing = true;
//            start = false;
//        }

//        public void refresh()
//        {
//            manager.grid.requestPath(startIndex, endIndex, onPathFound);
//        }
//	}
//}
