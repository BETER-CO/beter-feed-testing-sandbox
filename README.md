# BETER Feed Testing Sandbox (BFTS)

This repository contains a sandbox for testing integrations with BETER's Feed. It consists of:

- **Sandbox server** (`src/Hosts/Beter.Feed.TestingSandbox.Server`): serves the four Feed SignalR hubs and the
REST API described in [`openapi/sandbox.yaml`](openapi/sandbox.yaml). It is published as the Docker image
`beterco/beter-feed-testing-sandbox`.
- **Sample consumer** (`src/Hosts/Beter.Feed.TestingSandbox.Consumer`): a simple Feed consumer that connects to the
four hubs and logs every message it receives. It is a testing client for development, not for production usage.

State of this branch: the server accepts hub connections, sends an empty snapshot and heartbeats, and answers the
version, health and API description operations. It loads the case files of [`cases/`](cases) and serves the case
catalogue, and takes a custom case at runtime. A run plays a case to the hub connections that present the run's
`ApiKey`. The run output is not built yet: the transcript and timeline operations answer `501`, and the run summary is
empty. The former Generator host, Kafka, Kafka UI and the docker-compose files are removed.

## Documentation

- [Building and Running Applications](doc/RUNNING_APPS.md)
- [Running Test Consumer](doc/TEST_CONSUMER.md) (optional)
- [Sandbox API](doc/API.md)
- [Sandbox contract](openapi/README.md)

## Contributing

We welcome contributions! If you encounter any issues or have suggestions for improvements, please open an issue or submit a pull request in the repository. For detailed guidelines on how to contribute, please refer to our [Contributing Guide](CONTRIBUTING.md).
