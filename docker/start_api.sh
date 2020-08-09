#!/bin/bash
mkdir ./rubicon-test/src && cp -a ../src/. ./rubicon-test/src/

echo "Creating and starting dev containers"
docker-compose -p dev -f docker-compose.yml up -d

rm -rf ./rubicon-test/src