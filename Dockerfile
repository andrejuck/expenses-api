# Usando a imagem runtime do .NET para execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN ls -l

COPY ./publish .

# Definindo a porta e o comando de entrada
EXPOSE 8080
ENTRYPOINT ["dotnet", "Expenses.Api.dll"]