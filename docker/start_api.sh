#!/bin/bash
mkdir ./rubicon-test/src && cp -a ../src/RubiconBlogProject/. ./rubicon-test/src/

echo "Creating and starting dev containers"
docker-compose -p rubicon -f docker-compose.yml up -d

rm -rf ./rubicon-test/src