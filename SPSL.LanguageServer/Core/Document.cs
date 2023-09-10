using System.Text;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace SPSL.LanguageServer.Core;

/// <summary>
/// Manages a single document and updates its content.
/// </summary>
public class Document
{
    private static bool IsIncrementalChange(TextDocumentContentChangeEvent change)
    {
        return change.Range is not null;
    }

    private static bool IsFullChange(TextDocumentContentChangeEvent change)
    {
        return change.Range is null;
    }

    private static Range GetWellFormedRange(Range range)
    {
        Position start = range.Start;
        Position end = range.End;

        if (start.Line > end.Line || (start.Line == end.Line && start.Character > end.Character))
        {
            return new() { Start = end, End = start };
        }

        return range;
    }

    private static List<int> ComputeLineOffsets(string text, bool isAtLineStart, int textOffset = 0)
    {
        List<int> result = new();
        if (isAtLineStart) result.Add(textOffset);

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];

            if (ch != 13 /* CharCode.CarriageReturn */ && ch != 10 /* CharCode.LineFeed */) continue;
            if (ch == 13 /* CharCode.CarriageReturn */ && i + 1 < text.Length &&
                text[i + 1] == 10 /* CharCode.LineFeed */)
                i++;

            result.Add(textOffset + i + 1);
        }

        return result;
    }

    private readonly StringBuilder _content = new();
    private List<int>? _lineOffsets;

    /// <summary>
    /// The document uri in the workspace.
    /// </summary>
    public DocumentUri Uri { get; init; }

    /// <summary>
    /// The language ID of the document.
    /// </summary>
    public string LanguageId { get; init; }

    /// <summary>
    /// The document version.
    /// </summary>
    public int? Version { get; set; }

    /// <summary>
    /// Gets the number of lines in this document.
    /// </summary>
    public int LineCount => GetLineOffsets().Count();

    /// <summary>
    /// Gets the number of characters in this document.
    /// </summary>
    public int Length => _content.Length;

    public Document(DocumentUri uri, string languageId, int? version = null, string? initialContent = null)
    {
        Uri = uri;
        LanguageId = languageId;
        Version = version;

        if (initialContent is not null)
            _content.Append(initialContent);
    }

    public string GetText(Range? range = null)
    {
        var str = _content.ToString();

        if (range is null)
            return str;

        var startOffset = OffsetAt(range.Start);
        var endOffset = OffsetAt(range.End);

        return str.Substring(startOffset, endOffset - startOffset + 1);
    }

    public void SetText(string text)
    {
        _content.Clear();
        _content.Append(text);

        _lineOffsets = null;
    }

    public void Update(Container<TextDocumentContentChangeEvent> changes, int? version = null)
    {
        foreach (TextDocumentContentChangeEvent change in changes)
        {
            if (IsIncrementalChange(change))
            {
                // Clean range
                Range range = GetWellFormedRange(change.Range!);

                // Update content
                var startOffset = OffsetAt(range.Start);
                var endOffset = OffsetAt(range.End);
                var content = _content.ToString();

                _content.Clear();
                _content.Append(content[..startOffset]);
                _content.Append(change.Text);
                _content.Append(content[endOffset..]);

                // Update offsets
                var startLine = Math.Max(range.Start.Line, 0);
                var endLine = Math.Max(range.End.Line, 0);
                var lineOffsets = _lineOffsets!;
                var addedLineOffsets = ComputeLineOffsets(change.Text, false, startOffset);
                if (endLine - startLine == addedLineOffsets.Count)
                {
                    for (var i = 0; i < addedLineOffsets.Count; i++)
                        lineOffsets[i + startLine + 1] = addedLineOffsets[i];
                }
                else
                {
                    lineOffsets.RemoveRange(startLine + 1, endLine - startLine);
                    lineOffsets.InsertRange(startLine + 1, addedLineOffsets);
                }

                var diff = change.Text.Length - (endOffset - startOffset);
                if (diff == 0) continue;

                for (int i = startLine + 1 + addedLineOffsets.Count, l = lineOffsets.Count; i < l; i++)
                    lineOffsets[i] += diff;
            }
            else if (IsFullChange(change))
            {
                _content.Clear();
                _content.Append(change.Text);

                _lineOffsets = null;
            }
            else
            {
                throw new ArgumentException
                (
                    "Encountered a change event which is neither full nor incremental.",
                    nameof(changes)
                );
            }
        }

        Version = version;
    }

    public IEnumerable<int> GetLineOffsets()
    {
        return _lineOffsets ??= ComputeLineOffsets(_content.ToString(), true);
    }

    public Position PositionAt(int offset)
    {
        offset = Math.Max(Math.Min(offset, _content.Length), 0);
        var lineOffsets = GetLineOffsets() as List<int> ?? GetLineOffsets().ToList();
        int low = 0, high = lineOffsets.Count;
        if (high == 0)
            return new Position { Line = 0, Character = offset };

        while (low < high)
        {
            var mid = (int)Math.Floor((low + high) / 2.0);
            if (lineOffsets[mid] > offset)
                high = mid;
            else
                low = mid + 1;
        }

        var line = low - 1;
        return new Position { Line = line, Character = offset - lineOffsets[line] };
    }

    public int OffsetAt(Position position)
    {
        var lineOffsets = GetLineOffsets() as List<int> ?? GetLineOffsets().ToList();

        if (position.Line >= lineOffsets.Count)
            return _content.Length;

        if (position.Line < 0)
            return 0;

        var lineOffset = lineOffsets[position.Line];
        var nextLineOffset = (position.Line + 1 < lineOffsets.Count) ? lineOffsets[position.Line + 1] : _content.Length;

        return Math.Max(Math.Min(lineOffset + position.Character, nextLineOffset), lineOffset);
    }
}