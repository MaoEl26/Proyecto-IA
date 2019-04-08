using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;

using EditorDesignerUI;
using EditorDesignerUI.Controls;

namespace AStar_2D.Editor
{        
    internal abstract class HelpInspector : DesignerComponent
    {
        // Private
        private static readonly string userGuideUrl = "AStar 2D/UserGuide.pdf";
        private static readonly string referenceGuideUrl = "mk:@MSITStore:{0}/AStar 2D/Scripting Reference.chm";

        private string helpTopic = "";
        private string helpUrl = "";

        // Constructor
        public HelpInspector(string helpTopic)
        {
            this.helpTopic = helpTopic;
        }

        // Methods
        public override void OnEnable()
        {
            // Create the help url
            helpUrl = string.Format(referenceGuideUrl, Application.dataPath);

            // Construct the UI elements
            createUI();
        }

        public override void onRender()
        {
            // Render default inspector
            DrawDefaultInspector();

            // Render controls
            base.onRender();
        }

        private void createUI()
        {
            Panel panel = addControl<Panel>();
            {
                panel.Style = new VisualStyle(EditorStyle.HelpBox);

                HorizontalLayout layout = panel.addControl<HorizontalLayout>();
                {
                    // Label
                    Label label = layout.addControl<Label>();
                    {
                        label.Content.Text = "AStar 2D";
                    }

                    // Push to the right
                    layout.addControl<FlexibleSpacer>();

                    Button userGuideButton = layout.addControl<Button>();
                    {
                        userGuideButton.Content.Text = "User Guide";
                        userGuideButton.Content.Tooltip = "Open the user guide for AStar 2D";
                        userGuideButton.Style = new VisualStyle(EditorStyle.ToolbarButton);

                        userGuideButton.onClicked += (object sender) =>
                        {
                            // Open the user guide
                            Application.OpenURL(Application.dataPath + "/" + userGuideUrl);
                        };
                    }

                    // Help button
                    Button helpButton = layout.addControl<Button>();
                    {
                        helpButton.Content.Text = "Get Help";
                        helpButton.Content.Tooltip = string.Format("Open help page '{0}'", helpUrl);
                        helpButton.Style = new VisualStyle(EditorStyle.ToolbarButton);

                        helpButton.onClicked += (object sender) =>
                        {
                            // Create the topic url
                            string file = string.Format("{0}::{1}", helpUrl, helpTopic);

                            // Launch the process
                            Process.Start("hh.exe", file);
                        };
                    }
                }
            }
        }
    }
}
