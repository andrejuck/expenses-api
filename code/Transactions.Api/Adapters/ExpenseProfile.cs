using AutoMapper;
using CsvHelper.Configuration;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Api.Resources;
using Microsoft.Extensions.Localization;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.Adapters;

public class ExpenseProfile : Profile
{
    public ExpenseProfile()
    {
        CreateMap<Transaction, TransactionResponse>();
        CreateMap<TransactionForm, Transaction>();
        CreateMap<Transaction, TransactionForm>();
    }
}

public class ExpenseMap : ClassMap<TransactionFileResponse>
{
    public ExpenseMap(IStringLocalizer<SharedResources> localizer)
    {
        Map(m => m.TransactionDate)
            .Name(localizer[nameof(TransactionFileResponse.TransactionDate)])
            .TypeConverterOption.Format("yyyy-MM-dd");
        Map(m => m.Description)
            .Name(localizer[nameof(TransactionFileResponse.Description)]);
        Map(m => m.Location)
            .Name(localizer[nameof(TransactionFileResponse.Location)]);
        Map(m => m.PaymentMethod.Name)
            .Name(localizer[$"PaymentMethod.{nameof(TransactionFileResponse.PaymentMethod.Name)}"]);
        Map(m => m.PaymentMethod.PaymentType)
            .Name(localizer[$"PaymentMethod.{nameof(TransactionFileResponse.PaymentMethod.PaymentType)}"]);
        Map(m => m.TotalPrice)
            .Name(localizer[nameof(TransactionFileResponse.TotalPrice)]);
        Map(m => m.ExpenseCategories)
            .Convert(row => string.Join(" | ", row.Value.ExpenseCategories ?? new List<string>()))
            .Name(localizer[nameof(TransactionFileResponse.ExpenseCategories)]);
    }
}