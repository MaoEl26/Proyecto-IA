using UnityEngine;
using System.Collections;

// Import the AStar_2D namespace
using AStar_2D;

// Namespace
namespace AStar_2D.Demo
{
    /// <summary>
    /// This would be the base tile class for a tile based game which contains properties about the appearance etc of the tile
    /// Can inherit any class required but must implement the INode interface to be able to be used by the pathfinding system
    /// </summary>
	public class Tile : MonoBehaviour, IPathNode
	{
        internal bool touchingPathFlag = false;
        // Delegates
        /// <summary>
        /// Delegate used when tiles are selected by a mouse click.
        /// </summary>
        /// <param name="tile">The tile that was clicked.</param>
        /// <param name="mouseButton">The mouse button that was pressed</param>
        public delegate void TileSelectedDelegate(Tile tile, int mouseButton);

        /// <summary>
        /// Delegate used when tiles are hovered by the mouse
        /// </summary>
        /// <param name="tile"></param>
        public delegate void TileHoverDelegate(Tile tile);

        // Events
        /// <summary>
        /// Event that triggers when this tile has been selected a mouse click.
        /// Informs the tile manager so that the appropriate action can be taken.
        /// </summary>
        public event TileSelectedDelegate onTileSelected;

        /// <summary>
        /// Event that triggers when this tile is hovered by the mouse.
        /// Informs the tile manager so that a preview path can be shown.
        /// </summary>
        public event TileHoverDelegate onTileHover;

        // Private
        [SerializeField]
        private float weighting = 0;

        [SerializeField]
        private bool walkable = true;
        private bool canSend = true;
        private float lastTime = 0;
        
        // Public
        /// <summary>
        /// The index into the grid that this tile is located at.
        /// </summary>
        public Index index = new Index();

        /// <summary>
        /// The method used to connect adjacent nodes.
        /// </summary>
        public PathNodeDiagonalMode diagonalMode = PathNodeDiagonalMode.UseGlobal;

		// Properties
        /// <summary>
        /// Implement the IsWalkable property in the INode interface
        /// It is up to the user to determine whether or not a tile should be walkable
        /// </summary>
        public bool IsWalkable
        {
            get { return walkable; } // Only need to implement the get but set is useful
            set { walkable = value; }
        }

        /// <summary>
        /// Implement the Weighting property in the INode interface
        /// It is up to the user to decide a suitable value between 0 and 1 which represents avoidance of tiles to a certain level
        /// e.g. Grass tiles might have a higher weighting value when there are path tile to walk on
        /// The lower the weighting value, the less resistance there will be
        /// </summary>
        public float Weighting
        {
            get { return weighting; }
            set { weighting = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// The position in 3D space that this tile is located at.
        /// </summary>
        public Vector3 WorldPosition
        {
            get { return transform.position; }
        }

        /// <summary>
        /// The diagonal mode to use for pathfinding.
        /// </summary>
        public PathNodeDiagonalMode DiagonalMode
        {
            get { return diagonalMode; }
        }

		// Methods
        /// <summary>
        /// Called by Unity.
        /// Left blank for demonstration.
        /// </summary>
		public void Start () 
		{
			// DO setup code for the tile
		}
		
        /// <summary>
        /// Called by Unity.
        /// Left blank for demonstration.
        /// </summary>
		public void Update () 
		{
            // Update tile specific properties or modify the IsWalkable / Weighting values at runtime
        }
        
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void LateUpdate()
        {
            if (Time.time > (lastTime + 0.2f))
            {
                canSend = true;
                lastTime = Time.time;
            }
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void OnMouseEnter()
        {
            // Trigger the hover event
            if (onTileHover != null)
                onTileHover(this);
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void OnMouseOver()
        {
            // Make sure events can be sent
            if (canSend == false)
                return;

            // Check for mouse button
            if(Input.GetMouseButtonDown(0) == true)
            {
                // Block message sending
                canSend = false;

                // Trigger the event
                if (onTileSelected != null)
                    onTileSelected(this, 0);
            }
            else if(Input.GetMouseButtonDown(1) == true)
            {
                // Block message sending
                canSend = false;

                // Trigger the event
                if (onTileSelected != null)
                    onTileSelected(this, 1);
            }
        }

        /// <summary>
        /// Toggle the walkable state of this tile.
        /// </summary>
        public void toggleWalkable()
        {
            walkable = !walkable;

            // Change the tile colour if needed
            UpdateTileColor();
        }

        /// <summary>
        /// Returns true if this node is included in the specified path.
        /// </summary>
        /// <param name="path">The path to search</param>
        /// <returns>True if this node is in the specified path or false if not</returns>
        public bool isTouchingPath(Path path)
        {
            // Check if this tile is a node in the specified path
            foreach (PathRouteNode node in path)
                if (node.Index.Equals(index) == true)
                    return true;

            // Not in the path
            return false;
        }

        /// <summary>
        /// Force the tile to update its current colour based on its weighting value and walkable status.
        /// </summary>
        public Sprite blocked;
        public void UpdateTileColor()
        {
            blocked = Resources.Load<Sprite>("blocked");
            // Get the sprite renderer
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();

            if (IsWalkable == true)
            {
                // interpolate the colour
                Color targetColour = Color.Lerp(Color.white, Color.yellow, weighting);

                // Assign the colour
                renderer.color = targetColour;
            }
            else
            {
                // Red colour for non-walkable
                //renderer.color = Color.red;
                this.GetComponent<SpriteRenderer>().sprite = blocked;

            }
        }
	}
}
