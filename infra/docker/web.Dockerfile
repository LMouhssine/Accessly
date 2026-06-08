# Accessly Web (Angular) — build then serve with nginx
FROM node:26 AS build
WORKDIR /app
COPY src/Accessly.Web/package*.json ./
RUN npm ci
COPY src/Accessly.Web/ ./
RUN npm run build

FROM nginx:alpine AS final
COPY infra/docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/accessly-web/browser /usr/share/nginx/html
EXPOSE 80
