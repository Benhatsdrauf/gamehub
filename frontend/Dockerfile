# ---- build stage: produce the static SPA bundle ----
FROM node:22-alpine AS build
WORKDIR /app
RUN npm install -g pnpm@latest

# Install deps first (cached until the lockfile changes).
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# The SPA runs in the BROWSER, so VITE_API_URL must be the host-reachable API URL
# (http://localhost:5206/api) — not the internal docker "api" hostname. It's baked in
# at build time, hence a build ARG.
COPY . .
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL
RUN pnpm build

# ---- runtime stage: nginx serving the static files ----
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
