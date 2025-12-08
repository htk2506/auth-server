# auth-server

Implementing a simple authentication server.

## Running with Docker

1. Build the image.

`docker build -t auth-server .`

2. Make sure environment variables/secrets are configured in the run environment (see [AuthServer/appsettings.json](AuthServer/appsettings.json)). Run the container.

`docker run -d -p HOST_PORT:CONTAINER_PORT auth-server`