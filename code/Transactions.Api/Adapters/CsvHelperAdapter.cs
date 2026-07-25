using CsvHelper;
using CsvHelper.Configuration;
using Transactions.Api.Resources;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text;

namespace Transactions.Api.Adapters;

public class CsvHelperAdapter
{

    private readonly IServiceProvider _serviceProvider;
    public CsvHelperAdapter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Stream GenerateFile<T>(Stream stream, List<T> content, CultureInfo culture = null)
    {
        using var writer = new StreamWriter(stream, new UTF8Encoding(true), leaveOpen: true);
        using (var csv = new CsvWriter(writer, culture ?? CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(content);

            writer.Flush();
            stream.Position = 0;
        }

        return stream;
    }

    public Stream GenerateFile<T, TMap>(Stream stream, List<T> content, CultureInfo culture = null)
        where TMap : ClassMap<T>
    {
        using var writer = new StreamWriter(stream, new UTF8Encoding(true), leaveOpen: true);
        using (var csv = new CsvWriter(writer, culture ?? CultureInfo.InvariantCulture))
        {
            csv.Context.AutoMap<TMap>();
            var classMap = CreateClassMapInstance<TMap>();
            csv.Context.RegisterClassMap(classMap);
            csv.WriteRecords(content);

            writer.Flush();
            stream.Position = 0;
        }

        return stream;
    }

    public (List<string> Headers, List<T> Records) ReadCsv<T, TMap>(string csvContent, CultureInfo culture = null)
        where TMap : ClassMap<T>
    {
        var csvConfig = new CsvConfiguration(culture ?? CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        using var reader = new StringReader(csvContent);
        using (var csv = new CsvReader(reader, csvConfig))
        {
            csv.Context.AutoMap<T>();
            var classMap = CreateClassMapInstance<TMap>();
            csv.Context.RegisterClassMap(classMap);
            var records = csv.GetRecords<T>().ToList();
            var headers = csv.HeaderRecord.ToList();

            return (headers, records);
        };

    }

    private TMap CreateClassMapInstance<TMap>() where TMap : ClassMap
    {
        var constructor = typeof(TMap).GetConstructor(new[] { typeof(IStringLocalizer<SharedResources>) });

        if (constructor != null)
        {
            var localizer = (IStringLocalizer<SharedResources>)_serviceProvider.GetService(typeof(IStringLocalizer<SharedResources>));
            return (TMap)constructor.Invoke(new object[] { localizer });
        }

        return Activator.CreateInstance<TMap>();
    }
}