Rubicon Test Project
===========================

Project is structured into three parts:
* docker
* src
* test

## Docker

Created simple docker compose env, so we can run it our application in docker container
For starting application:
* cd docker && ./start_api.sh && cd ..

For rebuild only app image:
* cd docker && ./rebuild_api.sh && cd ..

For rebuild all and app and database image:
* cd docker && ./rebuild_all.sh && cd ..

## src

Here is our application with all models, services and controllers inside it. The most important part are controllers, where we have few controllers:
* TagController
    * Get all tags => Input parameters:
        * none
* BlogController
    * Get all blogs based on tag query => Input parameters:
        * [FromQuery] query => string (If query is not defined, it will return all blogs)
    * Get blog by slug => Input parameters:
        * slug => string
    * Create new blog => Input parameters:
        * [FromBody] blogCreateComplexDto => BlogCreateComplexDto
    * Update existing booking => Input parameters
        * slug => string
        * [FromBody] blogUpdateDto => BlogUpdateDto
    * Delete existing booking => Input parameters
        * slug => string

## test

Application tests