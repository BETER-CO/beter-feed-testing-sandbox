﻿using Microsoft.Extensions.Logging.Abstractions;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging;

public class GeneratorMessagesListener : BackgroundService
{
    private readonly IGeneratorMessagesConsumer _consumer;
    private readonly ILogger<GeneratorMessagesListener> _logger;

    public GeneratorMessagesListener(IGeneratorMessagesConsumer consumer, ILogger<GeneratorMessagesListener> logger)
    {
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _logger = logger ?? NullLogger<GeneratorMessagesListener>.Instance;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _consumer.StartConsuming(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error while consuming generator messages");
        }
    }

    public override void Dispose()
    {
        _consumer.Dispose();

        base.Dispose();
    }
}