﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Sokoban.Core.Analytics;
using Sokoban.Core.Game;
using Sokoban.Core.PuzzleLogic;
using SokoSolve.Core.Model.DataModel;
using Path = System.IO.Path;

namespace Sokoban.Core.Library
{
    public class LibraryComponent
    {
        public string GetPathData(string rel)
        {
            return Path.Combine(Properties.Settings.Default.PathData, rel);
        }

        public Collection GetCollection()
        {
            var col = new Collection()
            {
                Libraries = new List<string>()
            };
            foreach (var line in File.ReadAllLines(GetPathData("Collections.dat")))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                col.Libraries.Add(line);
            }
            return col;
        }

        public Library LoadLibrary(string fileName)
        {
            return LoadLegacySokoSolve_SSX(fileName);
        }

        public void SaveLibrary(Library lib, string fileName)
        {
            throw new NotImplementedException();
        }

        public Profile LoadProfile(string fileName)
        {
            var profile = new Profile()
            {
                FileName = fileName,
                Current = new PuzzleIdent(),
                Statistics = new Statistics()
            };
            var pairs = TrivialNameValueFileFormat.Load(fileName);
            var binder = new TrivialNameValueFileFormat.WithBinder<Profile>();
            foreach (var pair in pairs)
            {
                binder.SetWhen(pair, profile, x=>x.Name);
                binder.SetWhen(pair, profile, x => x.Created);
                binder.SetWhen(pair, profile, x => x.TimeInGame);
                binder.With(profile, x => x.Current)
                    .SetWhen(pair, x=>x.Library)
                    .SetWhen(pair, x => x.Puzzle);
                binder.With(profile, x => x.Statistics)
                    .SetWhen(pair, x => x.Pushes)
                    .SetWhen(pair, x => x.Steps)
                    .SetWhen(pair, x => x.Started)
                    .SetWhen(pair, x => x.Undos)
                    .SetWhen(pair, x => x.Restarts)
                    ;

            }
            return profile;
        }


        public TrivialNameValueFileFormat SaveProfile(Profile lib, string fileName)
        {
            var txt = TrivialNameValueFileFormat.Serialise(lib);
            txt.Save(fileName);
            return txt;
        }

        public Library LoadLegacySokoSolve_SSX(string fileName)
        {
            var ser = new XmlSerializer(typeof(SokobanLibrary));

            SokobanLibrary xmlLib = null;
            using (var reader = File.OpenText(fileName))
            {
                xmlLib = (SokobanLibrary)ser.Deserialize(reader);
            }
            var lib = Convert(xmlLib);
            var cc = 0;
            foreach (var puzzle in lib)
            {
                puzzle.Rating = StaticAnalysis.CalculateRating(puzzle);
                puzzle.Ident = new PuzzleIdent()
                {
                    Library = fileName,
                    Puzzle = !string.IsNullOrWhiteSpace(puzzle.Name) ? puzzle.Name : cc.ToString()
                };
            }
            return lib;
        }

        private Library Convert(SokobanLibrary xmlLib)
        {
            var lib = new Library();
            foreach (var puzzle in xmlLib.Puzzles)
            {
                var map = puzzle.Maps.FirstOrDefault();
                if (map != null && map.Row != null)
                {
                    lib.Add(Convert(puzzle, map));
                }
            }
            return lib;
        }

        private LibraryPuzzle Convert(SokobanLibraryPuzzle xmlLib, SokobanLibraryPuzzleMap xp)
        {
            return new LibraryPuzzle(new Puzzle(xp.Row))
            {
                Name = xmlLib.PuzzleDescription != null ? xmlLib.PuzzleDescription.Name : null,
                Details = new AuthoredItem()
                {
                    Name = xmlLib.PuzzleDescription != null ? xmlLib.PuzzleDescription.Name : null
                }
            };
        }

        public List<Puzzle> LoadAllPuzzles(IEnumerable<PuzzleIdent> idents)
        {
            var libs = idents.Select(x => x.Library)
                .Distinct()
                .Select<string, Tuple<string, Library>>(
                    x => new Tuple<string, Library>(x, LoadLibrary(GetPathData(x))))
                .ToDictionary(x => x.Item1, x => x.Item2);
                
            var res = new List<Puzzle>();
            foreach (var ident in idents)
            {
                var p = libs[ident.Library][ident.Puzzle];
                if (p == null) throw new Exception("Ident not found:"+ident);
                res.Add(p);
            }
            return res;
        }
    }
}