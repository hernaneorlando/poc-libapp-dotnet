using Application.Common.FluentValidation;
using Application.LoanManagement.BookCheckouts.DTOs;

namespace Application.LoanManagement.BookCheckouts.Queries;

public class GetCheckedOutBooksValidator : BasePagedQueryValidator<GetCheckedOutBooksQuery, BookCheckoutDto>;