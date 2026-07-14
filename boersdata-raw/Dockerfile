FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["boersdata_raw/boersdata_raw/boersdata_raw.csproj", "boersdata_raw/boersdata_raw/boersdata_raw.csproj"]
COPY ["boersdata_raw/boersdata_raw.DataAccess/boersdata_raw.DataAccess.csproj", "boersdata_raw/boersdata_raw.DataAccess/boersdata_raw.DataAccess.csproj"]
COPY ["boersdata_raw/boersdata_raw.Domain/boersdata_raw.Domain.csproj", "boersdata_raw/boersdata_raw.Domain/boersdata_raw.Domain.csproj"]
COPY ["TTM.Shared/TTM.Shared/TTM.Shared.csproj", "TTM.Shared/TTM.Shared/TTM.Shared.csproj"]

#RUN dotnet restore boersdata_raw/boersdata_raw/boersdata_raw.csproj
COPY . .

RUN dotnet publish boersdata_raw/boersdata_raw/boersdata_raw.csproj -c Release -o /app/publish --os linux

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "boersdata_raw.dll"]
