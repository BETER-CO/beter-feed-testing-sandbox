# AGENTS.md

Rules for anyone, person or coding agent, who changes this repository. Tool-agnostic:
personal tool configuration stays outside the repository.

## What this repository is

The BETER feed testing sandbox: one container a client runs to test a feed integration
without contacting BETER. It is public. A client reads this repository, pulls the image
`beterco/beter-feed-testing-sandbox` and pins its tag.

## Architecture

| Path | Holds |
| --- | --- |
| `src/Hosts/Beter.Feed.TestingSandbox.Server` | The one host: the four SignalR hubs and the REST operations |
| `src/Hosts/Beter.Feed.TestingSandbox.Consumer` | The sample consumer a client reads; never part of the image |
| `src/Common/*` | Feed models, constants, hosting and logging shared by both |
| `openapi/sandbox.yaml` | The description of the REST operations, hand-written, the master copy |
| `schemas/` | JSON Schemas of the case file and the run output, referenced by the description |
| `cases/` | The case set of the image: one `<caseId>.json` per case, and `retired.json` with the retired ids |
| `tests/*.Server.UnitTests` | Host tests, the OpenAPI drift check, the coverage threshold |
| `tests/*.ContractTests` | Tests of `openapi/` and `schemas/` on their own |
| `tests/*.EndToEndTests` | The test through one container with the sample consumer |

- The feed half of the contract, the hubs `/time_table`, `/trading`, `/scoreboard` and
  `/incident`, is the public feed contract. The sandbox plays it unchanged and never
  extends it.
- The REST half is `openapi/sandbox.yaml`. The host maps every operation of that file; an
  operation that is not built yet answers `501`.
- The container serves the description at `/openapi.json` as one document with the schemas
  inlined (`Description/OpenApiDocumentBundler.cs`). The files in the repository keep their
  relative references.

## Tech stack

- .NET 10 (LTS), ASP.NET Core minimal APIs, SignalR with the JSON and MessagePack protocols.
- xUnit. No database, no message broker, no compose file: one process, one container.
- Package versions live in `Directory.Packages.props` only; a project file carries no
  `Version` attribute.

## Coding conventions

- `.editorconfig`, `.globalconfig`, `stylecop.json` and `Directory.Build.props` are the
  code standard. Do not weaken a rule to silence a warning; fix the code, or suppress at
  the one place with the reason.
- Text files are LF (`.gitattributes`).
- `Nullable` is on by default (`Directory.Build.props`). The host, the sample consumer and
  the host's unit tests still opt out in their project files; the host's `Api/` and
  `Description/` code opts back in per file with `#nullable enable`.
- `GenerateDocumentationFile` is on: a public member without XML documentation is a build
  warning. New public members carry it.

## Build and test

```
dotnet build Beter.Feed.TestingSandbox.sln
dotnet test Beter.Feed.TestingSandbox.sln --filter "Category!=smoke"
dotnet list Beter.Feed.TestingSandbox.sln package --vulnerable --include-transitive
```

The end-to-end test needs Docker and an image name:

```
docker build -t beter-feed-testing-sandbox:local -f src/Hosts/Beter.Feed.TestingSandbox.Server/Dockerfile .
SANDBOX_IMAGE=beter-feed-testing-sandbox:local dotnet test tests/Beter.Feed.TestingSandbox.EndToEndTests --filter "Category=smoke"
```

What the tests enforce, and where:

| Rule | Enforced by |
| --- | --- |
| The mapped operations equal `openapi/sandbox.yaml`: method, path, `operationId`, parameters, request body, response codes and content types, in both directions | `OpenApiDriftTests` |
| The served description has no reference outside itself and embeds the schema files, changed only in `$schema` (dropped) and in their references (made local) | `OpenApiDocumentBundlerTests` |
| The build version has `info.version` of `openapi/sandbox.yaml` as its `major.minor.patch` | `BuildVersion_HasTheCommittedDocumentVersionAsItsCore` |
| Every file of `cases/` validates against the case schema, is named by its id and stays below `100000`; `retired.json` is well-formed | `CaseSetTests` (the host refuses any other case set at start) |
| Line coverage of the host stays at or above 50 % | the `Threshold` of `tests/*.Server.UnitTests` |
| No package, direct or transitive, has a known advisory | the vulnerability step of both workflows |

A change to an operation changes `openapi/sandbox.yaml` and `Api/SandboxApi.cs` together.
A changed schema changes its samples under `schemas/**/examples` too.

## Versioning and release

- One version string: the git tag without `v`, the image tag, `info.version` of the served
  description and `sandboxVersion` of `GET /api/version`.
- `VersionPrefix` in `Directory.Build.props` and `info.version` in `openapi/sandbox.yaml`
  change together. A build without a tag reports `<version>-dev`.
- A tag `vMAJOR.MINOR.PATCH` or `vMAJOR.MINOR.PATCH-rc.N` runs
  `.github/workflows/docker-publish.yml`: tests, push, a keyless Sigstore signature and a
  build provenance attestation. A pre-release tag never moves `latest`.
- The signature names that workflow file at the tag. Renaming the file, or moving the
  publishing steps into a reusable workflow, changes the identity every client verifies
  against; do neither without changing the documented verification command with it.
- Third-party actions are pinned to a commit hash, with the version in a comment.

## Security and public-repository rules

- Nothing internal to BETER and nothing derived from a client or partner enters the
  repository, the image or a commit message: no internal host name, package feed, ticket
  key, partner or customer name, personal path or recorded production data.
- No secret in any file. The publishing workflow reads Docker Hub credentials from
  repository secrets; the signature and the attestation use the workflow's own short-lived
  identity token, and no signing key is stored anywhere.
- Packages come from nuget.org only (`nuget.config`).
- The image runs as a non-root user and listens on port 51858.
