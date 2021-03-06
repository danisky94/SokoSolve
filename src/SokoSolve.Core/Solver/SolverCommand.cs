﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Threading;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Debugger;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{

    public enum DuplicateMode
    {
        Discard,
        AddAsChild,
        ReuseInPool
    }
   


    public class SolverCommand
    {
        public SolverCommand()
        {
            Debug = NullDebugEventPublisher.Instance;
            CheckAbort = x => CancellationToken.IsCancellationRequested;
        }

        public SolverCommand(SolverCommand rhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            Puzzle = rhs.Puzzle;
            Report = rhs.Report;
            ExitConditions = rhs.ExitConditions;
            CheckAbort = rhs.CheckAbort;
            Progress = rhs.Progress;
            Debug = rhs.Debug;
            Parent = null;
            ServiceProvider = rhs.ServiceProvider;
            AggProgress = rhs.AggProgress;

            CheckAbort = x => CancellationToken.IsCancellationRequested;
        }

        public Puzzle?                    Puzzle            { get; set; }
        public ExitConditions?            ExitConditions    { get; set; }
        public ITextWriterBase?                Report            { get; set; }
        public IDebugEventPublisher       Debug             { get; set; }
        public Func<SolverCommand, bool>? CheckAbort        { get; set; }
        public CancellationToken          CancellationToken { get; set; } = new CancellationToken();
        public IProgressNotifier?         Progress          { get; set; }
        public ISolver?                   Parent            { get; set; }
        public ISolverContainer?          ServiceProvider   { get; set; }
        public IProgressNotifier?         AggProgress { get; set; }
        public DuplicateMode              DuplicateMode { get; set; }   
        public Func<SolverNode, bool>? Inspector { get; set; }
    }

    public class SolutionChain
    {
        public SolverNode?     ForwardNode { get; set; }
        public SolverNode?     ReverseNode { get; set; }
        public INodeEvaluator? FoundUsing  { get; set; }
    }

    public class SolverState
    {
        public SolverState()
        {
            Statistics = new SolverStatistics();
        }

        public SolverCommand              Command               { get; set; }
        public SolverStatistics            Statistics            { get; set; }
        public StaticAnalysisMaps          StaticMaps            { get; set; }
        public Exception?                  Exception             { get; set; }
        public bool                        EarlyExit             { get; set; }
        public string?                     ExitDescription       { get; set; }
        public List<SolverNode>?           SolutionsNodes        { get; set; }
        public List<SolutionChain>?        SolutionsNodesReverse { get; set; }
        public List<Path>?                 Solutions             { get; set; }
        public List<(Path, string error)>? SolutionsInvalid      { get; set; }
        public ExitConditions.Conditions   Exit                  { get; set; }
        public SolverResultSummary? Summary { get; set; }
        

        public bool HasSolution => 
            (SolutionsNodes != null && SolutionsNodes.Any()) || 
            (SolutionsNodesReverse != null && SolutionsNodesReverse.Any());

        
        public void ThrowErrors()
        {
            if (Exception != null) throw new Exception("Solver Failed", Exception);
        }

        public virtual SolverNode? GetRootForward() => null;
        public virtual SolverNode? GetRootReverse() => null;
    }
}