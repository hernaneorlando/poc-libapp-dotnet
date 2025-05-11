using Application.Common;
using Application.SeedWork.BaseDTO;
using Domain.CatalogManagement;

namespace Application.CatalogManagement.Contributors.DTOs;

public record ContributorDto(string FirstName, string LastName) : BaseDto
{
    public DateOnly? DateOfBirth { get; set; }
    public IList<BookContributorDto> Books { get; set; } = [];

    public static implicit operator ContributorDto(Contributor contributor)
    {
        var contributorDto = new ContributorDto(contributor.FirstName, contributor.LastName)
        {
            DateOfBirth = contributor.DateOfBirth,
            Books = [.. contributor.Books.Select(b => (BookContributorDto)b)]
        };

        contributorDto.ConvertModelBaseProperties(contributor);
        return contributorDto;
    }

    public static implicit operator Contributor(ContributorDto contributorDto)
    {
        var contributor = new Contributor()
        {
            FirstName = contributorDto.FirstName,
            LastName = contributorDto.LastName,
            DateOfBirth = contributorDto.DateOfBirth,
            Books = [.. contributorDto.Books.Select(b => (BookContributor)b)]
        };

        contributor.ConvertDtoBaseProperties(contributorDto);
        return contributor;
    }
}