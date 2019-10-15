﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Library;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Puzzle
{
   
    public interface IPosition
    {
        VectorInt2 Position { get; set; }
    }

    public struct Tile : IPosition
    {
        public VectorInt2 Position { get; set; }
        public CellDefinition<> State { get; set; }
    }

    public class Puzzle : Puzzle<CharCellDefinition>
    {
        
    }

    public abstract class Puzzle<T> : IEnumerable<Tile>
    {
        private readonly List<List<T>> map;    // TODO: Map?

        #region TODO_MoveTo_BuilderPattern
       
        public Puzzle() : this(TestLibrary.Default)
        {
        }

        public Puzzle(IEnumerable<string> puzzleStr)
        {
            map = puzzleStr.Select(x => new List<char>(x)).ToList();
        }

        public Puzzle(Puzzle puzzle)
        {
            map = new List<List<char>>(puzzle.map.Select(x => new List<char>(x)));
            Definition = puzzle.Definition;
        }

        public Puzzle(string puzzleText) : this(puzzleText.Split('\n').Select(x => x.Trim('\r'))
            .Where(x => x.Length > 0))
        {
        }
        
        #endregion

        public CharCellDefinition.Set Definition { get; } = CharCellDefinition.Default;

        public T this[int x, int y]
        {
            get => map[y][x];
            set => map[y][x] = value;
        }

        public T this[VectorInt2 p]
        {
            get => map[p.Y][p.X];
            set => map[p.Y][p.X] = value;
        }

        public int Width => map[0].Count;
        public int Height => map.Count;

        // Additional metadata (should not be here; just for convience)

        public string Name { get; set; }
        public object Tag { get; set; }

        public Tile Player
        {
            get
            {
                foreach (var c in this)
                    if (c.State == CellEnum.PlayerFloor || c.State == CellEnum.PlayerGoalFloor)
                        return c;
                return new Tile
                {
                    Position = new VectorInt2(-1, -1)
                };
            }
        }

        public RectInt2 Area =>
            new RectInt2
            {
                Size = new VectorInt2(Width, Height)
            };

        public bool IsSolved => Count(Definition.Crate) == 0;


        public IEnumerator<Tile> GetEnumerator()
        {
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                yield return new Tile
                {
                    Position = new VectorInt2(x, y),
                    State = this[x, y]
                };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Puzzle Clone()
        {
            return new Puzzle(this);
        }


        public bool Contains(VectorInt2 v)
        {
            if (v.X < 0 || v.Y < 0) return false;
            if (v.X >= Width || v.Y >= Height) return false;
            return true;
        }

        public Bitmap ToMap(T c)
        {
            return Bitmap.Create(new VectorInt2(Width, Height), Where(x => x == c).Select(x => x.Position));
        }

        public Bitmap ToMap(params T[] c)
        {
            return Bitmap.Create(new VectorInt2(Width, Height), Where(x => c.Contains(x)).Select(x => x.Position));
        }

        public IEnumerable<Tile> Where(Func<T, bool> where)
        {
            foreach (var cell in this)
                if (where(cell.State))
                    yield return cell;
        }


        public int Count(T state)
        {
            var cc = 0;
            foreach (var c in this)
                if (c.State == state)
                    cc++;
            return cc;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++) sb.Append(this[x, y]);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public List<string> ToStringList()
        {
            var res = new List<string>();
            for (var y = 0; y < Height; y++)
            {
                var sb = new StringBuilder();
                for (var x = 0; x < Width; x++) sb.Append(this[x, y]);
                res.Add(sb.ToString());
            }

            return res;
        }
    }
}