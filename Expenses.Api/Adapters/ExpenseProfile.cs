using AutoMapper;
using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.Models;

namespace Expenses.Api.Adapters;

public class ExpenseProfile : Profile
{
    public ExpenseProfile()
    {
        CreateMap<Expense, ExpenseResponse>();
        CreateMap<ExpenseForm, Expense>();
        CreateMap<Expense, ExpenseForm>();
    }
}