using Domain.Entities;
using Domain.Facades;

namespace Domain.Commands;

public class CreateCategoryCommand : ICommand
{
    private readonly CategoryFacade _categoryService;
    private readonly string _categoryName;
    private readonly TypeOfOperation _categoryType;

    public CreateCategoryCommand(CategoryFacade service, string name, TypeOfOperation type)
    {
        _categoryService = service;
        _categoryName = name;
        _categoryType = type;
    }

    public void Execute()
    {
        var createdCategory = _categoryService.Create(_categoryName, _categoryType);
        Console.WriteLine($"Категория '{createdCategory.Name}' ({GetTypeDescription(createdCategory.Type)}) создана");
    }

    private string GetTypeDescription(TypeOfOperation type)
    {
        return type == TypeOfOperation.Income ? "доход" : "расход";
    }
}