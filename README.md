Rubicon Test Project
===========================

Project is structured into three parts:
* docker
* src
* test

## docker

Created simple docker compose env, so we can run it our application in docker container.
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
        * [FromQuery] tag => string (If tag is not defined, it will return all blogs)
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


## Endpoint examples
Getting all tags:
* GET http://localhost:3000/api/tags

Getting all blog posts:
* GET http://localhost:3000/api/posts

Getting all blog posts with query:
* GET http://localhost:3000/api/posts?tag=Android

Get blog post by slug:
* GET http://localhost:3000/api/posts/title-4

Adding new blog post:
* POST http://localhost:3000/api/posts
    * 
    ```yaml
    {
        "Title": "title 4",
        "Description": "description 4",
        "Body": "body 4",
        "TagList": [
            "IOS",
            "Android",
            "Mac",
            "Test"
        ]
    }

Updating new blog post:
* PUT http://localhost:3000/api/posts/title-4
    * 
    ```yaml
    {
        "Title": "title 4",
        "Description": "description 4",
        "Body": "body 4"
    }

Delete blog post:
* DELETE http://localhost:3000/api/posts/title-4
