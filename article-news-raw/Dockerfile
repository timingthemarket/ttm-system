FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["article-news-raw/article_news_raw/article_news_raw.csproj", "article-news-raw/article_news_raw/article_news_raw.csproj"]
COPY ["article-news-raw/article_news_raw.DataAccess/article_news_raw.DataAccess.csproj", "article-news-raw/article_news_raw.DataAccess/article_news_raw.DataAccess.csproj"]
COPY ["article-news-raw/article_news_raw.Domain/article_news_raw.Domain.csproj", "article-news-raw/article_news_raw.Domain/article_news_raw.Domain.csproj"]
COPY ["TTM.Shared/TTM.Shared/TTM.Shared.csproj", "TTM.Shared/TTM.Shared/TTM.Shared.csproj"]

COPY . .

RUN dotnet publish article-news-raw/article_news_raw/article_news_raw.csproj -c Release -o /app/publish --os linux

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "article_news_raw.dll"]
