using System.Diagnostics;
using System.Globalization;
using AutoMapper;
using CsvHelper.Configuration;
using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.Models;
using Libs.Api.Adapters.Resources;
using Microsoft.Extensions.Localization;

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

public class ExpenseMap : ClassMap<ExpenseFileResponse>
{
    public ExpenseMap(IStringLocalizer<SharedResources> localizer)
    {
        Map(m => m.TransactionDate)
            .Name(localizer[nameof(ExpenseFileResponse.TransactionDate)]);
        Map(m => m.Description)
            .Name(localizer[nameof(ExpenseFileResponse.Description)]);
        Map(m => m.Installment)
            .Name(localizer[nameof(ExpenseFileResponse.Installment)]);
        Map(m => m.Location)
            .Name(localizer[nameof(ExpenseFileResponse.Location)]);
        Map(m => m.PaymentMethod.Name)
            .Name(localizer[nameof(ExpenseFileResponse.PaymentMethod.Name)]);
        Map(m => m.PaymentMethod.PaymentType)
            .Name(localizer[nameof(ExpenseFileResponse.PaymentMethod.PaymentType)]);
        Map(m => m.TotalPrice)
            .Name(localizer[nameof(ExpenseFileResponse.TotalPrice)]);
        Map(m => m.ExpenseCategories)
            .Convert(row => string.Join(" | ", row.Value.ExpenseCategories ?? new List<string>()))
            .Name(localizer[nameof(ExpenseFileResponse.ExpenseCategories)]);
    }
}