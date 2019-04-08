using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using EditorDesignerUI;
using EditorDesignerUI.Controls;
using EditorDesignerUI.Utility;

using AStar_2D.Threading;

namespace AStar_2D.Editor.Controls
{ 
    internal sealed class ThreadViewCollectionControl : Panel
    {
        // Private
        private ThreadManager manager = null;
        private List<ThreadViewControl> views = new List<ThreadViewControl>();
        private LoadingBar totalUsage = new LoadingBar();
        private bool active = false;

        // Properties
        public ThreadManager Manager
        {
            set { manager = value; }
        }

        public bool Active
        {
            set { active = value; }
        }

        // Constructor
        public ThreadViewCollectionControl()
        {
            Style = new VisualStyle(EditorStyle.HelpBox);
            //totalUsage.Layout.Size = new Vector2(40, 0);
        }

        // Methods
        public override void onRender()
        {
            if (manager != null)
            {
                // Check for change
                if(Event.current.type == EventType.Layout)
                    if (manager.ActiveThreads != views.Count)
                        updateCollection();
            }

            // Call the base method
            base.onRender();
        }

        protected override void onRenderContent()
        {
            base.onRenderContent();

            if (active == true)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (manager != null)
                    {
                        // Calcualte the average
                        float total = 0;

                        foreach (WorkerThread thread in manager)
                            total += thread.ThreadLoad;

                        // Avoid divide by 0
                        if (manager.ActiveThreads > 0)
                            total /= manager.ActiveThreads;


                        totalUsage.Value = total;
                        totalUsage.Content.Text = string.Format("Total Usage: {0}%", (int)(total * 100));
                    }

                    this.renderControl(totalUsage);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("No data - Thread usage data will be displayed here when the game is running", EditorStyles.helpBox);
            }
        }

        private void updateCollection()
        {
            List<ThreadViewControl> remove = new List<ThreadViewControl>();

            // Remove killed threads
            foreach(ThreadViewControl control in views)
            {
                if(manager.hasThread(control.WatchThread) == false)
                {
                    // Remove from render list
                    remove.Add(control);
                }
            }

            foreach (ThreadViewControl control in remove)
            {
                views.Remove(control);
                this.removeControl(control);
            }

            // Create a view for all threads
            foreach(WorkerThread thread in manager)
            {
                // Check if it exists in the view
                if (hasViewForThread(thread) == true)
                    continue;

                // The thread has not been added yet
                registerThreadView(thread);
            }
        }

        private bool hasViewForThread(WorkerThread thread)
        {
            foreach (ThreadViewControl control in views)
                if (control.WatchThread == thread)
                    return true;

            return false;
        }

        private void registerThreadView(WorkerThread thread)
        {
            ThreadViewControl control = addControl<ThreadViewControl>();
            {
                control.WatchThread = thread;
                control.ThreadID = manager.getThreadID(thread);

                // Add to collection
                views.Add(control);
            }
        }
    }
}
