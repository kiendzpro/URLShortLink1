# Sử dụng .NET SDK cho quá trình build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy tất cả các file .csproj và restore dependencies
COPY ["src/UrlShortener.Core/UrlShortener.Core.csproj", "src/UrlShortener.Core/"]
COPY ["src/UrlShortener.Web/UrlShortener.Web.csproj", "src/UrlShortener.Web/"]
RUN dotnet restore "src/UrlShortener.Web/UrlShortener.Web.csproj"

# Copy toàn bộ source code
COPY ["src/UrlShortener.Core/", "src/UrlShortener.Core/"]
COPY ["src/UrlShortener.Web/", "src/UrlShortener.Web/"]

# Build ứng dụng
WORKDIR "/src/src/UrlShortener.Web"
RUN dotnet build "UrlShortener.Web.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "UrlShortener.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Tạo image cuối cùng
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check cho container
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/ || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV TZ=Asia/Ho_Chi_Minh

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "UrlShortener.Web.dll"]