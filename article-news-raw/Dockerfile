FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY nuget.config nuget.config
COPY ["article_news_raw/article_news_raw.csproj", "article_news_raw/article_news_raw.csproj"]
COPY ["article_news_raw.DataAccess/article_news_raw.DataAccess.csproj", "article_news_raw.DataAccess/article_news_raw.DataAccess.csproj"]
COPY ["article_news_raw.Domain/article_news_raw.Domain.csproj", "article_news_raw.Domain/article_news_raw.Domain.csproj"]

RUN dotnet restore article_news_raw/article_news_raw.csproj
COPY . .

RUN dotnet publish article_news_raw/article_news_raw.csproj -c Release -o /app/publish --os linux

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "article_news_raw.dll"]
