# ---- 1. aşama: build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Önce sadece proje dosyaları -> restore adımı cache'lenebilsin
COPY Directory.Build.props Directory.Packages.props ./
COPY src/PortfolioPro.Domain/PortfolioPro.Domain.csproj src/PortfolioPro.Domain/
COPY src/PortfolioPro.Application/PortfolioPro.Application.csproj src/PortfolioPro.Application/
COPY src/PortfolioPro.Infrastructure/PortfolioPro.Infrastructure.csproj src/PortfolioPro.Infrastructure/
COPY src/PortfolioPro.API/PortfolioPro.API.csproj src/PortfolioPro.API/
RUN dotnet restore src/PortfolioPro.API/PortfolioPro.API.csproj

COPY src/ src/
RUN dotnet publish src/PortfolioPro.API/PortfolioPro.API.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---- 2. aşama: sadece runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Yüklenen dosyalar için klasör. Container root olmayan "app" kullanıcısıyla çalışıyor,
# bu yüzden klasörün sahibini değiştiriyoruz.
RUN mkdir -p /app/wwwroot/uploads && chown -R $APP_UID /app/wwwroot/uploads

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# NOT: .NET 8+ imajlarında hazır gelen root olmayan kullanıcı. Container ele geçirilse bile root yetkisi yok.
USER $APP_UID
ENTRYPOINT ["dotnet", "PortfolioPro.API.dll"]
