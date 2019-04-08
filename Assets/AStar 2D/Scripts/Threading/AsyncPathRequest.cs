using UnityEngine;
using System;
using System.Collections;

using AStar_2D.Pathfinding;

namespace AStar_2D.Threading
{
    public sealed class AsyncPathRequest
    {
        // Private
        private SearchGrid grid = null;
        private Index start = Index.zero;
        private Index end = Index.zero;
        private PathRequestDelegate callback = null;
        private DiagonalMode diagonal = DiagonalMode.Diagonal;
        private long timeStamp = 0;

        // Properties
        public SearchGrid Grid
        {
            get { return grid; }
        }

        public Index Start
        {
            get { return start; }
        }

        public Index End
        {
            get { return end; }
        }

        public DiagonalMode Diagonal
        {
            get { return diagonal; }
        }

        internal PathRequestDelegate Callback
        {
            get { return callback; }
        }

        internal long TimeStamp
        {
            get { return timeStamp; }
        }

        // Constructor
        public AsyncPathRequest(SearchGrid grid, Index start, Index end, PathRequestDelegate callback)
        {
            this.grid = grid;
            this.start = start;
            this.end = end;
            this.callback = callback;

            // Create a time stamp
            timeStamp = DateTime.UtcNow.Ticks;
        }

        public AsyncPathRequest(SearchGrid grid, Index start, Index end, DiagonalMode diagonal, PathRequestDelegate callback)
        {
            this.grid = grid;
            this.start = start;
            this.end = end;
            this.diagonal = diagonal;
            this.callback = callback;

            // Create a time stamp
            timeStamp = DateTime.UtcNow.Ticks;
        }
    }
}
