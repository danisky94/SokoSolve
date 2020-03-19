﻿using System.Collections.Generic;
using System.Threading;

namespace SokoSolve.Core.Solver
{
    public class ThreadSafeSolverNodeLookup : SolverNodeLookup
    {
        readonly object locker = new object();
       

        protected override void Flush()
        {
            lock (locker)
            {
                base.Flush();    
            }
        }
    }
}