#!/bin/bash

docker container kill horizon-server
sleep 1

set -e

####### Build the server
cd ..

docker build . -t horizon-server

docker run \
  -d \
  --rm \
  -e MIDDLEWARE_SERVER_IP=${HORIZON_MIDDLEWARE_SERVER_IP} \
  -e APP_ID=${HORIZON_APP_ID} \
  -e MIDDLEWARE_USER=${HORIZON_MIDDLEWARE_USER} \
  -e MIDDLEWARE_PASSWORD=${HORIZON_MIDDLEWARE_PASSWORD} \
  -e ASPNETCORE_ENVIRONMENT=${HORIZON_ASPNETCORE_ENVIRONMENT} \
  -e ROBO_SALT=${HORIZON_ROBO_SALT} \
  -p 10071:10071 \
  -p 10075:10075 \
  -p 10077:10077 \
  -p 10078:10078 \
  -p 10073:10073 \
  -p 8281:8281 \
  -p 8765:8765 \
  -p 50000-50100:50000-50100/udp \
  -p 10070:10070/udp \
  -v "${PWD}/logs":/logs \
  -v "${PWD}/database":/database \
  --name horizon-server \
  horizon-server
