# Running Applications

## Prerequisites

For running the published image:
- [Docker Engine](https://docs.docker.com/engine/install/).

For development (modifying tools or building and running tools locally):
- [Docker Engine](https://docs.docker.com/engine/install/), for the local image and the end-to-end tests;
- [.NET SDK 10](https://dotnet.microsoft.com/en-us/download) or greater.

## Run the applications

### Run the pre-built Docker image <a id="run-pre-built-docker-images" />

BETER maintains one Docker image:
[beterco/beter-feed-testing-sandbox](https://hub.docker.com/r/beterco/beter-feed-testing-sandbox), the image of the
sandbox server.

```shell
$ docker run -p 51858:51858 beterco/beter-feed-testing-sandbox:<tag>
```

The image tag is the version without the leading `v` of the git tag, for example `3.0.0-rc.1` for `v3.0.0-rc.1`.

### Build the Docker image from sources <a id="build-docker-images-from-sources" />

Clone the repository and run the build from the repository root:

```shell
$ git clone https://github.com/BETER-CO/beter-feed-testing-sandbox.git
$ cd beter-feed-testing-sandbox
$ docker build -t beter-feed-testing-sandbox:local -f src/Hosts/Beter.Feed.TestingSandbox.Server/Dockerfile .
$ docker run -p 51858:51858 beter-feed-testing-sandbox:local
```

### Local development <a id="local-development" />

Run the sandbox server without Docker. It listens on port `51858`.

```shell
$ dotnet run --project src/Hosts/Beter.Feed.TestingSandbox.Server
```

The sample consumer is described in [Running Test Consumer](TEST_CONSUMER.md).

## Use the applications

The sandbox server serves the Feed hubs and the REST API on port `51858`. Swagger UI is available at
`http://localhost:51858/swagger`. The served operations are described in the [API document](API.md).

## Build and test

```shell
$ dotnet build Beter.Feed.TestingSandbox.sln
$ dotnet test Beter.Feed.TestingSandbox.sln --filter "Category!=smoke"
```

The second command runs the unit tests, the contract tests and the OpenAPI drift check, and applies a 50 % line
coverage threshold to the server.

The end-to-end tests run against one container. Build the local image as shown above, then run:

```shell
$ SANDBOX_IMAGE=beter-feed-testing-sandbox:local dotnet test tests/Beter.Feed.TestingSandbox.EndToEndTests --filter "Category=smoke"
```
