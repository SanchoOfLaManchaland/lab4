using System;

public abstract class SupportHandler
{
    protected SupportHandler nextHandler;

    public void SetNext(SupportHandler handler)
    {
        nextHandler = handler;
    }

    public abstract bool Handle(SupportRequest request);
}

public class SupportRequest
{
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public string Issue { get; set; }
    public int Priority { get; set; }
}

public class TechnicalSupportHandler : SupportHandler
{
    public override bool Handle(SupportRequest request)
    {
        if (request.Category == "технічні" || request.SubCategory == "програмне забезпечення" ||
            request.SubCategory == "обладнання")
        {
            Console.WriteLine("            ТЕХНІЧНА ПІДТРИМКА (1-й рівень)                  ");
            Console.WriteLine($"Ваш запит: {request.Issue}");
            Console.WriteLine("Ви підключені до спеціаліста технічної підтримки.");
            Console.WriteLine("Ми допоможемо вирішити технічні проблеми з програмним забезпеченням та обладнанням.");
            return true;
        }

        if (nextHandler != null)
            return nextHandler.Handle(request);

        return false;
    }
}
public class BillingSupportHandler : SupportHandler
{
    public override bool Handle(SupportRequest request)
    {
        if (request.Category == "фінансові" || request.SubCategory == "рахунки" ||
            request.SubCategory == "платежі")
        {
            Console.WriteLine("            ФІНАНСОВА ПІДТРИМКА (2-й рівень)                  ");
            Console.WriteLine($"Ваш запит: {request.Issue}");
            Console.WriteLine("Ви підключені до спеціаліста з фінансових питань.");
            Console.WriteLine("Ми допоможемо з питаннями рахунків, платежів та фінансових операцій.");
            return true;
        }

        if (nextHandler != null)
            return nextHandler.Handle(request);

        return false;
    }
}

public class GeneralSupportHandler : SupportHandler
{
    public override bool Handle(SupportRequest request)
    {
        if (request.Category == "загальні" || request.SubCategory == "інформація" ||
            request.SubCategory == "консультації")
        {
            Console.WriteLine("            ЗАГАЛЬНА ПІДТРИМКА (3-й рівень)                   ");
            Console.WriteLine($"Ваш запит: {request.Issue}");
            Console.WriteLine("Ви підключені до спеціаліста загальної підтримки.");
            Console.WriteLine("Ми надамо загальну інформацію та консультації.");
            return true;
        }

        if (nextHandler != null)
            return nextHandler.Handle(request);

        return false;
    }
}

public class VipSupportHandler : SupportHandler
{
    public override bool Handle(SupportRequest request)
    {
        if (request.Priority >= 5 || request.Category == "vip")
        {
            Console.WriteLine("              VIP ПІДТРИМКА (4-й рівень)                      ");
            Console.WriteLine($"Ваш запит: {request.Issue}");
            Console.WriteLine("Ви підключені до старшого спеціаліста VIP підтримки.");
            Console.WriteLine("Ми забезпечуємо пріоритетне обслуговування для важливих питань.");
            return true;
        }

        if (nextHandler != null)
            return nextHandler.Handle(request);

        return false;
    }
}

// Головний клас системи підтримки
public class SupportSystem
{
    private SupportHandler chain;

    public SupportSystem()
    {
        // Створюємо ланцюжок обробників
        var technical = new TechnicalSupportHandler();
        var billing = new BillingSupportHandler();
        var general = new GeneralSupportHandler();
        var vip = new VipSupportHandler();

        // Встановлюємо ланцюжок
        technical.SetNext(billing);
        billing.SetNext(general);
        general.SetNext(vip);

        chain = technical;
    }

    public void StartSupport()
    {
        bool requestHandled = false;

        while (!requestHandled)
        {
            Console.Clear();
            Console.WriteLine("                 СИСТЕМА ПІДТРИМКИ КОРИСТУВАЧІВ               ");
            Console.WriteLine();

            var request = new SupportRequest();

            Console.WriteLine("1. Оберіть категорію вашої проблеми:");
            Console.WriteLine("   1 - Технічні питання");
            Console.WriteLine("   2 - Фінансові питання");
            Console.WriteLine("   3 - Загальні питання");
            Console.WriteLine("   4 - VIP підтримка");
            Console.WriteLine("   0 - Вихід");
            Console.Write("\nВаш вибір: ");

            string choice = Console.ReadLine();

            if (choice == "0")
            {
                Console.WriteLine("Дякуємо за звернення!");
                return;
            }

            switch (choice)
            {
                case "1":
                    request.Category = "технічні";
                    request = GetTechnicalDetails(request);
                    break;
                case "2":
                    request.Category = "фінансові";
                    request = GetBillingDetails(request);
                    break;
                case "3":
                    request.Category = "загальні";
                    request = GetGeneralDetails(request);
                    break;
                case "4":
                    request.Category = "vip";
                    request.Priority = 5;
                    request = GetVipDetails(request);
                    break;
                default:
                    Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                    Console.WriteLine("Натисніть будь-яку клавішу для продовження...");
                    Console.ReadKey();
                    continue;
            }

            // Пробуємо обробити запит
            Console.WriteLine("\nОбробка вашого запиту...");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");

            requestHandled = chain.Handle(request);

            if (!requestHandled)
            {
                Console.WriteLine("\nНа жаль, ми не змогли знайти відповідний рівень підтримки.");
                Console.WriteLine("Меню буде перезапущено для повторного вибору.");
                Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("\nВаш запит успішно переданий відповідному спеціалісту!");
                Console.WriteLine("Дякуємо за звернення!");
                Console.WriteLine("\nНатисніть будь-яку клавішу для завершення...");
                Console.ReadKey();
            }
        }
    }

    private SupportRequest GetTechnicalDetails(SupportRequest request)
    {
        while (true)
        {
            Console.WriteLine("\n1. Категорія: Технічні питання");
            Console.WriteLine("\n2. Оберіть тип технічної проблеми:");
            Console.WriteLine("   1 - Програмне забезпечення");
            Console.WriteLine("   2 - Обладнання");
            Console.WriteLine("   3 - Мережеві проблеми");
            Console.Write("\nВаш вибір: ");

            string subChoice = Console.ReadLine();
            switch (subChoice)
            {
                case "1":
                    request.SubCategory = "програмне забезпечення";
                    request.Issue = "Проблеми з програмним забезпеченням";
                    return GetPriority(request);
                case "2":
                    request.SubCategory = "обладнання";
                    request.Issue = "Проблеми з обладнанням";
                    return GetPriority(request);
                case "3":
                    request.SubCategory = "мережа";
                    request.Issue = "Мережеві проблеми";
                    return GetPriority(request);
                default:
                    Console.WriteLine("Невірний вибір. Введіть число від 1 до 3.");
                    Console.WriteLine("Натисніть будь-яку клавішу для повторення...");
                    Console.ReadKey();
                    Console.Clear();
                    Console.WriteLine("                 СИСТЕМА ПІДТРИМКИ КОРИСТУВАЧІВ               ");
                    break;
            }
        }
    }

    private SupportRequest GetBillingDetails(SupportRequest request)
    {
        while (true)
        {
            Console.WriteLine("\n1. Категорія: Фінансові питання");
            Console.WriteLine("\n2. Оберіть тип фінансового питання:");
            Console.WriteLine("   1 - Рахунки та виставлення");
            Console.WriteLine("   2 - Платежі");
            Console.WriteLine("   3 - Повернення коштів");
            Console.Write("\nВаш вибір: ");

            string subChoice = Console.ReadLine();
            switch (subChoice)
            {
                case "1":
                    request.SubCategory = "рахунки";
                    request.Issue = "Питання щодо рахунків";
                    return GetPriority(request);
                case "2":
                    request.SubCategory = "платежі";
                    request.Issue = "Проблеми з платежами";
                    return GetPriority(request);
                case "3":
                    request.SubCategory = "повернення";
                    request.Issue = "Повернення коштів";
                    return GetPriority(request);
                default:
                    Console.WriteLine("Невірний вибір. Введіть число від 1 до 3.");
                    Console.WriteLine("Натисніть будь-яку клавішу для повторення...");
                    Console.ReadKey();
                    Console.Clear();
                    Console.WriteLine("                 СИСТЕМА ПІДТРИМКИ КОРИСТУВАЧІВ               ");
                    break;
            }
        }
    }

    private SupportRequest GetGeneralDetails(SupportRequest request)
    {
        while (true)
        {
            Console.WriteLine("\n1. Категорія: Загальні питання");
            Console.WriteLine("\n2. Оберіть тип загального питання:");
            Console.WriteLine("   1 - Інформація про послуги");
            Console.WriteLine("   2 - Консультації");
            Console.WriteLine("   3 - Скарги та пропозиції");
            Console.Write("\nВаш вибір: ");

            string subChoice = Console.ReadLine();
            switch (subChoice)
            {
                case "1":
                    request.SubCategory = "інформація";
                    request.Issue = "Інформація про послуги";
                    return GetPriority(request);
                case "2":
                    request.SubCategory = "консультації";
                    request.Issue = "Потрібна консультація";
                    return GetPriority(request);
                case "3":
                    request.SubCategory = "скарги";
                    request.Issue = "Скарги та пропозиції";
                    return GetPriority(request);
                default:
                    Console.WriteLine("Невірний вибір. Введіть число від 1 до 3.");
                    Console.WriteLine("Натисніть будь-яку клавішу для повторення...");
                    Console.ReadKey();
                    Console.Clear();
                    Console.WriteLine("                 СИСТЕМА ПІДТРИМКИ КОРИСТУВАЧІВ               ");
                    break;
            }
        }
    }

    private SupportRequest GetVipDetails(SupportRequest request)
    {
        Console.WriteLine("\n2. Опишіть ваше питання:");
        Console.WriteLine("   (як VIP клієнт, ви маєте пріоритетне обслуговування)");
        Console.Write("\nВаше питання: ");

        string issue = Console.ReadLine();
        request.Issue = !string.IsNullOrEmpty(issue) ? issue : "VIP запит";
        request.Priority = 5; 

        return request;
    }

    private SupportRequest GetPriority(SupportRequest request)
    {
        while (true)
        {
            Console.WriteLine($"\n2. Підкатегорія: {request.Issue}");
            Console.WriteLine("\n3. Оцініть пріоритет вашого питання (1-5, де 5 - найвищий):");
            Console.Write("Пріоритет: ");

            string priorityInput = Console.ReadLine();

            if (int.TryParse(priorityInput, out int priority) && priority >= 1 && priority <= 5)
            {
                request.Priority = priority;
                return request;
            }
            else
            {
                Console.WriteLine("Невірний пріоритет. Введіть число від 1 до 5.");
                Console.WriteLine("Натисніть будь-яку клавішу для повторення...");
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine("                 СИСТЕМА ПІДТРИМКИ КОРИСТУВАЧІВ               ");
            }
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var supportSystem = new SupportSystem();
        supportSystem.StartSupport();
    }
}