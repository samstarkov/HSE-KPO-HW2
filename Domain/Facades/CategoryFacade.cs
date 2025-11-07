using Domain.Repositories;
using Domain.Entities;

namespace Domain.Facades;

public class CategoryFacade
{
    private readonly IRepository<Category> _categoriesStorage;

    public CategoryFacade(IRepository<Category> storage)
        => _categoriesStorage = storage;

    public Category Create(string categoryName, TypeOfOperation operationCategory)
    {
        var categoryData = BuildCategory(categoryName, operationCategory);
        _categoriesStorage.Add(categoryData);
        return categoryData;
    }

    public IEnumerable<Category> GetAllCategories()
        => _categoriesStorage.GetAllItems();

    public void RemoveCategoryById(Guid categoryIdentifier)
        => _categoriesStorage.Remove(categoryIdentifier);

    private Category BuildCategory(string name, TypeOfOperation type)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type
        };
    }
}