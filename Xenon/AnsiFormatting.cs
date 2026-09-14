using System;

namespace AnsiFormatting;

/// <summary>
/// Supported standard 8-color terminal palette.
/// </summary>
public enum Color
{
    Black = 0,
    Red = 1,
    Green = 2,
    Yellow = 3,
    Blue = 4,
    Magenta = 5,
    Cyan = 6,
    White = 7
}

/// <summary>
/// Base ANSI Escape codes mapped from your original layout.
/// </summary>
public static class AnsiCodes
{
    public static readonly string[] Regular =
    [
        "\u001b[0;30m", "\u001b[0;31m", "\u001b[0;32m", "\u001b[0;33m",
        "\u001b[0;34m", "\u001b[0;35m", "\u001b[0;36m", "\u001b[0;37m"
    ];

    public static readonly string[] Bold =
    [
        "\u001b[1;30m", "\u001b[1;31m", "\u001b[1;32m", "\u001b[1;33m",
        "\u001b[1;34m", "\u001b[1;35m", "\u001b[1;36m", "\u001b[1;37m"
    ];

    public static readonly string[] Underline =
    [
        "\u001b[4;30m", "\u001b[4;31m", "\u001b[4;32m", "\u001b[4;33m",
        "\u001b[4;34m", "\u001b[4;35m", "\u001b[4;36m", "\u001b[4;37m"
    ];

    public static readonly string[] Background =
    [
        "\u001b[40m", "\u001b[41m", "\u001b[42m", "\u001b[43m",
        "\u001b[44m", "\u001b[45m", "\u001b[46m", "\u001b[47m"
    ];

    public static readonly string[] HighIntens =
    [
        "\u001b[0;90m", "\u001b[0;91m", "\u001b[0;92m", "\u001b[0;93m",
        "\u001b[0;94m", "\u001b[0;95m", "\u001b[0;96m", "\u001b[0;97m"
    ];

    public static readonly string[] BoldHighIntens =
    [
        "\u001b[1;90m", "\u001b[1;91m", "\u001b[1;92m", "\u001b[1;93m",
        "\u001b[1;94m", "\u001b[1;95m", "\u001b[1;96m", "\u001b[1;97m"
    ];

    public static readonly string[] HighIntensBackground =
    [
        "\u001b[0;100m", "\u001b[0;101m", "\u001b[0;102m", "\u001b[0;103m",
        "\u001b[0;104m", "\u001b[0;105m", "\u001b[0;106m", "\u001b[0;107m"
    ];

    public const string Reset = "\u001b[0m";
}

/// <summary>
/// Fluent Extension methods for styling system strings natively.
/// </summary>
public static class FormatterExtensions
{
    public static string ToRegular(this string text, Color color) =>
        $"{AnsiCodes.Regular[(int)color]}{text}{AnsiCodes.Reset}";

    public static string ToBold(this string text, Color color) =>
        $"{AnsiCodes.Bold[(int)color]}{text}{AnsiCodes.Reset}";

    public static string ToUnderline(this string text, Color color) =>
        $"{AnsiCodes.Underline[(int)color]}{text}{AnsiCodes.Reset}";

    public static string ToHighIntens(this string text, Color color) =>
        $"{AnsiCodes.HighIntens[(int)color]}{text}{AnsiCodes.Reset}";

    public static string ToBoldHighIntens(this string text, Color color) =>
        $"{AnsiCodes.BoldHighIntens[(int)color]}{text}{AnsiCodes.Reset}";

    public static string WithBg(this string text, Color color) =>
        $"{AnsiCodes.Background[(int)color]}{text}{AnsiCodes.Reset}";

    public static string WithHighIntensBg(this string text, Color color) =>
        $"{AnsiCodes.HighIntensBackground[(int)color]}{text}{AnsiCodes.Reset}";
}
