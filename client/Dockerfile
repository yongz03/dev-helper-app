FROM node:22-alpine AS build
WORKDIR /src
COPY client/package*.json ./
RUN npm ci
COPY client/ ./
RUN npm run build -- --configuration production

FROM nginx:1.27-alpine
COPY client/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/dist/client/browser /usr/share/nginx/html
EXPOSE 8080
