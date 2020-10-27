﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Xunit
{
    public class Diff : IDiff
    {
        public class Options
        {
            public bool IgnoreWhiteSpace { get; set; }
            ///public bool IgnoreCase { get; set; }

            public static Options Default = new Options
            {
                //IgnoreCase = false,
                IgnoreWhiteSpace = false
            };
        }

        static Dictionary<ChangeType, char> ChangeSymbol = new Dictionary<ChangeType, char>
        {
            { ChangeType.Deleted, '-' },
            { ChangeType.Imaginary, '#' },
            { ChangeType.Inserted, '+' },
            { ChangeType.Modified, '~' },
            { ChangeType.Unchanged, ' ' },
        };

        static string DiffText(DiffPiece p) => $"{ChangeSymbol[p.Type]} {p.Text}";
        static string DiffLine(DiffPiece p) => string.Concat(p.SubPieces.Select(s => new string(ChangeSymbol[s.Type], s.Text?.Length ?? 0)));
        static string Padding(int length) => length >= 0 ? new string(' ', length) : "";

        static SideBySideDiffBuilder DiffBuilder = new SideBySideDiffBuilder(Differ.Instance, LineChunker.Instance, CharacterChunker.Instance);

        public string Generate(string left, string right, Options options = null)
        {
            var opts = options ?? Options.Default;

            var model = DiffBuilder.BuildDiffModel(left, right, opts.IgnoreWhiteSpace);

            var result = new StringBuilder();

            var oldMaxWidth = model.OldText.Lines.Max(s => s.Text.Length);
            var count = model.OldText.Lines.Count;

            for (var i = 0; i < count; i++)
            {
                var oldLine = model.OldText.Lines[i];
                var newLine = model.NewText.Lines[i];

                result
                    .Append(DiffText(oldLine))
                    .Append(Padding(oldMaxWidth - oldLine.Text.Length))
                    .Append(" | ")
                    .Append($"{DiffText(newLine)}\n");

                if (oldLine.Type == ChangeType.Modified)
                {
                    var c = $"  {DiffLine(oldLine)}";
                    result
                        .Append(c)
                        .Append(Padding(oldMaxWidth - c.Length + 2))
                        .Append(" | ")
                        .Append($"  {DiffLine(newLine)}\n");
                }
            }

            return result.ToString();
        }
    }
}
