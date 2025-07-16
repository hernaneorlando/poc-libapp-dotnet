using Application.CatalogManagement.Books.DTOs;
using Application.Common.BaseDTO;
using Domain.CatalogManagement;
using Domain.CatalogManagement.Enums;

namespace Application.CatalogManagement.Contributors.DTOs;

public record BookContributorDto(BookDto Book, ContributorDto Contributor, ContributorRoleEnum Role) : AuditableDto
{
    public static implicit operator BookContributorDto(BookContributor bookContributor)
    {
        var bookContributorDto = new BookContributorDto((BookDto)bookContributor.Book, (ContributorDto)bookContributor.Contributor, bookContributor.Role);
        bookContributorDto.ConvertAuditableProperties(bookContributor);
        return bookContributorDto;
    }
}