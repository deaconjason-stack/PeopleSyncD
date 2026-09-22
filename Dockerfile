FROM node:22-alpine
RUN apk add --no-cache unzip
WORKDIR /opt/caresyncd
COPY releases/CareSyncD-v3-HOSPITAL-SHIFT.zip /tmp/caresyncd-v3.zip
RUN unzip -q /tmp/caresyncd-v3.zip -d /opt/caresyncd  && rm /tmp/caresyncd-v3.zip  && cd /opt/caresyncd/CareSyncD  && npm test  && npm run check
WORKDIR /opt/caresyncd/CareSyncD
ENV NODE_ENV=production
ENV HOST=0.0.0.0
ENV PORT=3000
EXPOSE 3000
CMD ["npm","start"]
