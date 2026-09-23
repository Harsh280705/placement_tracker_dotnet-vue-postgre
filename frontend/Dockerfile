# Placement Tracker frontend — existing Vue 3 + Vite app, unchanged.
# Builds the static bundle, then serves it with `vite preview` on 0.0.0.0:5173.
# API calls use the relative path /api/..., so the browser talks to NGINX (:8080)
# which proxies /api/ to the backend. No app code changes needed.
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . ./
RUN npm run build

EXPOSE 5173
CMD ["npm", "run", "preview", "--", "--host", "0.0.0.0", "--port", "5173"]
