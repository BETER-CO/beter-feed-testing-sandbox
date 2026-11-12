# API docs

This section describes what the sandbox server serves. By default it listens on port `51858`
([Running Applications](RUNNING_APPS.md)).

## Feed hubs

The server serves four SignalR hubs: `/time_table`, `/trading`, `/scoreboard` and `/incident`. Each hub is also
served under `/v1/...`, for example `/v1/trading`. The hubs support the JSON and MessagePack protocols.

Query parameters:

| Parameter           | Description                                                                          |
| ------------------- | ------------------------------------------------------------------------------------ |
| `ApiKey`            | Any non-empty GUID. A missing or invalid value closes the connection.                |
| `snapshotBatchSize` | Accepted. The snapshot is empty on this branch.                                      |

On connect the server sends an empty snapshot: `OnUpdate` with an empty array. After that it sends `OnHeartbeat`
every 5 seconds. The server plays no feed data on this branch.

The feed messages and their data contracts are described in the public feed documentation at
<https://docs.beter.co>.

## REST API

The REST API is described in [`openapi/sandbox.yaml`](../openapi/sandbox.yaml).

| Operation           | Response                                                                             |
| ------------------- | ------------------------------------------------------------------------------------ |
| `GET /api/version`  | `{"sandboxVersion": "...", "outputVersion": 1, "contract": null}`                    |
| `GET /api/health`   | `{"status":"ok"}` once the case set is loaded; `503` with an error body until then   |
| `GET /api/cases`    | The catalogue rows; `?tag=` (repeatable, all must match), `?status=active\|retired\|all` |
| `GET /api/cases/{caseId}` | The case file with `status`; `404` for an unknown id                           |
| `POST /api/cases`   | Loads a case for the container's life: `201` with its row, or `422` with the schema errors |
| `GET /openapi.json` | The description of `openapi/sandbox.yaml` as one self-contained JSON document        |
| `GET /swagger`      | Swagger UI for that description                                                      |

In `GET /openapi.json` the JSON Schemas are inlined and `info.version` is the version of the build.

The `/api/runs...` operations of `openapi/sandbox.yaml` are mapped but not built yet. They answer `501` with
`{"error":"not_implemented","detail":"..."}`.

## Cases

The image carries the case files of [`cases/`](../cases), one `<caseId>.json` per case in the format of
[`schemas/case/case.schema.json`](../schemas/case/case.schema.json), and loads them at start. A file that fails the
schema stops the container.

- The case files are the former recorded scenarios, converted to the case format without re-recording: they carry
  `class`, `sport`, `tags`, `terminal`, `duration` and `script`, and no `covers`, `expected` or `exposes` yet.
- [`cases/retired.json`](../cases/retired.json) lists the retired ids, each with its `date` and `replacedBy` (an id
  or `null`). A retired id is listed with `?status=retired` or `?status=all` and resolves on
  `GET /api/cases/{caseId}` with `status: retired` and `replacedBy`. It carries the case file only while `cases/`
  keeps one.
- `POST /api/cases` takes a case file without `caseId`, or with one from `100000` up. The case is held in memory
  only: it is never written to disk and is gone after a restart.
- The sandbox plays no case yet: runs are not built.

## Version

The git tag, the Docker image tag, `info.version` of the served description and `sandboxVersion` of
`GET /api/version` carry one version string. A tag build takes it from the tag: `v3.0.0-rc.1` gives `3.0.0-rc.1`.
Any other build reports `3.0.0-dev`.
