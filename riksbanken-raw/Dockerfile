FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY nuget.config nuget.config
COPY ["riksbanken_raw/riksbanken_raw/riksbanken_raw.csproj", "riksbanken_raw/riksbanken_raw/riksbanken_raw.csproj"]
COPY ["riksbanken_raw/riksbanken_raw.DataAccess/riksbanken_raw.DataAccess.csproj", "riksbanken_raw/riksbanken_raw.DataAccess/riksbanken_raw.DataAccess.csproj"]
COPY ["riksbanken_raw/riksbanken_raw.Domain/riksbanken_raw.Domain.csproj", "riksbanken_raw/riksbanken_raw.Domain/riksbanken_raw.Domain.csproj"]
COPY ["riksbanken_raw/riksbanken_raw.Shared/riksbanken_raw.Shared.csproj", "riksbanken_raw/riksbanken_raw.Shared/riksbanken_raw.Shared.csproj"]
COPY ["TTM.Shared/TTM.Shared/TTM.Shared.csproj", "TTM.Shared/TTM.Shared/TTM.Shared.csproj"]

RUN dotnet restore riksbanken_raw/riksbanken_raw/riksbanken_raw.csproj
COPY . .

RUN dotnet publish riksbanken_raw/riksbanken_raw/riksbanken_raw.csproj -c Release -o /app/publish --os linux

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "riksbanken_raw.dll"]
