using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using EditorDesignerUI;
using EditorDesignerUI.Controls;
using EditorDesignerUI.Utility;

using AStar_2D.Editor.Controls;
using AStar_2D.Threading;
using AStar_2D.Visualisation;

namespace AStar_2D.Editor
{
    internal sealed class PerformanceVisualiserWindow : DesignerWindow
    {
        // Private
        private IDesignerControl parent = null;
        private HelpBox hint = null;
        private ThreadViewCollectionControl usageView = null;

        private ChartDynamicDataset timingData = new ChartDynamicDataset(Color.blue);
        private ChartDynamicDataset timingPeekData = new ChartDynamicDataset(Color.red);

        private ChartDynamicDataset[] usageData = new ChartDynamicDataset[ThreadManager.maxAllowedWorkerThreads];
        private float lastTime = 0;
        private float updateRate = 1;

        // Properties
        private ThreadManager Manager
        {
            get { return ThreadManager.Active; }
        }

        // Methods
        [MenuItem("Window/AStar 2D Performance Visualiser")]
        public static PerformanceVisualiserWindow showWindow()
        {
            return showWindow<PerformanceVisualiserWindow>();
        }

        public override void OnEnable()
        {
            // Listen for play events
            EditorApplication.playmodeStateChanged += onEnterExitPlayMode;

            WindowTitle = "AStar 2D";
            Layout.MinSize = new Vector2(380, 200);

            // Create the UI
            createUI();

            // Trigger state change
            if (Application.isPlaying == true)
                onEnterExitPlayMode();
        }

        public override void OnDisable()
        {
            // Remove listener
            EditorApplication.playmodeStateChanged -= onEnterExitPlayMode;
        }

        public override void onRender()
        {
            base.onRender();

            // Check for sample updates
            if(Time.time > lastTime + updateRate)
            {
                lastTime = Time.time;
                
                updateSamples();
            }

            // Check if the game is running
            if (Application.isPlaying == true)
            {
                // Constant refresh while playing
                repaint();
            }
        }

        private void createUI()
        {
            Toolbar toolbar = addControl<Toolbar>();
            {
                Label label = toolbar.addControl<Label>();
                {
                    label.Content.Text = "Update Rate: ";
                    label.Layout.Size = new Vector2(0, 0);
                }
                ToggleButton slow = toolbar.addControl<ToggleButton>("0");
                {
                    slow.Style = new VisualStyle(EditorStyle.ToolbarButton);
                    slow.Content.Text = "Slow Update";
                    slow.Content.Tooltip = "Set the refresh mode to slow. Less updates per frame";

                    slow.onClicked += (object sender) =>
                    {
                        setUpdateRate(0);
                    };
                }
                ToggleButton medium = toolbar.addControl<ToggleButton>("1");
                {
                    medium.Style = new VisualStyle(EditorStyle.ToolbarButton);
                    medium.Content.Text = "Medium Update";
                    medium.Content.Tooltip = "Set the refresh mode to medium. The default settings";

                    medium.onClicked += (object sender) =>
                    {
                        setUpdateRate(1);
                    };
                }
                ToggleButton fast = toolbar.addControl<ToggleButton>("2");
                {
                    fast.Style = new VisualStyle(EditorStyle.ToolbarButton);
                    fast.Content.Text = "Fast Update";
                    fast.Content.Tooltip = "Set the refresh mode to fast. More updates per frames for more accurate data";

                    fast.onClicked += (object sender) =>
                    {
                        setUpdateRate(2);
                    };
                }
                toolbar.addControl<FlexibleSpacer>();

                // Set the mode
                setUpdateRate(-1);
            }

            parent = addControl<VerticalLayout>();
            {
                HelpBox timingHelp = parent.addControl<HelpBox>();
                {
                    timingHelp.Content.Text = "Shows the average and peek time taken to complete the algorithm";
                }

                Chart timingChart = parent.addControl<Chart>();
                {
                    timingChart.Layout.MinSize = new Vector2(0, 0);
                    timingChart.setChartAxis(ChartAxis.Horizontal, 0, 1, "Update (Frame Step)");
                    timingChart.setChartAxis(ChartAxis.Vertical, 0, 1, "Time (ms)");

                    timingChart.setDatasetCount(2);
                    timingChart.setDataset(0, timingData);
                    timingChart.setDataset(1, timingPeekData);

                    timingData.Name = "Average Time (Per sample)";
                    timingPeekData.Name = "Highest Time (Per sample)";
                }

                HelpBox usageHelp = parent.addControl<HelpBox>();
                {
                    usageHelp.Content.Text = "Shows the current usages value for all worker threads (When active)";
                }

                HorizontalLayout usageLayout = parent.addControl<HorizontalLayout>();
                {
                    Chart usageChart = usageLayout.addControl<Chart>();
                    {
                        usageChart.Layout.MinSize = new Vector2(0, 0);
                        usageChart.setChartAxis(ChartAxis.Horizontal, 0, 1, "Update (Frame Step)");
                        usageChart.setChartAxis(ChartAxis.Vertical, 0, 1, "Usage (%)");

                        // Set data
                        usageChart.setDatasetCount(3);

                        for (int i = 0; i < ThreadManager.maxAllowedWorkerThreads; i++)
                        {
                            usageData[i] = new ChartDynamicDataset();
                            usageData[i].Name = string.Format("Thread {0}", i + 1);
                            usageChart.setDataset(i, usageData[i]);
                        }
                    }

                    usageView = usageLayout.addControl<ThreadViewCollectionControl>();
                    {
                        usageView.Layout.Size = new Vector2(230, 0);
                    }
                }
            }

            hint = addControl<HelpBox>();
            {
                hint.HelpType = HelpBoxType.Info;
                hint.Content.Text = "Waiting for game to launch. Timing and usage statistics can only be gathered in play mode";
            }
        }

        private void onEnterExitPlayMode()
        {
            if (EditorApplication.isPlaying == true)
            {
                lastTime = Time.time;

                // Enable the controls
                parent.Enabled = true;
                hint.Visible = false;

                usageView.Manager = Manager;
                usageView.Active = true;
            }
            else
            {
                // Disable the controls
                parent.Enabled = false;
                hint.Visible = true;

                usageView.Manager = null;
                usageView.Active = false;
                usageView.removeAll();

                // Issue a delayed repaint
                Routine.run(delayedRepaint());
            }

            // Force a repaint
            repaint();
        }

        private IEnumerator delayedRepaint()
        {
            yield return new WaitForSeconds(0.2f);

            repaint();
        }

        private void updateSamples()
        {
            // Update timing samples
            timingData.add(Performance.getAverageTimingValue());
            timingPeekData.add(Performance.getPeekTimingValue());

            // Update usage samples
            for (int i = 0; i < ThreadManager.maxAllowedWorkerThreads; i++)
            {
                // Check for active thread
                if (i <= ThreadManager.maxAllowedWorkerThreads)
                {
                    // Try to get the value
                    float value = Performance.getUsageValue(i);

                    // Add the usage data
                    usageData[i].add(value);
                }
            }

            // Move to next sample
            Performance.stepSample();
        }

        private void setUpdateRate(int setting)
        {
            switch(setting)
            {
                case -1:
                    {
                        // Try to load
                        if (EditorPrefs.HasKey("AStar_Samples") == true)
                        {
                            int value = EditorPrefs.GetInt("AStar_Samples");

                            if (value == -1)
                                value = 1;

                            // Load from settings
                            setUpdateRate(value);
                        }
                        else
                        {
                            // Set default
                            setUpdateRate(1);
                        }
                    } return;

                case 0:
                    {
                        updateRate = 1.2f;
                        setActiveButton(0);
                    } break;
                case 1:
                    {
                        updateRate = 0.6f;
                        setActiveButton(1);
                    } break;
                case 2:
                    {
                        updateRate = 0.2f;
                        setActiveButton(2);
                    } break;
            }

            // Save the value
            EditorPrefs.SetInt("AStar_Samples", setting);
        }

        private void setActiveButton(int id)
        {
            // Disable all
            foreach (ToggleButton button in findControlsOfType<ToggleButton>())
                button.Checked = false;

            findControl<ToggleButton>(id.ToString()).Checked = true;
        }
    }
}
