# Contributing to BETER Feed Testing Sandbox (BFTS)

## Introduction

Thank you for considering contributing to the BETER Feed Testing Sandbox (BFTS) project! This document provides guidelines and instructions to help you contribute effectively.

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [How to Contribute](#how-to-contribute)
   - [Fork the Repository](#fork-the-repository)
   - [Create a Branch](#create-a-branch)
   - [Make Changes](#make-changes)
   - [Test Your Changes](#test-your-changes)
   - [Submit a Pull Request](#submit-a-pull-request)
3. [Guidelines for Development](#guidelines-for-development)
   - [Versioning Process](#versioning-process)
4. [Reporting Issues](#reporting-issues)
5. [Getting Help](#getting-help)

## Code of Conduct

If you encounter any issues or unacceptable behavior, please report them by opening a pull request (PR) in the repository. Detailed descriptions and proper documentation in the PR will help us address the issues effectively.


## How to Contribute

### Fork the Repository

For detailed instructions on how to set up your local development environment and launch services, please refer to the [Local Development](doc/RUNNING_APPS.md#local-development) section in the repository documentation.


### Create a Branch

1. Clone the repository to your local machine:

```bash
git clone https://github.com/BETER-CO/beter-feed-testing-sandbox.git
cd beter-feed-testing-sandbox
```
2. Create a new branch for your feature or bug fix following GitHub's recommended flow. Use descriptive names for your branches, such as feature/your-feature-name or bugfix/your-bugfix-description:

```bash
git checkout -b your-branch-name
```

### Make Changes

1. Make your changes to the codebase, following the project's coding standards and guidelines. For detailed information on the coding standards and guidelines, please refer to the [Coding Standards](doc/CODING_STANDARDS.md) document.


### Test Your Changes

1. Ensure that your changes do not break existing functionality by running tests. The commands are in the [Build and test](doc/RUNNING_APPS.md#build-and-test) section.
2. Build and run the project locally to verify your changes.

### Submit a Pull Request

1. Push your branch to your forked repository:

```bash
git push origin your-branch-name
```

2. Open a pull request (PR) in the main repository:
* Go to the main repository.
* Click on the "Pull Requests" tab.
* Click on the "New Pull Request" button.
* Select your branch and compare it with the main branch.
* Provide a detailed description of your changes and submit the PR.

## Guidelines for Development

### Versioning Process

The BETER Feed Testing Sandbox (BFTS) project uses Semantic Versioning (SemVer) to manage releases.

#### Version Format

The version format is: `MAJOR.MINOR.PATCH`, with an optional pre-release suffix `-rc.N`.

- **MAJOR**: Incremented when there are incompatible API changes or significant new features that break backward compatibility.
- **MINOR**: Incremented when new, backwards-compatible functionality is introduced.
- **PATCH**: Incremented when backwards-compatible bug fixes are made.

#### Release tags

The git tag, the Docker image tag, `info.version` of the served API description and `GET /api/version` carry one version string. A release is a git tag `vMAJOR.MINOR.PATCH` or `vMAJOR.MINOR.PATCH-rc.N`; the image tag is the tag without the leading `v`. A build that does not start from a tag reports `3.0.0-dev`.


## Reporting Issues

If you encounter any issues or have suggestions for improvements, please open an issue in the repository. Provide as much detail as possible to help us understand and address the problem.

## Getting Help

If you need help or have any questions, feel free to reach out via:

* The [Issues](https://github.com/BETER-CO/beter-feed-testing-sandbox/issues) section in GitHub
* By submitting a pull request (PR) with detailed information regarding your issue or question.


Thank you for your contributions and support!

