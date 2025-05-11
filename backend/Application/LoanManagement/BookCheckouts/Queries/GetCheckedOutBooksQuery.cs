using Application.LoanManagement.BookCheckouts.DTOs;
using Application.SeedWork.MediatR;

namespace Application.LoanManagement.BookCheckouts.Queries;

public record GetCheckedOutBooksQuery : BasePagedQuery<BookCheckoutDto>;