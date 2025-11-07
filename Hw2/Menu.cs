using System.Globalization;
using Domain.Entities;
using Domain.Facades;
using Domain.ImportExport;
using Domain.Repositories;

namespace Hw2
{
    public class Menu
    {
        private readonly AccountFacade _accounts;
        private readonly CategoryFacade _categories;
        private readonly OperationFacade _operations;
        private readonly AnalyticFacade _analytics;
        private readonly IRepository<BankAccount> _accountStorage;
        private readonly IRepository<Category> _categoryStorage;
        private readonly IRepository<Operation> _operationStorage;

        public Menu(AccountFacade accountService,
                    CategoryFacade categoryService,
                    OperationFacade operationService,
                    AnalyticFacade analyticsService,
                    IRepository<BankAccount> accountRepo,
                    IRepository<Category> categoryRepo,
                    IRepository<Operation> operationRepo)
        {
            _accounts = accountService;
            _categories = categoryService;
            _operations = operationService;
            _analytics = analyticsService;
            _accountStorage = accountRepo;
            _categoryStorage = categoryRepo;
            _operationStorage = operationRepo;
        }

        public void Start()
        {
            while (true)
            {
                DisplayMenu();
                var selection = Console.ReadLine();
                Console.WriteLine();

                ProcessUserSelection(selection);
            }
        }

        private void DisplayMenu()
        {
            Console.WriteLine("1. Создать счет");
            Console.WriteLine("2. Показать счета");
            Console.WriteLine("3. Создать категорию");
            Console.WriteLine("4. Показать категории");
            Console.WriteLine("5. Создать операцию");
            Console.WriteLine("6. Показать операции");
            Console.WriteLine("7. Аналитика");
            Console.WriteLine("8. Импорт данных");
            Console.WriteLine("9. Экспорт данных");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите действие: ");
        }

        private void ProcessUserSelection(string? selection)
        {
            switch (selection)
            {
                case "1":
                    CreateNewAccount();
                    break;
                case "2":
                    DisplayAccounts();
                    break;
                case "3":
                    CreateNewCategory();
                    break;
                case "4":
                    DisplayCategories();
                    break;
                case "5":
                    CreateNewOperation();
                    break;
                case "6":
                    DisplayOperations();
                    break;
                case "7":
                    ShowAnalyticsData();
                    break;
                case "8":
                    PerformImport();
                    break;
                case "9":
                    PerformExport();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Неизвестная команда");
                    break;
            }
        }

        private void CreateNewAccount()
        {
            Console.Write("Название счета: ");
            var name = Console.ReadLine();

            decimal balance;
            while (true)
            {
                Console.Write("Начальный баланс: ");
                var input = Console.ReadLine();
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out balance))
                    break;
                Console.WriteLine("Неверное число. Введите корректное десятичное число.");
            }

            var account = _accounts.Create(name!, balance);
            Console.WriteLine($"Счет '{account.Name}' создан (ID: {account.Id}) с балансом {account.Balance}.");
        }

        private void DisplayAccounts()
        {
            var accountList = _accounts.GetAllAccounts().ToList();
            if (!accountList.Any())
            {
                Console.WriteLine("Счета не найдены.");
                return;
            }
            foreach (var account in accountList)
                Console.WriteLine($"{account.Id} : {account.Name} = {account.Balance:C}");
        }

        private void CreateNewCategory()
        {
            Console.Write("Название категории: ");
            var name = Console.ReadLine();

            TypeOfOperation type;
            while (true)
            {
                Console.Write("Тип (Income/Expense): ");
                var input = Console.ReadLine();
                if (Enum.TryParse<TypeOfOperation>(input, true, out type))
                    break;
                Console.WriteLine("Неверный тип. Введите 'Income' или 'Expense'.");
            }

            var category = _categories.Create(name!, type);
            Console.WriteLine($"Категория '{category.Name}' ({GetTypeName(category.Type)}) создана (ID: {category.Id}).");
        }

        private void DisplayCategories()
        {
            var categoryList = _categories.GetAllCategories().ToList();
            if (!categoryList.Any())
            {
                Console.WriteLine("Категории не найдены.");
                return;
            }
            foreach (var category in categoryList)
                Console.WriteLine($"{category.Id} : {category.Name} ({GetTypeName(category.Type)})");
        }

        private void CreateNewOperation()
        {
            Guid accountId = GetGuidInput("ID счета: ", "Неверный GUID. Введите корректный ID счета.");
            Guid categoryId = GetGuidInput("ID категории: ", "Неверный GUID. Введите корректный ID категории.");

            TypeOfOperation type;
            while (true)
            {
                Console.Write("Тип операции (Income/Expense): ");
                var input = Console.ReadLine();
                if (Enum.TryParse<TypeOfOperation>(input, true, out type))
                    break;
                Console.WriteLine("Неверный тип. Введите 'Income' или 'Expense'.");
            }

            decimal amount = GetDecimalInput("Сумма: ", "Неверное число. Введите корректную сумму.");
            DateTime date = GetDateInput("Дата (гггг-мм-дд): ", "Неверная дата. Используйте формат гггг-мм-дд.");

            Console.Write("Описание (необязательно): ");
            var description = Console.ReadLine() ?? "";

            var operation = _operations.RegisterNewOperation(type, accountId, amount, date, categoryId, description);
            Console.WriteLine($"Операция '{operation.Id}' записана: {GetTypeName(operation.Type)} {operation.Amount:C} на {operation.Date:yyyy-MM-dd}.");
        }

        private void DisplayOperations()
        {
            var operationList = _operations.GetAllOperations().ToList();
            if (!operationList.Any())
            {
                Console.WriteLine("Операции не найдены.");
                return;
            }
            foreach (var operation in operationList)
                Console.WriteLine($"{operation.Id} : {operation.Date:yyyy-MM-dd} {GetTypeName(operation.Type)} {operation.Amount:C} ({operation.Description})");
        }

        private void ShowAnalyticsData()
        {
            DateTime startDate = GetDateInput("Начальная дата (гггг-мм-дд): ", "Неверная дата. Используйте формат гггг-мм-дд.");
            DateTime endDate = GetDateInput("Конечная дата (гггг-мм-дд): ", "Неверная дата. Используйте формат гггг-мм-дд.");

            var netResult = _analytics.CalculateNetBalance(startDate, endDate);
            Console.WriteLine($"Чистый доход с {startDate:yyyy-MM-dd} по {endDate:yyyy-MM-dd}: {netResult:C}");

            var categoryData = _analytics.GetOperationsByCategory(startDate, endDate);
            if (!categoryData.Any())
            {
                Console.WriteLine("В указанный период операции отсутствуют.");
                return;
            }
            Console.WriteLine("Суммы по категориям:");
            foreach (var category in categoryData)
                Console.WriteLine($"{category.Key} : {category.Value:C}");
        }

        private void PerformImport()
        {
            Console.Write("Путь к файлу (csv/json/yaml): ");
            var filePath = Console.ReadLine()!;

            DataImporter importer = filePath.ToLowerInvariant() switch
            {
                string p when p.EndsWith(".json") => new JsonImporter(_accountStorage, _categoryStorage, _operationStorage),
                string p when p.EndsWith(".yaml") || p.EndsWith(".yml") => new YamlImporter(_accountStorage, _categoryStorage, _operationStorage),
                string p when p.EndsWith(".csv") => new CsvImporter(_accountStorage, _categoryStorage, _operationStorage),
                _ => null!
            };

            if (importer == null)
            {
                Console.WriteLine("Неподдерживаемый формат файла.");
                return;
            }

            try
            {
                importer.Import(filePath);
                Console.WriteLine("Импорт успешно завершен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при импорте: {ex.Message}");
            }
        }

        private void PerformExport()
        {
            Console.Write("Формат экспорта (csv/json/yaml): ");
            var formatInput = Console.ReadLine()!.Trim().ToLowerInvariant();

            if (formatInput != "csv" && formatInput != "json" && formatInput != "yaml" && formatInput != "yml")
            {
                Console.WriteLine("Неподдерживаемый формат. Используйте csv, json или yaml.");
                return;
            }

            var exportFormat = formatInput switch
            {
                "csv" => ExportFormat.Csv,
                "json" => ExportFormat.Json,
                _ => ExportFormat.Yaml
            };

            Console.Write("Директория для экспорта: ");
            var directoryPath = Console.ReadLine()!;

            try
            {
                string targetPath;
                if (exportFormat == ExportFormat.Csv)
                {
                    targetPath = directoryPath;
                }
                else
                {
                    var fileName = exportFormat == ExportFormat.Json ? "data.json" : "data.yaml";
                    targetPath = Path.Combine(directoryPath, fileName);
                }

                using var exporter = new DataExport(exportFormat, targetPath);

                foreach (var account in _accountStorage.GetAllItems())
                    account.Accept(exporter);
                foreach (var category in _categoryStorage.GetAllItems())
                    category.Accept(exporter);
                foreach (var operation in _operationStorage.GetAllItems())
                    operation.Accept(exporter);

                Console.WriteLine(exportFormat == ExportFormat.Csv
                    ? $"CSV экспорт завершен в директорию: {targetPath}"
                    : $"{exportFormat} экспорт завершен в файл: {targetPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка экспорта: {ex.Message}");
            }
        }

        private Guid GetGuidInput(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (Guid.TryParse(input, out Guid result))
                    return result;
                Console.WriteLine(errorMessage);
            }
        }

        private decimal GetDecimalInput(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
                    return result;
                Console.WriteLine(errorMessage);
            }
        }

        private DateTime GetDateInput(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;
                Console.WriteLine(errorMessage);
            }
        }

        private string GetTypeName(TypeOfOperation type)
        {
            return type == TypeOfOperation.Income ? "Доход" : "Расход";
        }
    }
}