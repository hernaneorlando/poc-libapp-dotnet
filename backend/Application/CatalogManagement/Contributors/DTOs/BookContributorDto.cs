using Application.CatalogManagement.Books.DTOs;
using Application.Common;
using Application.SeedWork.BaseDTO;
using Domain.CatalogManagement;
using Domain.CatalogManagement.Enums;

namespace Application.CatalogManagement.Contributors.DTOs;

public record BookContributorDto(BookDto Book, ContributorDto Contributor, ContributorRoleEnum Role) : AuditableDto
{
    public static implicit operator BookContributorDto(BookContributor bookContributor)
    {
        var bookContributorDto = new BookContributorDto((BookDto)bookContributor.Book, (ContributorDto)bookContributor.Contributor, bookContributor.Role);
        bookContributorDto.ConvertModelAuditableProperties(bookContributor);
        return bookContributorDto;
    }

    public static implicit operator BookContributor(BookContributorDto bookContributorDto)
    {
        var bookContributor = new BookContributor
        {
            Book = (Book)bookContributorDto.Book,
            Contributor = (Contributor)bookContributorDto.Contributor,
            Role = bookContributorDto.Role
        };

        bookContributor.ConvertDtoAuditableProperties(bookContributorDto);
        return bookContributor;
    }
}