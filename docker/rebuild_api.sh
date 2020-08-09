#!/bin/bash

api_container_name=rubicon

if [ "$( docker container inspect -f '{{.State.Running}}' $api_container_name )" == "true" ]; then
    docker stop $api_container_name
    docker rm $api_container_name
elif [  "$( docker container inspect -f '{{.State.Status}}' $api_container_name )" == "exited" && 
        "$( docker container inspect -f '{{.State.Running}}' $api_container_name )" == "false"]; then
    docker rm $api_container_name;
fi

docker rmi dev_$api_container_name

sh ./start_api.sh