# Beter Feed Testing Sandbox

This repository contains a suite of services designed for testing Beter's Feed system. The project consists of three main services:

- **Feed Generator**: Serves as a repository for test scenarios, enabling users to upload new scenarios and execute them to publish data to Kafka.
- **Feed Emulator**: The application on the server that reads data from Kafka and sends it to all connected users, emulating the feed's functionality.
- **Feed Consumer**: The client for the Emulator, capable of connecting to the feed and listening for data. It is primarily utilized as a **testing client** rather than for production purposes.

## Prerequisites

- [Docker](https://docs.docker.com/desktop/install/windows-install/) and [Docker Compose](https://docs.docker.com/compose/install/) installed.

## Run the Services

To start the feed services, follow these steps:
1. **Clone the Repository:**
    ```sh
    git clone https://github.com/BETER-CO/beter-feed-testing-sandbox.git
    cd beter-feed-testing-sandbox
    ```

2. **Docker Configuration:**
    Make sure you have two Docker Compose files in the root directory:
    - `docker-compose.yaml`
    - `docker-compose.override.yaml`

3. **Start the Docker Services:**
    Run the following command:

    ```sh
    docker-compose up --build
    ```

    This command builds the Docker images and starts the services defined in the `docker-compose.yaml` and `docker-compose.override.yaml` files. Each of the feed services (Generator, Emulator) is built from local Dockerfiles and runs on specific ports. They all depend on Kafka and Kafka-UI dependencies.

Upon running `docker-compose up --build`, the following containers will be created:

- **kafka**: Runs Confluent's Kafka server, exposing ports 9092 and 9101 for communication.
- **init-kafka**: A service used for initializing Kafka topics required by the application.
- **kafka-ui**: Provides a user interface for managing Kafka topics, accessible via port 8080.
- **beter.feed.testingsandbox.generator**: Hosts the Feed Generator service, running on port 51857.
- **beter.feed.testingsandbox.emulator**: Hosts the Feed Emulator service, running on port 51858.

These containers are interconnected and rely on each other, with dependencies managed through Docker Compose.

## Documentation
For comprehensive documentation covering all aspects of the Beter Feed Testing Sandbox, including usage guidelines and API description visit [https://docs.beter.co/public/beter-feed-testing-sandbox/general](https://docs.beter.co/public/beter-feed-testing-sandbox/general). There you will find detailed documentation to help you make the most of the testing sandbox. 

## Contributing
Contributions are welcome! If you encounter any issues or have suggestions for improvement, feel free to open an issue or submit a pull request in the repository.
