using Sokoban.Core.Primitives;
using Sokoban.Core.PuzzleLogic;

namespace Sokoban.Core.Analytics
{
    public class PuzzleAnalysis
    {
        public StaticMaps Static { get;  protected set; }

        public PuzzleAnalysis(Puzzle start)
        {
            Start = start;
            Static = StaticAnalysis.Generate(start);

            Static.DeadMap = DeadMapAnalysis.FindDeadMap(Static);
        }

        public PuzzleState Evalute(Puzzle current)
        {
            var crateMap = current.ToMap(current.Definition.AllCrates);
            return new PuzzleState()
            {
                Static = Static,
                Current = new StateMaps()
                {
                    CrateMap = crateMap,
                    MoveMap = FloodFill.Fill(Static.WallMap.BitwiseOR(crateMap), current.Player.Position)
                }
            };
        }

        public Puzzle Start { get; set; }
    }
}