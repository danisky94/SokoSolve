using System;
using System.Collections.Generic;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class SingleThreadedReverseSolver : SolverBase
    {
        public SingleThreadedReverseSolver(ISolverNodeFactory nodeFactory)
            : base(new ReverseEvaluator(nodeFactory))
        {
        }

    }
}