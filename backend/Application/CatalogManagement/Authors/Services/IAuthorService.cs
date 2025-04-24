using Domain.CatalogManagement;

namespace Application.CatalogManagement.Authors.Services;

public interface IAuthorService
{
    Task<IList<Author>> GetAll();
}