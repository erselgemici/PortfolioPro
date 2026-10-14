# ---- 1. aşama: Angular build ----
FROM node:22-alpine AS build
WORKDIR /app

# NOT: Önce sadece package dosyalarını kopyalıyoruz. Kod değişip paketler değişmediyse
# Docker "npm ci" adımını cache'ten kullanıyor, build çok daha hızlı oluyor.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---- 2. aşama: sadece derlenmiş dosyalar + nginx ----
# Son imajda node, node_modules ve kaynak kod YOK. İmaj küçük ve daha güvenli.
FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/client/browser /usr/share/nginx/html
EXPOSE 80
