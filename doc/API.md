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
| `GET /api/health`   | `{"status":"ok"}`                                                                    |
| `GET /openapi.json` | The description of `openapi/sandbox.yaml` as one self-contained JSON document        |
| `GET /swagger`      | Swagger UI for that description                                                      |

In `GET /openapi.json` the JSON Schemas are inlined and `info.version` is the version of the build.

The other operations of `openapi/sandbox.yaml` (`/api/cases...` and `/api/runs...`) are mapped but not built yet.
They answer `501` with `{"error":"not_implemented","detail":"..."}`.

## Version

The git tag, the Docker image tag, `info.version` of the served description and `sandboxVersion` of
`GET /api/version` carry one version string. A tag build takes it from the tag: `v3.0.0-rc.1` gives `3.0.0-rc.1`.
Any other build reports `3.0.0-dev`.
