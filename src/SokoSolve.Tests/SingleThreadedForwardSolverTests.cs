﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SokoSolve.Core.Common;
using SokoSolve.Core.PuzzleLogic;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests
{
    [TestFixture]
    public class SingleThreadedForwardSolverTests
    {
        

        void PuzzleShouldHaveSolution(ISolver solver, Puzzle puzzle, ExitConditions exit = null, bool verbose = false)
        {
            if (exit == null)
            {
                exit = new ExitConditions()
                {
                    Duration = TimeSpan.FromSeconds(60),
                    StopOnSolution = true,
                    TotalNodes = int.MaxValue,
                    TotalDead = int.MaxValue
                };
            }
            var command = new SolverCommand()
            {
                Puzzle = puzzle,
                Report = Console.Out,
                Progress = new ConsoleProgressNotifier(),
                ExitConditions = exit
            };

            // act 
            var result = solver.Init(command);
            solver.Solve(result);
            Console.WriteLine(result.ExitDescription);
            Console.WriteLine(SolverHelper.Summary(result));
            result.ThrowErrors();

            // assert    
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasSolution, Is.True);

            foreach (var sol in result.GetSolutions())
            {
                Console.WriteLine("Path: {0}", sol);
                string error = null;

                Assert.That(SolverHelper.CheckSolution(command.Puzzle, sol, out error), "Solution is INVALID! "+error);
            }
        }

        [Test] public void T001_Trivial()
        {
            PuzzleShouldHaveSolution(
                new SingleThreadedForwardSolver(),
                new Puzzle(new String[]
                {
                    "##########",
                    "#O...X...#",
                    "#O..XPX.O#",
                    "##########"
                }));
        }

        [Test] public void T002_BaseLine()
        {
            PuzzleShouldHaveSolution(
               new SingleThreadedForwardSolver(),
               new Puzzle());
        }


        [Test]
        [Ignore("Performance Benchmark")]
        public void SingleThreadedForwardSolver_TreePoolPerformance()
        {
            // IDEA: This puzzle has a relatively large amount of FLOOR space, which will create lots of Nodes in the TreePool

            // arrange
            var solver = new SingleThreadedForwardSolver();
            var command = new SolverCommand()
            {
                Puzzle = new Puzzle(new String[]
                {
                    "##########",
                    "#O.......#",
                    "#O.......#",
                    "#O.......#",
                    "#....X...#",
                    "#...XPX..#",
                    "#........#",
                    "##########"
                }),
                Report = Console.Out,
                ExitConditions = ExitConditions.OneMinute
            };

            // act 
            var result = solver.Init(command) as SolverBase.CommandResult;
            solver.Solve(result);
            Console.WriteLine(result.ExitDescription);
            Console.WriteLine(SolverHelper.Summary(result));
            result.ThrowErrors();

            // assert    
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [Ignore("Performance Benchmark")]
        public void SingleThreadedForwardSolver_TreePoolPerformance_Large()
        { 
            Console.WriteLine("IDEA: This puzzle has a relatively large amount of FLOOR space, which will create lots of Nodes in the TreePool");
            Console.WriteLine("[WILLOW: Started prep for multi-threading] 52000 nodes at 851.658640168867 nodes/sec. Exited EARLY. Time => Nodes: 52000, Dead: 0, Duration: 61.0573269 sec., Duplicates: 236306");
            Console.WriteLine("[WILLOW: Collection optimistion] 59000 nodes at 980.895125589307 nodes/sec. Exited EARLY. Time => Nodes: 59000, Dead: 0, Duration: 60.1491418 sec., Duplicates: 274401");
            Console.WriteLine("[Added Buffered inserts] 68000 nodes at 1129.36134313775 nodes/sec. Exited EARLY. Time => Nodes: 68000, Dead: 0, Duration: 60.2110214 sec., Duplicates: 476102");
            

            // arrange
            var solver = new SingleThreadedForwardSolver();
            var command = new SolverCommand()
            {
                Puzzle = new Puzzle(new String[]
                {
                    "##########",
                    "#O.......#",
                    "#O.....X.#",
                    "#O.......#",
                    "#........#",
                    "#........#",
                    "#....X...#",
                    "#...XPX..#",
                    "#....O...#",
                    "##########"
                }),
                Report = Console.Out,
                ExitConditions = new ExitConditions()
                {
                    Duration = TimeSpan.FromMinutes(1),
                    TotalNodes =  int.MaxValue,
                    TotalDead = int.MaxValue,
                    StopOnSolution = false // !!!!!!
                },
                Progress = new ConsoleProgressNotifier()

            };

            // act 
            var result = solver.Init(command) as SolverBase.CommandResult;
            solver.Solve(result);
            Console.WriteLine(SolverHelper.Summary(result));
            result.ThrowErrors();

            Console.WriteLine("All Nodes");
            Console.WriteLine( (result as SingleThreadedForwardSolver.CommandResult).Pool.ToString());

            // assert    
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [Ignore("Performance Benchmark")]
        public void SingleThreadedForwardSolver_BaseLine_Quick()
        {
            // arrange
            var solver = new SingleThreadedForwardSolver();
            var command = new SolverCommand()
            {
                Puzzle = new Puzzle(),
                Report = Console.Out,
                ExitConditions = new ExitConditions()
                {
                    Duration = TimeSpan.FromSeconds(10),
                    StopOnSolution = true,
                    TotalNodes = 10000
                }
            };

            // act 
            var result = solver.Init(command) as SolverBase.CommandResult;
            solver.Solve(result);
            Console.WriteLine(result.ExitDescription);
            Console.WriteLine(SolverHelper.Summary(result));
            result.ThrowErrors();

            // assert    
            Assert.That(result, Is.Not.Null);
            
        }
    }
}
