# API docs

This section describes what the sandbox server serves. By default it listens on port `51858`
([Running Applications](RUNNING_APPS.md)).

## Feed hubs

The server serves four SignalR hubs: `/time_table`, `/trading`, `/scoreboard` and `/incident`. Each hub is also
served under `/v1/...`, for example `/v1/trading`. The hubs support the JSON and MessagePack protocols.

Query parameters:

| Parameter           | Description                                                                          |
| ------------------- | ------------------------------------------------------------------------------------ |
| `ApiKey`            | Any non-empty GUID. A missing or invalid value closes the connection. The value binds the connection to the runs started with it. |
| `snapshotBatchSize` | Accepted. The snapshot is empty on this branch.                                      |

On connect the server sends an empty snapshot: `OnUpdate` with an empty array. After that it sends `OnHeartbeat`
every 5 seconds. A connection whose `ApiKey` matches no run receives these and nothing else; the feed data of a case
reaches the connections of a run's key ([Runs](#runs)).

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
| `POST /api/runs`    | Starts a run of one case: `201` with the run manifest; `400`, `404`, or `410` for a retired case |
| `GET /api/runs`     | The manifests of every run the container holds, oldest first; `?status=` filters, `400` for an unknown status |
| `GET /api/runs/{runId}` | The manifest of a run; `404` for an unknown id                                   |
| `POST /api/runs/{runId}/stop` | Stops a run: `200` with the manifest; `409` for a run in a terminal status   |
| `DELETE /api/runs/{runId}` | Removes a run in a terminal status: `204`; `409` while it is `waiting` or `playing` |
| `GET /openapi.json` | The description of `openapi/sandbox.yaml` as one self-contained JSON document        |
| `GET /swagger`      | Swagger UI for that description                                                      |

In `GET /openapi.json` the JSON Schemas are inlined and `info.version` is the version of the build.

`GET /api/runs/{runId}/messages` and `GET /api/runs/{runId}/events` are mapped but not built yet. They answer `501`
with `{"error":"not_implemented","detail":"..."}`. Every other error answers its status code with
`{"error":"...","detail":"..."}`.

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
- A run plays a case ([Runs](#runs)).

## Runs

A run plays one case to every hub connection that presents the run's `apiKey` as the feed `ApiKey`.

- `POST /api/runs` takes `caseId`, `apiKey`, and optionally `acceleration` (default `1`), `start` (default the case's
  `start`, else `onConnect`) and `waitTimeoutSeconds` (default `60`). Any other property is refused with `400`.
- `apiKey` is a non-empty GUID, as the hubs require of the feed `ApiKey`, in the hyphenated 8-4-4-4-12 form, in any
  letter case. Any other form, braces or 32 digits without hyphens included, is refused with `400`. Keys are compared
  as GUIDs, so a hub connection presenting the same GUID in another letter case is bound to the run; the manifest
  gives the key in lowercase.
- `acceleration` is from `0.25` to the case's `maxAcceleration`, `20` for a case that names none. It scales the
  script clock only: the heartbeat interval and `waitTimeoutSeconds` are real time.
- Several runs under one key play at the same time, each on its own clock, as distinct matches.
- The end, stop or deletion of a run closes no connection. An open connection receives the next run of its key.

### Status

A run is `waiting`, then `playing`, then one of `completed`, `stopped` and `failed`.

| Status      | Meaning                                                                    |
| ----------- | -------------------------------------------------------------------------- |
| `waiting`   | The script clock has not started.                                          |
| `playing`   | The script clock runs.                                                     |
| `completed` | The last script entry was sent.                                            |
| `stopped`   | `POST /api/runs/{runId}/stop` stopped the run, or the container shut down. |
| `failed`    | The sandbox could not play the case; `error` says why.                     |

With `start: onConnect` the script clock starts when a connection presenting the key is open on any of the four hubs:
at creation when one already is, else at the first such connect. When none joins within `waitTimeoutSeconds`, the run
ends `failed` with `error` `no connection joined`. With `start: now` the clock starts at creation, and the run plays
to its end with or without a connection.

A script entry that cannot be played, such as a value that does not deserialize into the channel's model or an
unsupported `messageType`, ends the run `failed` with an `error` that names the script index and the cause.

### Playback

Entry `i` is sent at `startedAt + at / acceleration`, in `at` order; entries with the same `at` keep their script
order. A connection made after the clock started receives only the messages sent after it joined.

| `messageType`                                     | Sent as                                                         |
| ------------------------------------------------- | --------------------------------------------------------------- |
| `time_table`, `trading`, `scoreboard`, `incident` | `OnUpdate` on the hub of the entry's `channel`                  |
| `system_event`                                    | `OnSystemEvent` on the hub of the entry's `channel`             |
| `subscriptions_removed`                           | `OnSubscriptionsRemove` with the `ids` of the entry's value, on the hub of its `channel` |

Directive entries (`connectionDrop`, `heartbeatStop`, `heartbeatResume`) are not acted on yet: the run passes over
them.

Each run rewrites the recorded content:

- Match ids: every recorded match id gets one new GUID for the run, the same on the four channels. This covers the
  `id` of every item and every id of `subscriptions_removed`. Two runs never share an id.
- Offsets: one counter for the container, starting at `1` and shared by every run. An item with `msgType` `1`
  (update) takes the next number; any other `msgType` repeats the last number of the match on that channel, or takes
  the next number when there is none. `system_event` and `subscriptions_removed` carry no offset.
- Timestamps: for each match and channel, the first item in script order is the base, and
  `new = startedAt + (recorded - base) / acceleration`. This applies to `timestamp` of time_table, trading and
  scoreboard, `date` of incident, and `startDate` of time_table, which takes the time_table base of its match. A
  scoreboard `timer.timeStamp` takes the item's new `timestamp`. No other field changes.

### Manifest

The run operations return the run manifest of
[`schemas/run-output/v1/run.schema.json`](../schemas/run-output/v1/run.schema.json). Its `summary` is the empty
block, and the two stream operations answer `501`, until the run output is built.

## Version

The git tag, the Docker image tag, `info.version` of the served description and `sandboxVersion` of
`GET /api/version` carry one version string. A tag build takes it from the tag: `v3.0.0-rc.1` gives `3.0.0-rc.1`.
Any other build reports `3.0.0-dev`.
