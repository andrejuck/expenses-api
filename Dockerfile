# syntax=docker/dockerfile:1.6

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN --mount=type=secret,id=github_token \
    dotnet nuget add source \
    https://nuget.pkg.github.com/andrejuck/index.json \
    --name github \
    --username x-access-token \
    --password "$(cat /run/secrets/github_token)" \
    --store-password-in-clear-text


COPY ./code .
RUN dotnet restore Expenses.Api.sln

WORKDIR /src/code/Expenses.Api
RUN dotnet publish -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Expenses.Api.dll"]