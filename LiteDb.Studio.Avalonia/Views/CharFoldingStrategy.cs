using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace LiteDb.Studio.Avalonia.Views;

public class CharFoldingStrategy
{
    private readonly char _openingChar;
    private readonly char _closingChar;

    public CharFoldingStrategy(char openingChar, char closingChar)
    {
        _openingChar = openingChar;
        _closingChar = closingChar;
    }

    private List<NewFolding> CreateNewFoldings(ITextSource document)
    {
        var newFoldings = new List<NewFolding>();
        var startOffsets = new Stack<int>();
        int lastNewLineOffset = 0;

        for (int i = 0; i < document.TextLength; i++)
        {
            char c = document.GetCharAt(i);
            if (c == _openingChar)
            {
                startOffsets.Push(i);
            }
            else if (c == _closingChar && startOffsets.Count > 0)
            {
                int startOffset = startOffsets.Pop();
                // don't fold if opening and closing brace are on the same line
                if (startOffset < lastNewLineOffset)
                {
                    newFoldings.Add(new NewFolding(startOffset, i + 1));
                }
            }
            else if (c == '\n' || c == '\r')
            {
                lastNewLineOffset = i + 1;
            }
        }

        newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
        return newFoldings;
    }

    private List<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
    {
        firstErrorOffset = -1;
        return CreateNewFoldings((ITextSource)document);
    }

    public void UpdateFoldings(FoldingManager manager, TextDocument document)
    {
        int firstErrorOffset;
        var newFoldings = CreateNewFoldings(document, out firstErrorOffset);
        manager.UpdateFoldings(newFoldings, firstErrorOffset);
    }
}