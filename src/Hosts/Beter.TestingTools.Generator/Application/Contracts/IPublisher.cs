﻿using Beter.TestingTools.Models;
using Beter.TestingTool.Generator.Domain.TestScenarios;

namespace Beter.TestingTool.Generator.Application.Contracts;

public interface IPublisher
{
    Task PublishAsync(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken);
    Task PublishAsync(HeartbeatModel heartbeatNotification, CancellationToken cancellationToken);
    Task PublishEmptyAsync(string messageType, string channel, string playbackId, CancellationToken cancellationToken);
}