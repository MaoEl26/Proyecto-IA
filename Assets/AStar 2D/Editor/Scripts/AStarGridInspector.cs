using UnityEngine;
using UnityEditor;
using System.Collections;

using EditorDesignerUI;
using EditorDesignerUI.Controls;
using EditorDesignerUI.Utility;

using AStar_2D.Visualisation;

namespace AStar_2D.Editor
{
    internal sealed class StateField : DesignerVisualControl, IInteractableControl
    {
        // Events
        public event ControlActionDelegate onPing;
        public event ControlActionDelegate onAdd;

        // Private
        private bool state = false;
        private bool targetState = false;
        private Button button = new Button();
        private TextField field = new TextField();

        // Properties
        public override VisualStyle DefaultStyle
        {
            get { return VisualStyle.EmptyStyle; }
        }

        public bool State
        {
            set { targetState = value; }
        }

        public string OffText
        {
            set { button.Content.Text = value; }
        }

        public string OnText
        {
            set { field.Content.Text = value; }
        }

        public string Tooltip
        {
            set
            {
                button.Content.Tooltip = value;
                field.Content.Tooltip = value;
            }
        }

        // Constructor
        public StateField()
        {
            field.Enabled = false;
            button.onClicked += (object sender) =>
            {
                invoke();
            };
        }

        // Methods
        public override void onRender()
        {
            // Check for state change
            if (Event.current.type == EventType.Layout)
                if (targetState != state)
                    state = targetState;

            if(state == true)
            {
                GUILayout.BeginHorizontal();
                {
                    // Render field
                    this.renderControl(field);

                    // Render ping button
                    if (GUILayout.Button("|->", EditorStyles.toolbarButton, GUILayout.Width(30)) == true)
                    {
                        // Invoke the control
                        if (onPing != null)
                            onPing(this);
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                // Render button
                this.renderControl(button);
            }
        }

        public void invoke()
        {
            if (onAdd != null)
                onAdd(this);
        }
    }

    [CustomEditor(typeof(AStarGrid), true)]
    [CanEditMultipleObjects]
    internal sealed class AStarGridInspector : HelpInspector
    {
        // Private
        private AStarGrid grid = null;
        private StateField visualizeGrid = null;
        private StateField visualizePath = null;

        // Constructor
        public AStarGridInspector()
            : base("/html/ac34c068-551e-0237-d21c-cc7d388761ef.htm")
        {

        }

        // Methods
        public override void OnEnable()
        {
            grid = target as AStarGrid;

            // Construct the UI elements
            createUI();

            base.OnEnable();
        }

        public override void onRender()
        {
            base.onRender();

            // Check for visual components
            PathView pathView = PathView.findViewForGrid(grid);
            GridView gridView = GridView.findViewForGrid(grid);

            // Check for view
            if (pathView == null)
            {
                visualizePath.State = false;
            }
            else
            {
                visualizePath.OnText = string.Format("Paths visualized by '{0}'", pathView.gameObject.name);
                visualizePath.State = true;
            }

            // Check for grid
            if(gridView == null)
            {
                visualizeGrid.State = false;
            }
            else
            {
                visualizeGrid.OnText = string.Format("Grid visualized by '{0}'", gridView.gameObject.name);
                visualizeGrid.State = true;
            }
        }

        private void createUI()
        {
            visualizeGrid = addControl<StateField>();
            {
                visualizeGrid.OffText = "Visualize Grid";
                visualizeGrid.OnText = "This grid is currently being visualized by ''";

                visualizeGrid.onAdd += (object sender) =>
                {
                    // Create a object
                    GameObject go = new GameObject(target.name + " (Visual Grid)");

                    // Add the view
                    GridView view = go.AddComponent<GridView>();

                    view.visualizeGrid = grid;
                };
                visualizeGrid.onPing += (object sender) =>
                {
                    // Find the view
                    GridView view = GridView.findViewForGrid(grid);

                    // Ping in hierarchy
                    if (view != null)
                        EditorGUIUtility.PingObject(view.gameObject);
                };
            }

            visualizePath = addControl<StateField>();
            {
                visualizePath.OffText = "Visualize Path";
                visualizePath.OnText = "Paths are currently being visualized by ''";

                visualizePath.onAdd += (object sender) =>
                {
                    // Create a object
                    GameObject go = new GameObject(target.name + " (Visual Path)");

                    // Add the view
                    PathView view = go.AddComponent<PathView>();

                    view.visualizeGrid = grid;
                };
                visualizePath.onPing += (object sender) =>
                {
                    // Find the view
                    PathView view = PathView.findViewForGrid(grid);

                    // Ping in hierarchy
                    if (view != null)
                        EditorGUIUtility.PingObject(view.gameObject);
                };
            }

            addControl<Spacer>();
        }
    }
}
