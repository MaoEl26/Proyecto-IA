using UnityEngine;
using UnityEditor;
using System.Collections;

using EditorDesignerUI;
using EditorDesignerUI.Controls;
using EditorDesignerUI.Utility;

using AStar_2D.Editor.Controls;
using AStar_2D.Threading;

namespace AStar_2D.Editor
{    
    [CustomEditor(typeof(ThreadManager))]
    internal sealed class ThreadManagerInspector : DesignerComponent
    {
        // Private
        private ThreadManager manager = null;

        // Methods
        public override void OnEnable()
        {
            manager = target as ThreadManager;

            // Construct the UI elements
            createUI();
        }

        public override void onRender()
        {
            DrawDefaultInspector();

            base.onRender();

            // Always refresh
            repaint();
        }

        private void createUI()
        {
            ThreadViewCollectionControl control = addControl<ThreadViewCollectionControl>();
            {
                control.Manager = manager;
            }
        }
    }
}
