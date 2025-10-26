# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./code .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

RUN ls -l

EXPOSE 8080
ENTRYPOINT ["dotnet", "Expenses.Api.dll"]