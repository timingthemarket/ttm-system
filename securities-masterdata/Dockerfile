FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["securities-masterdata/securities_masterdata/securities_masterdata.csproj", "securities-masterdata/securities_masterdata/securities_masterdata.csproj"]
COPY ["securities-masterdata/securities_masterdata.DataAccess/securities_masterdata.DataAccess.csproj", "securities-masterdata/securities_masterdata.DataAccess/securities_masterdata.DataAccess.csproj"]
COPY ["securities-masterdata/securities_masterdata.Domain/securities_masterdata.Domain.csproj", "securities-masterdata/securities_masterdata.Domain/securities_masterdata.Domain.csproj"]
COPY ["TTM.Shared/TTM.Shared/TTM.Shared.csproj", "TTM.Shared/TTM.Shared/TTM.Shared.csproj"]

#RUN dotnet restore securities_masterdata/securities_masterdata.csproj
COPY . .

RUN dotnet publish securities-masterdata/securities_masterdata/securities_masterdata.csproj -c Release -o /app/publish --os linux

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "securities_masterdata.dll"]
