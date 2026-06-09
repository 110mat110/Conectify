#stage 1
FROM node:20.15.0 as node
WORKDIR /app
COPY Conectify.UI/. .
RUN npm install --loglevel=error
RUN npm install chart.js --loglevel=error
RUN npm run build --prod
#stage 2
FROM nginx:alpine
COPY --from=node /app/dist/Conectify.UI /usr/share/nginx/html