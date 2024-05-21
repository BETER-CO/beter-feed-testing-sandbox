﻿namespace Beter.Feed.TestingSandbox.Generator.Application.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimeMilliseconds(this DateTime dateValue)
    {
        return new DateTimeOffset(dateValue).ToUnixTimeMilliseconds();
    }
}