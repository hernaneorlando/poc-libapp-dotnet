using Application.Common.MediatR;
using Application.LoanManagement.BookCheckouts.DTOs;

namespace Application.LoanManagement.BookCheckouts.Queries;

public record GetCheckedOutBooksQuery : BasePagedQuery<BookCheckoutDto>;