FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["riksbanken-raw/riksbanken_raw/riksbanken_raw.csproj", "riksbanken-raw/riksbanken_raw/riksbanken_raw.csproj"]
COPY ["riksbanken-raw/riksbanken_raw.DataAccess/riksbanken_raw.DataAccess.csproj", "riksbanken-raw/riksbanken_raw.DataAccess/riksbanken_raw.DataAccess.csproj"]
COPY ["riksbanken-raw/riksbanken_raw.Domain/riksbanken_raw.Domain.csproj", "riksbanken-raw/riksbanken_raw.Domain/riksbanken_raw.Domain.csproj"]
COPY ["TTM.Shared/TTM.Shared/TTM.Shared.csproj", "TTM.Shared/TTM.Shared/TTM.Shared.csproj"]

#RUN dotnet restore riksbanken-raw/riksbanken_raw/riksbanken_raw.csproj
COPY . .

RUN dotnet publish riksbanken-raw/riksbanken_raw/riksbanken_raw.csproj -c Release -o /app/publish --os linux

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "riksbanken_raw.dll"]
