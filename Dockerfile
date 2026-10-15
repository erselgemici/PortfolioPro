# Tüm site tek imajda: Angular derlenip .NET API'nin wwwroot klasörüne konuyor, API hem sayfaları hem /api'yi sunuyor.
# NOT: Ücretsiz hosting'ler (Render) tek container çalıştırıyor, bu yüzden nginx'i ayrı container olarak kullanmıyorum.

# ---- 1. aşama: Angular build ----
FROM node:22-alpine AS client
WORKDIR /client
# Önce sadece package dosyaları -> kod değişip paketler değişmediyse "npm ci" cache'ten geliyor
COPY client/package.json client/package-lock.json ./
RUN npm ci
COPY client/ ./
RUN npm run build

# ---- 2. aşama: .NET build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS server
WORKDIR /src
COPY server/Directory.Build.props server/Directory.Packages.props ./
COPY server/src/PortfolioPro.Domain/PortfolioPro.Domain.csproj src/PortfolioPro.Domain/
COPY server/src/PortfolioPro.Application/PortfolioPro.Application.csproj src/PortfolioPro.Application/
COPY server/src/PortfolioPro.Infrastructure/PortfolioPro.Infrastructure.csproj src/PortfolioPro.Infrastructure/
COPY server/src/PortfolioPro.API/PortfolioPro.API.csproj src/PortfolioPro.API/
RUN dotnet restore src/PortfolioPro.API/PortfolioPro.API.csproj
COPY server/src/ src/
RUN dotnet publish src/PortfolioPro.API/PortfolioPro.API.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---- 3. aşama: sadece runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=server /app/publish .
COPY --from=client /client/dist/client/browser ./wwwroot

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# NOT: .NET 8+ imajlarında hazır gelen root olmayan kullanıcı. Container ele geçirilse bile root yetkisi yok.
USER $APP_UID
ENTRYPOINT ["dotnet", "PortfolioPro.API.dll"]
