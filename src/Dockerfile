FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
RUN dotnet restore "src/AppAuthentication.csproj"
COPY . .
WORKDIR "/src/src/AppAuthentication"
RUN dotnet build "AppAuthentication.csproj" -c Release -f -o /app/build

FROM build AS publish
RUN dotnet publish "AppAuthentication.csproj" -c Release -f -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppAuthentication.dll"]
CMD ["run", "--verbose:debug"]

