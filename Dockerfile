FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app

COPY ./Expenses.Api.sln .
COPY ./Expenses.Api/Expenses.Api.csproj ./Expenses.Api/
COPY ./Expenses.Domain/Expenses.Domain.csproj ./Expenses.Domain/
COPY ./Expenses.Infra/Expenses.Infra.csproj ./Expenses.Infra/
RUN dotnet restore Expenses.Api/Expenses.Api.csproj

COPY . .
WORKDIR /app/Expenses.Api
RUN dotnet publish -c Release -o /app/publish

# Usando a imagem runtime do .NET para execu��o
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=base /app/publish .

# Definindo a porta e o comando de entrada
EXPOSE 8080
ENTRYPOINT ["dotnet", "Expenses.Api.dll"]