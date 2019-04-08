using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AStar_2D.Threading;

namespace AStar_2D.Visualisation
{
    internal sealed class PerformanceSample
    {
        // Private
        private List<float> samples = new List<float>();

        // Methods
        public void addSample(float value)
        {
            // Add the value
            samples.Add(value);
        }

        public void clearSample()
        {
            samples.Clear();
        }

        public float getAverageUsage()
        {
            float accumulator = 0;

            foreach (float value in samples)
                accumulator += value;

            return accumulator / samples.Count;
        }

        public float getHighestUsage()
        {
            float highest = 0;

            foreach (float value in samples)
                if (value > highest)
                    highest = value;

            return highest;
        }
    }

    internal static class Performance
    {
        // Private
        private static PerformanceSample timing = new PerformanceSample();
        private static List<PerformanceSample> threadUsage = new List<PerformanceSample>();        

        // Properties
        public static IList<PerformanceSample> ThreadSamples
        {
            get { return threadUsage; }
        }

        public static void addTimingSample(float value)
        {
            // Lock the list
            lock(timing)
            {
                // Add the sample
                timing.addSample(value);
            }
        }

        public static void addUsageSample(int id, float normalizedUsage)
        {
            // Skip clamp because thread usage is normalized anyway
            lock(threadUsage)
            {
                // Add samples for the threads
                while (id >= threadUsage.Count)
                {
                    threadUsage.Add(new PerformanceSample());
                }

                // Add the sample
                threadUsage[id].addSample(normalizedUsage);
            }
        }

        public static float getAverageTimingValue()
        {
            lock(timing)
            {
                return timing.getAverageUsage();
            }
        }

        public static float getPeekTimingValue()
        {
            lock(timing)
            {
                return timing.getHighestUsage();
            }
        }

        public static float getUsageValue(int id)
        {
            lock(threadUsage)
            {
                if(id < threadUsage.Count && id >= 0)
                {
                    // Get the value
                    return threadUsage[id].getHighestUsage();
                }
            }
            return 0;
        }

        public static void stepSample()
        {
            // Clear threead usage
            foreach (PerformanceSample sample in threadUsage)
                sample.clearSample();

            // Clear timing
            timing.clearSample();
        }
    }
}
