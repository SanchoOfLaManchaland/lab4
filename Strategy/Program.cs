using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public abstract class LightNode
{
    public abstract string ToHtml();
    public abstract string GetInnerHtml();
}

public class LightTextNode : LightNode
{
    public string Text { get; private set; }

    public LightTextNode(string text)
    {
        Text = text ?? string.Empty;
    }

    public override string ToHtml()
    {
        return Text;
    }

    public override string GetInnerHtml()
    {
        return Text;
    }
}

public enum DisplayType
{
    Block,
    Inline
}

public enum TagClosingType
{
    SelfClosing,
    WithClosingTag
}

public interface IImageLoadingStrategy
{
    Task<bool> LoadImageAsync(string source);
    string GetImageInfo(string source);
}

public class FileSystemImageStrategy : IImageLoadingStrategy
{
    public Task<bool> LoadImageAsync(string source)
    {
        return Task.FromResult(File.Exists(source));
    }

    public string GetImageInfo(string source)
    {
        if (File.Exists(source))
        {
            var fileInfo = new FileInfo(source);
            return $"Локальний файл: {fileInfo.Name}, Розмір: {fileInfo.Length} байт";
        }
        return "Файл не знайдено";
    }
}

public class NetworkImageStrategy : IImageLoadingStrategy
{
    private static readonly HttpClient httpClient = new HttpClient();

    public async Task<bool> LoadImageAsync(string source)
    {
        try
        {
            var response = await httpClient.GetAsync(source, HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public string GetImageInfo(string source)
    {
        return $"Мережеве зображення: {source}";
    }
}

public class LightElementNode : LightNode
{
    public string TagName { get; private set; }
    public DisplayType DisplayType { get; private set; }
    public TagClosingType ClosingType { get; private set; }
    public List<string> CssClasses { get; private set; }
    public List<LightNode> Children { get; private set; }
    public Dictionary<string, string> Attributes { get; private set; }

    public int ChildrenCount => Children.Count;

    public LightElementNode(string tagName, DisplayType displayType = DisplayType.Block,
                          TagClosingType closingType = TagClosingType.WithClosingTag)
    {
        TagName = tagName?.ToLower() ?? throw new ArgumentNullException(nameof(tagName));
        DisplayType = displayType;
        ClosingType = closingType;
        CssClasses = new List<string>();
        Children = new List<LightNode>();
        Attributes = new Dictionary<string, string>();
    }

    public LightElementNode AddClass(string cssClass)
    {
        if (!string.IsNullOrWhiteSpace(cssClass) && !CssClasses.Contains(cssClass))
        {
            CssClasses.Add(cssClass);
        }
        return this;
    }

    public LightElementNode AddAttribute(string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Attributes[name] = value ?? string.Empty;
        }
        return this;
    }

    public LightElementNode AddChild(LightNode child)
    {
        if (child != null && ClosingType == TagClosingType.WithClosingTag)
        {
            Children.Add(child);
        }
        return this;
    }

    public LightElementNode AddText(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            AddChild(new LightTextNode(text));
        }
        return this;
    }

    public override string GetInnerHtml()
    {
        if (ClosingType == TagClosingType.SelfClosing)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var child in Children)
        {
            sb.Append(child.ToHtml());
        }
        return sb.ToString();
    }

    public override string ToHtml()
    {
        var sb = new StringBuilder();

        sb.Append($"<{TagName}");

        foreach (var attr in Attributes)
        {
            sb.Append($" {attr.Key}=\"{attr.Value}\"");
        }

        if (CssClasses.Any())
        {
            var existingClass = Attributes.ContainsKey("class") ? Attributes["class"] + " " : "";
            sb.Append($" class=\"{existingClass}{string.Join(" ", CssClasses)}\"");
        }

        if (ClosingType == TagClosingType.SelfClosing)
        {
            sb.Append("/>");
        }
        else
        {
            sb.Append(">");
            sb.Append(GetInnerHtml());
            sb.Append($"</{TagName}>");
        }

        return sb.ToString();
    }

    public string ToFormattedHtml(int indent = 0)
    {
        var sb = new StringBuilder();
        var indentStr = new string(' ', indent * 2);

        sb.Append($"{indentStr}<{TagName}");

        foreach (var attr in Attributes)
        {
            sb.Append($" {attr.Key}=\"{attr.Value}\"");
        }

        if (CssClasses.Any())
        {
            var existingClass = Attributes.ContainsKey("class") ? Attributes["class"] + " " : "";
            sb.Append($" class=\"{existingClass}{string.Join(" ", CssClasses)}\"");
        }

        if (ClosingType == TagClosingType.SelfClosing)
        {
            sb.AppendLine("/>");
        }
        else
        {
            sb.AppendLine(">");

            foreach (var child in Children)
            {
                if (child is LightElementNode elementChild)
                {
                    sb.Append(elementChild.ToFormattedHtml(indent + 1));
                }
                else
                {
                    sb.AppendLine($"{new string(' ', (indent + 1) * 2)}{child.ToHtml()}");
                }
            }

            sb.AppendLine($"{indentStr}</{TagName}>");
        }

        return sb.ToString();
    }
}

public class LightImageNode : LightNode
{
    private IImageLoadingStrategy _loadingStrategy;
    private string _source;
    private string _altText;
    private Dictionary<string, string> _attributes;
    private List<string> _cssClasses;

    public string Source { get { return _source; } }
    public string AltText { get { return _altText; } }
    public bool IsLoaded { get; private set; }

    public LightImageNode(string source, string altText = "")
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _altText = altText ?? "";
        _attributes = new Dictionary<string, string>();
        _cssClasses = new List<string>();

        _loadingStrategy = IsNetworkUrl(source)
            ? (IImageLoadingStrategy)new NetworkImageStrategy()
            : (IImageLoadingStrategy)new FileSystemImageStrategy();
    }

    private bool IsNetworkUrl(string source)
    {
        return source.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               source.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    public LightImageNode AddAttribute(string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _attributes[name] = value ?? string.Empty;
        }
        return this;
    }

    public LightImageNode AddClass(string cssClass)
    {
        if (!string.IsNullOrWhiteSpace(cssClass) && !_cssClasses.Contains(cssClass))
        {
            _cssClasses.Add(cssClass);
        }
        return this;
    }

    public async Task<bool> LoadAsync()
    {
        IsLoaded = await _loadingStrategy.LoadImageAsync(_source);
        return IsLoaded;
    }

    public string GetImageInfo()
    {
        return _loadingStrategy.GetImageInfo(_source);
    }

    public override string ToHtml()
    {
        var sb = new StringBuilder();
        sb.Append("<img");

        sb.Append($" src=\"{_source}\"");
        sb.Append($" alt=\"{_altText}\"");

        foreach (var attr in _attributes)
        {
            sb.Append($" {attr.Key}=\"{attr.Value}\"");
        }

        if (_cssClasses.Any())
        {
            var existingClass = _attributes.ContainsKey("class") ? _attributes["class"] + " " : "";
            sb.Append($" class=\"{existingClass}{string.Join(" ", _cssClasses)}\"");
        }

        sb.Append("/>");
        return sb.ToString();
    }

    public override string GetInnerHtml()
    {
        return string.Empty; 
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Демонстрація роботи LightHTML з Image елементом ===\n");

        var table = new LightElementNode("table", DisplayType.Block)
            .AddClass("student-table")
            .AddClass("bordered")
            .AddAttribute("border", "1")
            .AddAttribute("cellpadding", "5");

        var thead = new LightElementNode("thead", DisplayType.Block);
        var headerRow = new LightElementNode("tr", DisplayType.Block);

        headerRow.AddChild(new LightElementNode("th", DisplayType.Block).AddText("№"))
                 .AddChild(new LightElementNode("th", DisplayType.Block).AddText("Ім'я"))
                 .AddChild(new LightElementNode("th", DisplayType.Block).AddText("Спеціальність"))
                 .AddChild(new LightElementNode("th", DisplayType.Block).AddText("Курс"));

        thead.AddChild(headerRow);
        table.AddChild(thead);

        Console.WriteLine("1. Оригінальна таблиця створена успішно");
        Console.WriteLine($"   Кількість дочірніх елементів: {table.ChildrenCount}");

        Console.WriteLine("\n2. Демонстрація Image елементу:");

        var localImage = new LightImageNode("./images/photo.jpg", "Локальне фото студента")
            .AddClass("student-photo")
            .AddAttribute("width", "100")
            .AddAttribute("height", "100");

        Console.WriteLine($"   Локальне зображення:");
        Console.WriteLine($"   - Джерело: {localImage.Source}");
        Console.WriteLine($"   - HTML: {localImage.ToHtml()}");
        Console.WriteLine($"   - Інформація: {localImage.GetImageInfo()}");

        bool localLoaded = await localImage.LoadAsync();
        Console.WriteLine($"   - Завантажено: {(localLoaded ? "Так" : "Ні")}");

        var networkImage = new LightImageNode("https://via.placeholder.com/150", "Placeholder зображення")
            .AddClass("placeholder-image")
            .AddAttribute("width", "150")
            .AddAttribute("height", "150");

        Console.WriteLine($"\n   Мережеве зображення:");
        Console.WriteLine($"   - Джерело: {networkImage.Source}");
        Console.WriteLine($"   - HTML: {networkImage.ToHtml()}");
        Console.WriteLine($"   - Інформація: {networkImage.GetImageInfo()}");

        Console.WriteLine("   - Спроба завантаження...");
        bool networkLoaded = await networkImage.LoadAsync();
        Console.WriteLine($"   - Завантажено: {(networkLoaded ? "Так" : "Ні")}");

        Console.WriteLine("\n3. Створення HTML сторінки з зображеннями:");

        var htmlPage = new LightElementNode("html")
            .AddChild(new LightElementNode("head")
                .AddChild(new LightElementNode("title").AddText("Демонстрація LightHTML Image")))
            .AddChild(new LightElementNode("body")
                .AddChild(new LightElementNode("h1").AddText("Галерея зображень"))
                .AddChild(new LightElementNode("div")
                    .AddClass("image-gallery")
                    .AddChild(localImage)
                    .AddChild(networkImage))
                .AddChild(new LightElementNode("hr", DisplayType.Block, TagClosingType.SelfClosing))
                .AddChild(table));

        Console.WriteLine("   HTML сторінка (форматована):");
        Console.WriteLine(htmlPage.ToFormattedHtml());

        Console.WriteLine("\n4. Тестування різних типів джерел:");

        var images = new LightImageNode[]
        {
            new LightImageNode("C:\\temp\\image.png", "Windows шлях"),
            new LightImageNode("/usr/local/images/photo.jpg", "Unix шлях"),
            new LightImageNode("https://example.com/image.gif", "HTTPS URL"),
            new LightImageNode("http://test.com/photo.png", "HTTP URL")
        };

        foreach (var img in images)
        {
            Console.WriteLine($"   - {img.GetImageInfo()}");
            Console.WriteLine($"     HTML: {img.ToHtml()}");
        }

        Console.WriteLine("\n=== Кінець демонстрації ===");
    }
}