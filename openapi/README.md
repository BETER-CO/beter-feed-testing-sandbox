# Sandbox contract

The machine-readable contract of the sandbox, version 3.0.0. It is written
ahead of the code: the sandbox on this branch does not serve these operations
yet, and `doc/API.md` describes what it serves today.

| File | Describes |
| --- | --- |
| `openapi/sandbox.yaml` | The operations a client's tests call to list cases, start a run and read its output (OpenAPI 3.1) |
| `schemas/case/case.schema.json` | The case file |
| `schemas/run-output/v1/run.schema.json` | `run.json`, the manifest of a run |
| `schemas/run-output/v1/message.schema.json` | One line of `messages.jsonl` |
| `schemas/run-output/v1/event.schema.json` | One line of `events.jsonl` |

The schemas are JSON Schema 2020-12. The OpenAPI document references them by
relative path and copies none of them. `examples/` beside each schema holds
samples that validate against it.

The feed itself, the four hubs and their messages, is the public feed contract
at <https://docs.beter.co> and is not described here.

## Checking a change

```
dotnet test tests/Beter.Feed.TestingSandbox.ContractTests
```

The tests validate the OpenAPI document against the OpenAPI 3.1 schema, check
its operations, error responses and references, and validate the samples
against the schemas.
