#nullable enable

using System.Text.Json;

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>One entry of a case script, as the player schedules it.</summary>
/// <param name="Index">The zero-based position of the entry in <c>script</c>.</param>
/// <param name="At">The offset from the case start, in milliseconds of feed time.</param>
/// <param name="Channel">The channel of a message entry; null for a directive.</param>
/// <param name="MessageType">The <c>messageType</c> of a message entry; null for a directive.</param>
/// <param name="Directive">The directive of a directive entry; null for a message.</param>
/// <param name="Value">The <c>value</c> of a message entry; <see cref="JsonValueKind.Undefined"/> for a directive.</param>
internal sealed record ScriptEntry(int Index, long At, string? Channel, string? MessageType, string? Directive, JsonElement Value)
{
    /// <summary>Gets a value indicating whether the entry is a fault directive.</summary>
    public bool IsDirective => Directive is not null;
}
