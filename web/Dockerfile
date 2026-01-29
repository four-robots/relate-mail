# Build stage
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files and install
COPY package*.json ./
RUN npm ci

# Copy source and build
COPY . .
RUN npm run build

# Production stage with nginx
FROM nginx:alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy entrypoint script for runtime configuration
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# OCI Labels
LABEL org.opencontainers.image.title="Relate SMTP Web UI"
LABEL org.opencontainers.image.description="Web interface for Relate SMTP email management"
LABEL org.opencontainers.image.vendor="Relate SMTP"
LABEL org.opencontainers.image.source="https://github.com/tyevco/relate-smtp"

EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
