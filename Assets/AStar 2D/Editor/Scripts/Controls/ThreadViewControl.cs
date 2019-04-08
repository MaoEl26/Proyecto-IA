using UnityEngine;
using System.Collections;

using EditorDesignerUI;
using EditorDesignerUI.Controls;
using EditorDesignerUI.Utility;

using AStar_2D.Threading;

namespace AStar_2D.Editor.Controls
{
    internal sealed class ThreadViewControl : HorizontalLayout
    {
        // Private
        private WorkerThread thread = null;
        private Label label = new Label();
        private LoadingBar usage = new LoadingBar();

        // Properties
        public WorkerThread WatchThread
        {
            get { return thread; }
            set { thread = value; }
        }

        public int ThreadID
        {
            set { label.Content.Text = string.Format("Worker Thread [{0}]: ", value); }
        }

        // Constructor
        public ThreadViewControl()
        {
            label.Layout.Size = new Vector2(115, 0);
        }

        // Methods
        protected override void onRenderContent()
        {
            if (thread != null)
            {
                // Update usage
                usage.Value = thread.ThreadLoad;
                usage.Content.Text = string.Format("Usage: {0}%", (int)(thread.ThreadLoad * 100));                
            }

            this.renderControl(label);
            this.renderControl(usage);
        }
    }
}
