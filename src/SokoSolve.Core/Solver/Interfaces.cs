using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public interface ISolver : IExtendedFunctionalityDescriptor 
    {
        SolverStatistics[]?  Statistics         { get; }
        int                 VersionMajor       { get; }
        int                 VersionMinor       { get; }
        int                 VersionUniversal   { get; }
        string              VersionDescription { get; }
        SolverState Init(SolverCommand command);
        
        ExitConditions.Conditions Solve(SolverState state);
    }
    

    public interface ISolverVisualisation
    {
        bool TrySample(out SolverNode? node);        // false = not supported
        IEnumerable<SolverNode> GetAll();        // throw notsupported
    }

    /// <summary>
    /// Prove more information that can be inferred from GetType()
    /// </summary>
    public interface IExtendedFunctionalityDescriptor
    {
        string TypeDescriptor { get; }
        IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state);  // throws NoSupported
    }
    
    public interface IProgressNotifier
    {
        void Update(ISolver caller, SolverState state, SolverStatistics global, string txt);
    }

    public interface IStatisticsProvider
    {
        SolverStatistics Statistics { get; }    // Not real-time accurate, do not use for functionality, only reporting
    }
    
    public interface ISolverQueue : IStatisticsProvider, IExtendedFunctionalityDescriptor
    {
        void Enqueue(SolverNode node);
        void Enqueue(IEnumerable<SolverNode> nodes);

        SolverNode?   Dequeue();
        SolverNode[]? Dequeue(int count);
    }

    public interface ISolverPool : IStatisticsProvider, IExtendedFunctionalityDescriptor
    {
        void Add(SolverNode node);
        void Add(IReadOnlyCollection<SolverNode> nodes);

        SolverNode? FindMatch(SolverNode find);
    }

    public interface ISolverPoolChained : ISolverPool
    {
        ISolverPool InnerPool { get;  }
    }
    
    
    public interface ISolverPoolBatching : ISolverPool
    {
        int MinBlockSize { get; }
        bool IsReadyToAdd(IReadOnlyCollection<SolverNode> buffer);
    }
    
    
    public interface ISolverNodeFactory : IExtendedFunctionalityDescriptor
    {
        bool TryGetPooledInstance(out SolverNode node);
        
        SolverNode CreateInstance(SolverNode parent, VectorInt2 player, VectorInt2 push, IBitmap crateMap, IBitmap moveMap);
        void       ReturnInstance(SolverNode canBeReused);
        IBitmap CreateBitmap(VectorInt2 size);
        IBitmap CreateBitmap(IBitmap clone);

        SolverNode CreateFromPush(SolverNode parent, IBitmap nodeCrateMap, IBitmap walls, VectorInt2 p, VectorInt2 pp, VectorInt2 ppp, VectorInt2 push);
        SolverNode CreateFromPull(SolverNode parent, IBitmap nodeCrateMap, IBitmap walls, VectorInt2 pc, VectorInt2 p, VectorInt2 pp);
    }

    public interface ISolveNodeFactoryPuzzleDependant : ISolverNodeFactory
    {
        void SetupForPuzzle(Puzzle commandPuzzle);
    }
    
    public interface INodeEvaluator
    {
        SolverNode Init(Puzzle puzzle, ISolverQueue queue);

        bool Evaluate(
            SolverState  state, 
            ISolverQueue queue,
            ISolverPool  pool,
            ISolverPool  solutionPool, 
            SolverNode   node);
    }
   
}