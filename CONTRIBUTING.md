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
   - [Resource Requirements](#resource-requirements)
   - [Load testing resources requirements](#load-testing-resources-requirements)
   - [Versioning Process](#versioning-process)
4. [Reporting Issues](#reporting-issues)
5. [Getting Help](#getting-help)

## Code of Conduct

If you encounter any issues or unacceptable behavior, please report them by opening a pull request (PR) in the repository. Detailed descriptions and proper documentation in the PR will help us address the issues effectively.


## How to Contribute

### Fork the Repository

For detailed instructions on how to set up your local development environment and launch services, please refer to the [Local Development](https://github.com/BETER-CO/beter-feed-testing-sandbox/blob/main/doc/RUNNING_APPS.md#local-development) section in the repository documentation.


### Create a Branch

1. Create a new branch for your feature or bug fix following GitHub's recommended flow. Use descriptive names for your branches, such as `feature/your-feature-name` or `bugfix/your-bugfix-description`:

```bash
git checkout -b your-branch-name
```
2. Clone the repository, make your changes, and then provide a pull request (PR) to the main repository.

### Make Changes

### Make Changes

1. Make your changes to the codebase, following the project's coding standards and guidelines. For detailed information on the coding standards and guidelines, please refer to the [Coding Standards](https://github.com/BETER-CO/beter-feed-testing-sandbox/blob/main/doc/CODING_STANDARDS.md) document.


### Test Your Changes

1. Ensure that your changes do not break existing functionality by running tests.
2. Build and run the project locally to verify your changes.

### Submit a Pull Request

1. Push your branch to your forked repository:

```bash
git push origin feature/your-feature-name
```
2. Open a pull request (PR) in the main repository:
* Go to the main repository.
* Click on the "Pull Requests" tab.
* Click on the "New Pull Request" button.
* Select your branch and compare it with the main branch.
* Provide a detailed description of your changes and submit the PR.

## Guidelines for Development

### Resource Requirements

- Total RAM allocated: 2.5 GB
- Total CPU cores allocated: 1 core


### Load testing resources requirements

- Total RAM allocated: 8 GB
- Total CPU cores allocated: 8 cores

### Versioning Process

The BETER Feed Testing Sandbox (BFTS) project uses the latest Semantic Version (SemVer) to manage releases. This ensures that version numbers provide meaningful information about the changes in each release.

#### Version Format

The version format is: `MAJOR.MINOR.PATCH`

- **MAJOR**: Incremented when there are incompatible API changes or significant new features that break backward compatibility.
- **MINOR**: Incremented when new, backwards-compatible functionality is introduced.
- **PATCH**: Incremented when backwards-compatible bug fixes are made.

#### Scenarios for Version Updates

1. **Major Version Updates:**
   - When data contracts of the Feed change completely, all Emulated Test Cases must be updated.
   - The major version must be incremented (+1) and the minor version must be reset to 0.
     - Example: Version `1.2` -> Version `2.0`
   - Every major change of version must be reflected in the update of git tags and Docker image versions respectively.

2. **Minor Version Updates:**
   - When specific Emulated Test Cases are updated as a reaction to a bug, the minor version must be incremented.
     - Example: Version `1.2` -> Version `1.3`
   - Every minor change of version must be reflected in the update of git tags and Docker image versions respectively.

#### Documentation

1. Documentation describing the Emulated Test Cases must be present in the git repository. For detailed descriptions, please refer to the [SCENARIOS.md](https://github.com/BETER-CO/beter-feed-testing-sandbox/blob/main/doc/SCENARIOS.md) document.
2. The history of changes must be documented for every Emulated Test Case, including both major and minor updates, with every version change implicitly specified.
3. The documentation must explain any limitations according to current requirements.
4. Changes to Emulated Test Cases are forbidden without an update of the version and an update of the description and log of changes.
5. There is no need to keep all versions of the test cases in the repository. If a client needs an old version, it can be obtained from the git repository or Docker image.
6. Emulated Test Cases may be deleted if they are no longer required. However, the ID cannot be reused by another Emulated Test Case. Documentation must be updated to keep a record of deprecated/removed Emulated Test Cases, with a special mark/status indicating that the Emulated Test Case was removed.

#### Example Version Update

Let's say we ship an initial and very first version of the Feed Emulator.
- **Emulated Test Case 1 version:** 1.0
- **Emulated Test Case 2 version:** 1.0
- **Version of Feed Emulator:** 1.0

After that, Emulated Test Case 2 was updated (not a major change). This affects versions and the new versions may be found below:
- **Emulated Test Case 1 version:** 1.0
- **Emulated Test Case 2 version:** 1.1
- **Version of Feed Emulator:** 1.1

After that, Emulated Test Case 3 was added. This affects versions and the new versions may be found below:
- **Emulated Test Case 1 version:** 1.0
- **Emulated Test Case 2 version:** 1.1
- **Emulated Test Case 3 version:** 1.0
- **Version of Feed Emulator:** 1.2

After that, data contracts of the Feed changed. This requires an update of every test case. This affects versions and the new versions may be found below:
- **Emulated Test Case 1 version:** 2.0
- **Emulated Test Case 2 version:** 2.0
- **Emulated Test Case 3 version:** 2.0
- **Version of Feed Emulator:** 2.0

After that, Emulated Test Case 2 was updated (not a major change). This affects versions and the new versions may be found below:
- **Emulated Test Case 1 version:** 2.0
- **Emulated Test Case 2 version:** 2.1
- **Emulated Test Case 3 version:** 2.0
- **Version of Feed Emulator:** 2.1


## Reporting Issues

If you encounter any issues or have suggestions for improvements, please open an issue in the repository. Provide as much detail as possible to help us understand and address the problem.

## Getting Help

If you need help or have any questions, feel free to reach out via:

* The [Issues](https://github.com/BETER-CO/beter-feed-testing-sandbox/issues) section in GitHub
* By submitting a pull request (PR) with detailed information regarding your issue or question.


Thank you for your contributions and support!

