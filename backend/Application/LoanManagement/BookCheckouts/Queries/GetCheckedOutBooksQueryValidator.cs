using Application.LoanManagement.BookCheckouts.DTOs;
using Application.SeedWork.FluentValidation;

namespace Application.LoanManagement.BookCheckouts.Queries;

public class GetCheckedOutBooksQueryValidator : BasePagedQueryValidator<GetCheckedOutBooksQuery, BookCheckoutDto>;