using System;
using System.Collections.Generic;

public class DocumentMemento
{
    private readonly string content;
    private readonly DateTime timestamp;

    public DocumentMemento(string content)
    {
        this.content = content;
        this.timestamp = DateTime.Now;
    }

    public string GetContent() => content;
    public DateTime GetTimestamp() => timestamp;
}

public class TextDocument
{
    private string content;

    public TextDocument()
    {
        content = string.Empty;
    }

    public string Content
    {
        get => content;
        set => content = value ?? string.Empty;
    }

    public DocumentMemento CreateMemento()
    {
        return new DocumentMemento(content);
    }

    public void RestoreFromMemento(DocumentMemento memento)
    {
        if (memento != null)
        {
            content = memento.GetContent();
        }
    }

    public void AppendText(string text)
    {
        content += text;
    }

    public void SetText(string text)
    {
        content = text ?? string.Empty;
    }

    public void Clear()
    {
        content = string.Empty;
    }

    public int Length => content.Length;

    public override string ToString()
    {
        return content;
    }
}

public class DocumentHistory
{
    private readonly Stack<DocumentMemento> history;
    private readonly int maxHistorySize;

    public DocumentHistory(int maxHistorySize = 10)
    {
        this.maxHistorySize = maxHistorySize;
        history = new Stack<DocumentMemento>();
    }

    public void SaveState(DocumentMemento memento)
    {
        if (history.Count >= maxHistorySize)
        {
            var tempStack = new Stack<DocumentMemento>();
            for (int i = 0; i < maxHistorySize - 1; i++)
            {
                if (history.Count > 0)
                {
                    tempStack.Push(history.Pop());
                }
            }
            history.Clear();
            while (tempStack.Count > 0)
            {
                history.Push(tempStack.Pop());
            }
        }

        history.Push(memento);
    }

    public DocumentMemento GetPreviousState()
    {
        return history.Count > 0 ? history.Pop() : null;
    }

    public bool HasPreviousState => history.Count > 0;

    public int HistoryCount => history.Count;

    public void ClearHistory()
    {
        history.Clear();
    }
}

public class TextEditor
{
    private readonly TextDocument document;
    private readonly DocumentHistory history;

    public TextEditor()
    {
        document = new TextDocument();
        history = new DocumentHistory();
    }

    public string Content => document.Content;
    public int DocumentLength => document.Length;
    public bool CanUndo => history.HasPreviousState;

    private void SaveCurrentState()
    {
        var memento = document.CreateMemento();
        history.SaveState(memento);
    }

    public void WriteText(string text)
    {
        SaveCurrentState();
        document.SetText(text);
    }

    public void AppendText(string text)
    {
        SaveCurrentState();
        document.AppendText(text);
    }

    public void ClearDocument()
    {
        SaveCurrentState();
        document.Clear();
    }

    public bool Undo()
    {
        var previousState = history.GetPreviousState();
        if (previousState != null)
        {
            document.RestoreFromMemento(previousState);
            return true;
        }
        return false;
    }

    public int HistoryCount => history.HistoryCount;

    public void ClearHistory()
    {
        history.ClearHistory();
    }

    public void DisplayDocument()
    {
        Console.WriteLine($"Документ ({document.Length} символів):");
        Console.WriteLine($"'{document}'");
        Console.WriteLine($"Доступно скасувань: {history.HistoryCount}");
        Console.WriteLine();
    }
}

public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Демонстрація текстового редактора з Мементо ===\n");

        var editor = new TextEditor();

        Console.WriteLine("1. Створюємо новий документ");
        editor.DisplayDocument();

        Console.WriteLine("2. Додаємо текст 'Привіт, '");
        editor.WriteText("Привіт, ");
        editor.DisplayDocument();

        Console.WriteLine("3. Додаємо текст 'світ!'");
        editor.AppendText("світ!");
        editor.DisplayDocument();

        Console.WriteLine("4. Замінюємо весь текст на 'Новий текст документа'");
        editor.WriteText("Новий текст документа");
        editor.DisplayDocument();

        Console.WriteLine("5. Додаємо ще текст ' з доповненням'");
        editor.AppendText(" з доповненням");
        editor.DisplayDocument();

        Console.WriteLine("=== Скасування операцій ===\n");

        Console.WriteLine("6. Скасовуємо останню операцію (Undo)");
        if (editor.Undo())
        {
            Console.WriteLine("Операцію скасовано успішно!");
        }
        editor.DisplayDocument();

        Console.WriteLine("7. Ще одне скасування (Undo)");
        if (editor.Undo())
        {
            Console.WriteLine("Операцію скасовано успішно!");
        }
        editor.DisplayDocument();

        Console.WriteLine("8. Ще одне скасування (Undo)");
        if (editor.Undo())
        {
            Console.WriteLine("Операцію скасовано успішно!");
        }
        editor.DisplayDocument();

        Console.WriteLine("9. Спробуємо скасувати всі залишкові операції:");
        while (editor.CanUndo)
        {
            editor.Undo();
            Console.WriteLine($"Скасовано. Залишилось скасувань: {editor.HistoryCount}");
        }
        editor.DisplayDocument();

        Console.WriteLine("10. Спроба скасування, коли історія пуста:");
        bool undoResult = editor.Undo();
        Console.WriteLine($"Результат скасування: {(undoResult ? "Успішно" : "Неможливо - історія пуста")}");

        Console.WriteLine("\n=== Нові операції після скасувань ===\n");

        editor.WriteText("Відновлена робота");
        editor.DisplayDocument();

        editor.AppendText(" з редактором");
        editor.DisplayDocument();

        Console.WriteLine("Фінальне скасування:");
        editor.Undo();
        editor.DisplayDocument();
    }
}