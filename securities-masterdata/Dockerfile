FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY nuget.config nuget.config
COPY ["securities_masterdata/securities_masterdata.csproj", "securities_masterdata/securities_masterdata.csproj"]
COPY ["securities_masterdata.DataAccess/securities_masterdata.DataAccess.csproj", "securities_masterdata.DataAccess/securities_masterdata.DataAccess.csproj"]
COPY ["securities_masterdata.Domain/securities_masterdata.Domain.csproj", "securities_masterdata.Domain/securities_masterdata.Domain.csproj"]
COPY ["securities_masterdata.Shared/securities_masterdata.Shared.csproj", "securities_masterdata.Shared/securities_masterdata.Shared.csproj"]
RUN dotnet restore securities_masterdata/securities_masterdata.csproj
COPY . .

RUN dotnet publish securities_masterdata/securities_masterdata.csproj -c Release -o /app/publish --os linux

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "securities_masterdata.dll"]
