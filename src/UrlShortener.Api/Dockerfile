# Sử dụng .NET SDK cho quá trình build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy tất cả các file .csproj và restore dependencies
COPY ["src/UrlShortener.Core/UrlShortener.Core.csproj", "src/UrlShortener.Core/"]
COPY ["src/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj", "src/UrlShortener.Infrastructure/"]
COPY ["src/UrlShortener.Api/UrlShortener.Api.csproj", "src/UrlShortener.Api/"]
RUN dotnet restore "src/UrlShortener.Api/UrlShortener.Api.csproj"

# Copy toàn bộ source code
COPY ["src/UrlShortener.Core/", "src/UrlShortener.Core/"]
COPY ["src/UrlShortener.Infrastructure/", "src/UrlShortener.Infrastructure/"]
COPY ["src/UrlShortener.Api/", "src/UrlShortener.Api/"]

# Build ứng dụng
WORKDIR "/src/src/UrlShortener.Api"
RUN dotnet build "UrlShortener.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "UrlShortener.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Tạo image cuối cùng
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cài đặt curl và bash
RUN apt-get update && apt-get install -y curl bash

# Tạo script chờ SQL Server
RUN echo '#!/bin/bash\n\
echo "Waiting for SQL Server to start..."\n\
until (curl -s sqlserver:1433 || true) 2>&1 > /dev/null; do\n\
  echo "SQL Server is unavailable - sleeping"\n\
  sleep 2\n\
done\n\
echo "SQL Server is up - executing command"\n\
exec "$@"' > /app/wait-for-sqlserver.sh \
&& chmod +x /app/wait-for-sqlserver.sh

# Health check cho container
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/api/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV TZ=Asia/Ho_Chi_Minh

# Chạy ứng dụng với script chờ
ENTRYPOINT ["/app/wait-for-sqlserver.sh", "dotnet", "UrlShortener.Api.dll"]