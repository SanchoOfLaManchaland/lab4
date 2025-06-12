using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public delegate void EventHandler(object sender, LightEventArgs e);

public class LightEventArgs : EventArgs
{
    public string EventType { get; }
    public LightElementNode Target { get; }
    public Dictionary<string, object> Data { get; }

    public LightEventArgs(string eventType, LightElementNode target, Dictionary<string, object> data = null)
    {
        EventType = eventType;
        Target = target;
        Data = data ?? new Dictionary<string, object>();
    }
}

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

public class LightElementNode : LightNode
{
    public string TagName { get; private set; }
    public DisplayType DisplayType { get; private set; }
    public TagClosingType ClosingType { get; private set; }
    public List<string> CssClasses { get; private set; }
    public List<LightNode> Children { get; private set; }
    public Dictionary<string, string> Attributes { get; private set; }

    private Dictionary<string, List<EventHandler>> EventListeners { get; set; }

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
        EventListeners = new Dictionary<string, List<EventHandler>>();
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

    public LightElementNode AddEventListener(string eventType, EventHandler handler)
    {
        if (string.IsNullOrWhiteSpace(eventType) || handler == null)
            return this;

        eventType = eventType.ToLower();

        if (!EventListeners.ContainsKey(eventType))
        {
            EventListeners[eventType] = new List<EventHandler>();
        }

        EventListeners[eventType].Add(handler);
        return this;
    }

    public LightElementNode RemoveEventListener(string eventType, EventHandler handler)
    {
        if (string.IsNullOrWhiteSpace(eventType) || handler == null)
            return this;

        eventType = eventType.ToLower();

        if (EventListeners.ContainsKey(eventType))
        {
            EventListeners[eventType].Remove(handler);
            if (EventListeners[eventType].Count == 0)
            {
                EventListeners.Remove(eventType);
            }
        }

        return this;
    }
    public void DispatchEvent(string eventType, Dictionary<string, object> eventData = null)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return;

        eventType = eventType.ToLower();

        if (EventListeners.ContainsKey(eventType))
        {
            var args = new LightEventArgs(eventType, this, eventData);

            var handlers = new List<EventHandler>(EventListeners[eventType]);

            foreach (var handler in handlers)
            {
                try
                {
                    handler(this, args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при виконанні event handler для події '{eventType}': {ex.Message}");
                }
            }
        }
    }

    public IEnumerable<string> GetEventTypes()
    {
        return EventListeners.Keys.ToList();
    }

    public bool HasEventListener(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return false;

        return EventListeners.ContainsKey(eventType.ToLower()) &&
               EventListeners[eventType.ToLower()].Count > 0;
    }

    public int GetEventListenerCount(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return 0;

        eventType = eventType.ToLower();
        return EventListeners.ContainsKey(eventType) ? EventListeners[eventType].Count : 0;
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

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Демонстрація роботи LightHTML з Event Listeners ===\n");

        var button = new LightElementNode("button", DisplayType.Inline)
            .AddClass("btn")
            .AddClass("btn-primary")
            .AddAttribute("type", "button")
            .AddText("Натисни мене!");

        button.AddEventListener("click", OnButtonClick);
        button.AddEventListener("click", OnButtonClickSecond);
        button.AddEventListener("mouseover", OnButtonMouseOver);
        button.AddEventListener("mouseout", OnButtonMouseOut);

        var input = new LightElementNode("input", DisplayType.Inline, TagClosingType.SelfClosing)
            .AddClass("form-control")
            .AddAttribute("type", "text")
            .AddAttribute("placeholder", "Введіть текст...");

        input.AddEventListener("focus", OnInputFocus);
        input.AddEventListener("blur", OnInputBlur);
        input.AddEventListener("change", OnInputChange);

        var container = new LightElementNode("div", DisplayType.Block)
            .AddClass("container")
            .AddAttribute("id", "main-container");

        container.AddEventListener("click", OnContainerClick);

        container.AddChild(new LightElementNode("h2", DisplayType.Block).AddText("Демонстрація Event Listeners"))
                 .AddChild(new LightElementNode("p", DisplayType.Block).AddText("Цей приклад показує роботу з подіями:"))
                 .AddChild(button)
                 .AddChild(new LightElementNode("br", DisplayType.Inline, TagClosingType.SelfClosing))
                 .AddChild(new LightElementNode("br", DisplayType.Inline, TagClosingType.SelfClosing))
                 .AddChild(input);

        Console.WriteLine("1. Інформація про event listeners:");
        Console.WriteLine($"   Кнопка має listeners для подій: [{string.Join(", ", button.GetEventTypes())}]");
        Console.WriteLine($"   Кількість click listeners на кнопці: {button.GetEventListenerCount("click")}");
        Console.WriteLine($"   Інпут має listeners для подій: [{string.Join(", ", input.GetEventTypes())}]");
        Console.WriteLine($"   Контейнер має click listener: {container.HasEventListener("click")}");

        Console.WriteLine("\n2. HTML структура:");
        Console.WriteLine(container.ToFormattedHtml());

        Console.WriteLine("\n3. Симуляція подій:");

        Console.WriteLine("\n--- Симуляція кліку по кнопці ---");
        button.DispatchEvent("click", new Dictionary<string, object>
        {
            { "x", 100 },
            { "y", 200 },
            { "timestamp", DateTime.Now }
        });

        Console.WriteLine("\n--- Симуляція наведення мишки на кнопку ---");
        button.DispatchEvent("mouseover");

        Console.WriteLine("\n--- Симуляція відведення мишки з кнопки ---");
        button.DispatchEvent("mouseout");

        Console.WriteLine("\n--- Симуляція фокусу на інпуті ---");
        input.DispatchEvent("focus");

        Console.WriteLine("\n--- Симуляція зміни значення інпуту ---");
        input.DispatchEvent("change", new Dictionary<string, object>
        {
            { "value", "Новий текст" },
            { "oldValue", "" }
        });

        Console.WriteLine("\n--- Симуляція втрати фокусу інпутом ---");
        input.DispatchEvent("blur");

        Console.WriteLine("\n--- Симуляція кліку по контейнеру ---");
        container.DispatchEvent("click");

        Console.WriteLine("\n4. Видалення event listener:");
        button.RemoveEventListener("click", OnButtonClickSecond);
        Console.WriteLine($"   Після видалення одного click listener: {button.GetEventListenerCount("click")}");

        Console.WriteLine("\n--- Повторна симуляція кліку по кнопці ---");
        button.DispatchEvent("click");

        Console.WriteLine("\n=== Кінець демонстрації ===");
    }

    static void OnButtonClick(object sender, LightEventArgs e)
    {
        Console.WriteLine($"[CLICK] Кнопка '{((LightElementNode)sender).GetInnerHtml()}' була натиснута!");
        if (e.Data.ContainsKey("x") && e.Data.ContainsKey("y"))
        {
            Console.WriteLine($"[CLICK] Координати: ({e.Data["x"]}, {e.Data["y"]})");
        }
        if (e.Data.ContainsKey("timestamp"))
        {
            Console.WriteLine($"[CLICK] Час події: {e.Data["timestamp"]}");
        }
    }

    static void OnButtonClickSecond(object sender, LightEventArgs e)
    {
        Console.WriteLine($"[CLICK-2] Другий обробник кліку для кнопки!");
    }

    static void OnButtonMouseOver(object sender, LightEventArgs e)
    {
        Console.WriteLine($"[MOUSEOVER] Мишка наведена на кнопку!");
    }

    static void OnButtonMouseOut(object sender, LightEventArgs e)
    {
        Console.WriteLine($"[MOUSEOUT] Мишка покинула кнопку!");
    }

    static void OnInputFocus(object sender, LightEventArgs e)
    {
        Console.WriteLine($"[FOCUS] Інпут отримав фокус!");
    }

    static void OnInputBlur(object sender, LightEventArgs e)
    {
        Console.WriteLine($"[BLUR] Інпут втратив фокус!");
    }

    static void OnInputChange(object sender, LightEventArgs e)
    {
        Console.WriteLine($"[CHANGE] Значення інпуту змінилось!");
        if (e.Data.ContainsKey("value"))
        {
            Console.WriteLine($"[CHANGE] Нове значення: '{e.Data["value"]}'");
        }
        if (e.Data.ContainsKey("oldValue"))
        {
            Console.WriteLine($"[CHANGE] Старе значення: '{e.Data["oldValue"]}'");
        }
    }

    static void OnContainerClick(object sender, LightEventArgs e)
    {
        var element = (LightElementNode)sender;
        var id = element.Attributes.ContainsKey("id") ? element.Attributes["id"] : "немає ID";
        Console.WriteLine($"[CONTAINER-CLICK] Клік по контейнеру з ID: {id}");
    }
}