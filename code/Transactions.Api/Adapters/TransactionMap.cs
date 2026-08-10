using CsvHelper.Configuration;
using Microsoft.Extensions.Localization;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Api.Resources;

namespace Transactions.Api.Adapters;

public sealed class TransactionMap : ClassMap<TransactionFileResponse>
{
    public TransactionMap(IStringLocalizer<SharedResources> localizer)
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