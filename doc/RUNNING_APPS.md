# Running Applications

## Prerequisites

For testing only:
- [Docker Engine](https://docs.docker.com/engine/install/);
- [Docker Compose Plugin](https://docs.docker.com/compose/install/), not standalone `docker-compose`.

For development (modifying tools or building and running tools locally):
- [Docker Engine](https://docs.docker.com/engine/install/);
- [Docker Compose](https://docs.docker.com/compose/install/), not standalone `docker-compose`;
- [.NET SDK 7](https://dotnet.microsoft.com/en-us/download) or greater.

## Run the applications

You have a few options to start applications:
* [Run pre-built Docker images](#run-pre-built-docker-images) hosted on Docker Hub. The simplest option that launches
all required application in 1 minute.
* [Build Docker images from sources](#build-docker-images-from-sources). This option is useful if your security policies
prohibit running unaudited software locally or on production servers. You can audit this repository, build the images,
and run them. You may also publish locally built images to your own storage, such as Nexus, or to publicly available
services like Docker Hub or GitHub Packages.
* [Local development](#local-development). Launch dependencies in Docker containers locally and build all required
applications locally as well. Applications connect to local Docker containers and exchange data. This option is best
for developing the applications, for example if you want to improve our tool and submit a pull request.

### Run pre-built Docker images [#run-pre-built-docker-images]

BETER maintains the following Docker images:
* [beterco/beter-feed-testing-sandbox-generator](https://hub.docker.com/r/beterco/beter-feed-testing-sandbox-generator)
- image of the Feed Generator application;
* [beterco/beter-feed-testing-sandbox-emulator](https://hub.docker.com/r/beterco/beter-feed-testing-sandbox-emulator)
- image of the Feed Emulator application;

We have prepared [`docker-compose.yml`](../docker-compose.yml) for you to simplify the launch. By default, without any
additional configuration:
* Port `8080` is exposed on the host machine for Kafka UI;
* Kafka ports are not exposed on host machine at all;
* The latest version (latest Docker tag) of the Feed Emulator and Feed Generator published on Docker Hub will be used;
* Feed Generator exposes port `51857` on host machine;
* Feed Emulator exposes port `51858` on host machine;
* Feed Consumer is not launched.

To launch everything by default, follow these steps:

1. Download [`docker-compose.yml`](../docker-compose.yml).

2. Launch it in the background.

```shell
$ docker compose up -d
```

3. Check that the services are running.

```shell
docker ps -a
CONTAINER ID   IMAGE                                                 COMMAND                   CREATED          STATUS                            PORTS                                              NAMES
11c7aedda99f   beterco/beter-feed-testing-sandbox-emulator:latest    "dotnet Beter.Feed.T…"    13 seconds ago   Up 3 seconds (health: starting)   0.0.0.0:51858->80/tcp                              bfts-emulator
6a06c23d0ea7   beterco/beter-feed-testing-sandbox-generator:latest   "dotnet Beter.Feed.T…"    13 seconds ago   Up 3 seconds (health: starting)   0.0.0.0:51857->80/tcp                              bfts-generator
766924df4e61   confluentinc/cp-kafka:7.4.1                           "/bin/sh -c '\n# bloc…"   13 seconds ago   Exited (0) 4 seconds ago                                                             bfts-init-kafka
cc842a46143d   provectuslabs/kafka-ui:latest                         "/bin/sh -c 'java --…"    13 seconds ago   Up 8 seconds                      0.0.0.0:8080->8080/tcp                             bfts-kafka-ui
c0810d48eda7   confluentinc/cp-kafka:7.4.1                           "/etc/confluent/dock…"    13 seconds ago   Up 13 seconds (healthy)           0.0.0.0:64430->9092/tcp, 0.0.0.0:64431->9101/tcp   bfts-kafka
```

> The `bfts-init-kafka` container is in "exited" status. It's ok, as this container launches once to create Kafka topics.

You can configure ports and other options by setting environment variables before launching `docker compose`:
* `BFTS_KAFKA_UI_PORT` - sets the port used for Kafka UI.
* `BFTS_GENERATOR_PORT` - sets the port used by Feed Generator to receive API commands to run/list/stop test scenarios.
* `BFTS_EMULATOR_PORT` - sets port used by Feed Emulator to listen connections from Feed Consumers, and also control
Feed Emulator via REST API.
* `BFTS_GENERATOR_DOCKER_IMAGE_VERSION` - specifies the exact version of the Feed Generator published to Docker Hub (you
may use `latest` as well).
* `BFTS_EMULATOR_DOCKER_IMAGE_VERSION` - specifies the exact version of the Feed Emulator published to Docker Hub (you 
may use `latest` as well).

You can prepend environment variables to the `docker compose` command to activate mentioned settings
```
$ BFTS_KAFKA_UI_PORT=8011 \
  BFTS_GENERATOR_PORT=8012 \
  BFTS_EMULATOR_PORT=8013 \
  BFTS_GENERATOR_DOCKER_IMAGE_VERSION=1.0.1 \
  BFTS_EMULATOR_DOCKER_IMAGE_VERSION=1.0.1 \
  docker compose up -d
```

Or you can
[put environment variables in a `.env`](https://docs.docker.com/compose/environment-variables/variable-interpolation/#ways-to-set-variables-with-interpolation)
in the root of the repository (gitignored) file and run `docker compose` as usual.

```
$ cat .env
BFTS_KAFKA_UI_PORT=8011
BFTS_GENERATOR_PORT=8012
BFTS_EMULATOR_PORT=8013
BFTS_GENERATOR_DOCKER_IMAGE_VERSION=1.0.1
BFTS_EMULATOR_DOCKER_IMAGE_VERSION=1.0.1

$ docker compose up -d
```

4. Use applications

* Kafka UI is available at [http://localhost:8080](http://localhost:8080) or at the port specified in
`BFTS_KAFKA_UI_PORT` environment variable.
* Feed Generator listens for REST API requests on [http://localhost:51857](http://localhost:51857). Swagger
documentation for the API is available at
[http://localhost:51857/swagger/index.html](http://localhost:51857/swagger/index.html). **Note.**
Replace port `51857` with the value of `BFTS_GENERATOR_PORT` if it was modified.
* Feed Emulator listens for REST API requests on [http://localhost:51858](http://localhost:51858). Swagger
documentation for the API is available at
[http://localhost:51858/swagger/index.html](http://localhost:51858/swagger/index.html). **Note.**
Replace port `51858` with the value of `BFTS_EMULATOR_PORT` if it was modified.

### Build Docker images from sources [#build-docker-images-from-sources]

To build Docker images from source, you need to clone this repository first.

1. Clone the repository.

```shell
$ git clone https://github.com/BETER-CO/beter-feed-testing-sandbox.git
$ cd beter-feed-testing-sandbox
```

2. Choose the appropriate version of BFTS.

```shell
$ git tag
v1.0.0
v1.0.1

$ git checkout tags/v1.0.1
```

3. Build images.

Specify the tag to associate with Docker image. As you checked out tag `v1.0.1` in example above, please, set
the version as environment variables for `docker compose` and run the build:

```shell
$ BFTS_GENERATOR_DOCKER_IMAGE_VERSION=1.0.1 \
  BFTS_EMULATOR_DOCKER_IMAGE_VERSION=1.0.1 \
  docker compose -f docker-compose.local-build.yml build 
```

After this step, you will see a newly built images

```shell
$ docker images
REPOSITORY                             TAG                IMAGE ID       CREATED         SIZE
beter-feed-testing-sandbox-generator   1.0.1              ba35d24edafe   2 minutes ago   365MB
beter-feed-testing-sandbox-emulator    1.0.1              e53abe1e3bae   2 minutes ago   278MB
```

> You can now push the images to your local storage.

4. Optionally, you can run the built images on your local machine by running

```shell
$ BFTS_GENERATOR_DOCKER_IMAGE_VERSION=1.0.1 \
  BFTS_EMULATOR_DOCKER_IMAGE_VERSION=1.0.1 \
  docker compose -f docker-compose.local-build.yml up -d
```

Containers should be up and running.

```shell
$ docker ps
CONTAINER ID   IMAGE                                        COMMAND                  CREATED          STATUS                    PORTS                                                                                      NAMES
753df3327cfc   beter-feed-testing-sandbox-generator:1.0.1   "dotnet Beter.Feed.T…"   50 seconds ago   Up 35 seconds (healthy)   0.0.0.0:51857->80/tcp, :::51857->80/tcp                                                    bfts-generator
b80316074b3b   beter-feed-testing-sandbox-emulator:1.0.1    "dotnet Beter.Feed.T…"   50 seconds ago   Up 35 seconds (healthy)   0.0.0.0:51858->80/tcp, :::51858->80/tcp                                                    bfts-emulator
d4ae36108eaf   provectuslabs/kafka-ui:latest                "/bin/sh -c 'java --…"   50 seconds ago   Up 42 seconds             0.0.0.0:8080->8080/tcp, :::8080->8080/tcp                                                  bfts-kafka-ui
db2013a2bf6d   confluentinc/cp-kafka:7.4.1                  "/etc/confluent/dock…"   50 seconds ago   Up 49 seconds (healthy)   0.0.0.0:32771->9092/tcp, :::32771->9092/tcp, 0.0.0.0:32770->9101/tcp, :::32770->9101/tcp   bfts-kafka
```

> If you need to change ports or other options, please, check
[Run pre-built docker images](#run-pre-built-docker-images) section.

5. Use applications

* Kafka UI is available at [http://localhost:8080](http://localhost:8080) or at the port specified in
`BFTS_KAFKA_UI_PORT` environment variable.
* Feed Generator listens for REST API requests on [http://localhost:51857](http://localhost:51857). Swagger
documentation for the API is available at
[http://localhost:51857/swagger/index.html](http://localhost:51857/swagger/index.html). **Note.**
Replace port `51857` with the value of `BFTS_GENERATOR_PORT` if it was modified.
* Feed Emulator listens for REST API requests on [http://localhost:51858](http://localhost:51858). Swagger
documentation for the API is available at
[http://localhost:51858/swagger/index.html](http://localhost:51858/swagger/index.html). **Note.**
Replace port `51858` with the value of `BFTS_EMULATOR_PORT` if it was modified.

### Local development [#local-development]

This method is intended for development purposes only. We assume that you will run Kafka and Kafka UI in Docker, but
the Feed Generator and Feed Emulator will run on your local machine, not in containers.
Therefore, [`docker-compose.development.yml`](../docker-compose.development.yml) differs from
[`docker-compose.yml`](../docker-compose.yml) and [`docker-compose.local-build.yml`](../docker-compose.local-build.yml):

* Kafka exposes port `9092` on the host machine (these can be redefined, see below);
* You should run the Feed Generator and Feed Emulator locally and configure them to use `localhost:9092` (or redefined
port).

Steps:

1. Clone the repository.

```shell
$ git clone https://github.com/BETER-CO/beter-feed-testing-sandbox.git
$ cd beter-feed-testing-sandbox
```

2. Choose the appropriate version of BFTS.

```shell
$ git tag
v1.0.0
v1.0.1

$ git checkout tags/v1.0.1
```

3. Run Kafka and Kafka UI

```shell
$ docker compose -f docker-compose.development.yml up -d
```

If ports `8080` or `9092` is busy you may specify the proper port to use by setting environment variables.

```shell
$ BFTS_KAFKA_UI_PORT=8011 BFTS_KAFKA_PORT=8014 docker compose -f docker-compose.development.yml up -d
```

Verify that everything is running as expected.

```shell
$ docker ps

CONTAINER ID   IMAGE                                    COMMAND                  CREATED          STATUS                    PORTS                                            NAMES
6f7d6a5ab4b9   provectuslabs/kafka-ui:latest            "/bin/sh -c 'java --…"   28 seconds ago   Up 23 seconds             0.0.0.0:8011->8080/tcp                           bfts-kafka-ui
a73c517e79f1   confluentinc/cp-kafka:7.4.1              "/etc/confluent/dock…"   28 seconds ago   Up 28 seconds (healthy)   0.0.0.0:8014->9092/tcp                           bfts-kafka
```

4. Build and run the projects locally.

If the ports haven't changed, run the projects as usual.

```shell
$ dotnet build src/Hosts/Beter.Feed.TestingSandbox.Emulator/Beter.Feed.TestingSandbox.Emulator.csproj
$ dotnet build src/Hosts/Beter.Feed.TestingSandbox.Generator/Beter.Feed.TestingSandbox.Generator.csproj

$ dotnet src/Hosts/Beter.Feed.TestingSandbox.Emulator/bin/Debug/net7.0/Beter.Feed.TestingSandbox.Emulator.dll
# and in second terminal window
$ dotnet src/Hosts/Beter.Feed.TestingSandbox.Generator/bin/Debug/net7.0/Beter.Feed.TestingSandbox.Generator.dll
```

If the Kafka port was changed, you need to specify the new port value before running the applications.

**For Feed Emulator**:

* change the value of `Messaging.ConsumerConfig.BootstrapServers` in
`src/Hosts/Beter.Feed.TestingSandbox.Emulator/configs/appsettings.json` and rebuild the project;
* alternatively, set the environment variable `Messaging__ConsumerConfig__BootstrapServers`.
* specify the port of the Feed Emulator by setting the environment variable `ASPNETCORE_URLS="http://+:51858"`
to open port `51858`.

Example,

```shell
# if Kafka's port is exposed to port 8014 on host machine...
$ ASPNETCORE_URLS="http://+:51858" \
  Messaging__ConsumerConfig__BootstrapServers="localhost:8014" \
  src/Hosts/Beter.Feed.TestingSandbox.Emulator/bin/Debug/net7.0/Beter.Feed.TestingSandbox.Emulator.dll
```

For Feed Generator:

* change the values of `FeedEmulator.ApiBaseUrl` (Kafka broker host and port) in
`src/Hosts/Beter.Feed.TestingSandbox.Generator/configs/appsettings.json` and rebuild the project;
* alternatively, set the environment variables `Publisher__BootstrapServers` and `FeedEmulator__ApiBaseUrl`.
* specify the port of the Feed Emulator by setting the environment variable `ASPNETCORE_URLS="http://+:51857"`
 to open port 51857.

Example,

```shell
# if Kafka's port is exposed to port 8014 on host machine...
# ... and Feed Emulator oponed port 51858 for incoming connections...
$ ASPNETCORE_URLS="http://+:51857" \
  Publisher__BootstrapServers="localhost:8014" \
  FeedEmulator__ApiBaseUrl="localhost:51858" \
  dotnet bin/Debug/net7.0/Beter.Feed.TestingSandbox.Generator.dll
```
5. Use applications

* Kafka UI is available at [http://localhost:8080](http://localhost:8080) or at the port specified in
`BFTS_KAFKA_UI_PORT` environment variable.
* Feed Generator listens for REST API requests on [http://localhost:51857](http://localhost:51857). Swagger
documentation for the API is available at
[http://localhost:51857/swagger/index.html](http://localhost:51857/swagger/index.html). **Note.**
Replace port `51857` with the value of `ASPNETCORE_URLS` if it was modified.
* Feed Emulator listens for REST API requests on [http://localhost:51858](http://localhost:51858). Swagger
documentation for the API is available at
[http://localhost:51858/swagger/index.html](http://localhost:51858/swagger/index.html). **Note.**
Replace port `51858` with the value of `ASPNETCORE_URLS` if it was modified.
* Kafka is listening on `9092`, or on `Publisher__BootstrapServers` if the port was redefined.