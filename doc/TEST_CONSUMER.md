# Running Testing Feed Consumer

## Prerequisites

The sandbox server must be running ([Running Applications](RUNNING_APPS.md)). The consumer retries the connection
while the sandbox is not up.

The Feed Consumer from this repository connects to the four Feed hubs of the sandbox, receives messages via SignalR
and passes each received message to an `IFeedMessageSink`. The default sink logs one line per message.

You will likely implement your own consumer with specific business logic, so feel free to use your own consumer
for testing integration. This Feed Consumer is suitable for sandbox development and data discovery.

## Configuration

The consumer reads the section `FeedService` of `src/Hosts/Beter.Feed.TestingSandbox.Consumer/configs/appsettings.json`:

| Key                       | Description                                              |
| ------------------------- | -------------------------------------------------------- |
| `Host`                    | Address of the sandbox, default `http://localhost:51858` |
| `ApiKey`                  | API key sent to the hubs, a GUID. The consumer receives the runs started with the same value as `apiKey` ([Sandbox API](API.md#runs)) |
| `SnapshotBatchSize`       | Value of the `snapshotBatchSize` query parameter, sent when greater than 0 |
| `ReconnectionWaitSeconds` | Wait between connection attempts, in seconds             |
| `Protocol`                | `json` (default) or `messagepack`                        |

Each key can also be set with an environment variable, for example `FeedService__Host`.

## Run testing Feed Consumer

```shell
$ dotnet run --project src/Hosts/Beter.Feed.TestingSandbox.Consumer
```
