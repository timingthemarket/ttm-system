FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY nuget.config nuget.config
COPY ["portfolio/portfolio/portfolio.csproj", "portfolio/portfolio/portfolio.csproj"]
COPY ["portfolio/portfolio.DataAccess/portfolio.DataAccess.csproj", "portfolio/portfolio.DataAccess/portfolio.DataAccess.csproj"]
COPY ["portfolio/portfolio.Domain/portfolio.Domain.csproj", "portfolio/portfolio.Domain/portfolio.Domain.csproj"]
COPY ["TTM.Shared/TTM.Shared/TTM.Shared.csproj", "TTM.Shared/TTM.Shared/TTM.Shared.csproj"]

RUN dotnet restore portfolio/portfolio/portfolio.csproj
COPY . .

RUN dotnet publish portfolio/portfolio/portfolio.csproj -c Release -o /app/publish --os linux /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "portfolio.dll"]
