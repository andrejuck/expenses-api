FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY ./code/publish .

RUN ls -l

EXPOSE 8080
ENTRYPOINT ["dotnet", "Expenses.Api.dll"]