using System.Text;

namespace ALE.ACS.Engine.PlcCodeGen;

/// <summary>
/// Writer para Structured Text (IEC 61131-3) com indentação automática.
/// Gera código compatível com CodeSys, Siemens SCL (TIA Portal) e Allen-Bradley ST.
/// </summary>
public sealed class StructuredTextWriter
{
    private readonly StringBuilder _sb = new();
    private int _indent;
    private bool _lineStartPending = true;
    private readonly string _indentUnit;

    /// <summary>Constrói com unidade de indentação (padrão: 4 espaços).</summary>
    public StructuredTextWriter(string indentUnit = "    ")
    {
        _indentUnit = indentUnit;
    }

    /// <summary>Nível de indentação atual.</summary>
    public int Indent
    {
        get => _indent;
        set => _indent = Math.Max(0, value);
    }

    /// <summary>Abre um bloco, escreve o header e incrementa indentação.</summary>
    public StructuredTextWriter OpenBlock(string header)
    {
        Line(header);
        Indent++;
        return this;
    }

    /// <summary>Fecha um bloco, decrementa indentação e escreve o footer.</summary>
    public StructuredTextWriter CloseBlock(string footer)
    {
        Indent--;
        Line(footer);
        return this;
    }

    /// <summary>Escreve uma linha (indentada automaticamente).</summary>
    public StructuredTextWriter Line(string text = "")
    {
        if (text.Length == 0)
        {
            _sb.AppendLine();
            _lineStartPending = true;
            return this;
        }
        EnsureIndent();
        _sb.AppendLine(text);
        _lineStartPending = true;
        return this;
    }

    /// <summary>Escreve texto sem quebrar linha (continuação).</summary>
    public StructuredTextWriter Write(string text)
    {
        EnsureIndent();
        _sb.Append(text);
        return this;
    }

    /// <summary>Escreve comentário IEC <c>(* ... *)</c>.</summary>
    public StructuredTextWriter Comment(string text)
    {
        // IEC 61131-3 aceita (* ... *) e // (em variantes modernas). Usamos (* *) por compatibilidade total.
        foreach (var rawLine in text.Replace("\r", "").Split('\n'))
        {
            Line($"(* {rawLine} *)");
        }
        return this;
    }

    /// <summary>Bloco de comentário multi-linha.</summary>
    public StructuredTextWriter CommentBlock(string text)
    {
        Line("(*");
        foreach (var rawLine in text.Replace("\r", "").Split('\n'))
            Line($"   {rawLine}");
        Line("*)");
        return this;
    }

    /// <summary>Linha em branco.</summary>
    public StructuredTextWriter Blank() => Line();

    /// <summary>Formata um double em literal ST (vírgula como ponto decimal).</summary>
    public static string F(double v)
    {
        if (double.IsNaN(v)) return "0.0";
        if (double.IsPositiveInfinity(v)) return "1.0E30";
        if (double.IsNegativeInfinity(v)) return "-1.0E30";
        // IEC ST usa ponto decimal. Garantir que sempre tenha casa decimal (ex.: 5 -> 5.0).
        var s = v.ToString("G17", System.Globalization.CultureInfo.InvariantCulture);
        if (!s.Contains('.') && !s.Contains('E') && !s.Contains('e'))
            s += ".0";
        return s;
    }

    /// <summary>Formata um bool como TRUE/FALSE.</summary>
    public static string B(bool v) => v ? "TRUE" : "FALSE";

    /// <summary>Formata um TimeSpan como literal ST (T#50ms, T#1s, T#100ms).</summary>
    public static string T(TimeSpan ts)
    {
        var totalMs = ts.TotalMilliseconds;
        if (totalMs < 1000)
            return $"T#{(int)totalMs}ms";
        var totalSec = ts.TotalSeconds;
        if (totalSec < 60)
            return totalSec == Math.Floor(totalSec)
                ? $"T#{(int)totalSec}s"
                : $"T#{(int)totalSec}s{(int)((totalSec - Math.Floor(totalSec)) * 1000)}ms";
        var totalMin = ts.TotalMinutes;
        return $"T#{(int)totalMin}m{(int)(ts.Seconds)}s";
    }

    /// <summary>Sanitiza um identificador para IEC 61131-3 (letras, dígitos, underscore).</summary>
    public static string Id(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
            sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
        if (sb.Length == 0) return "_";
        if (char.IsDigit(sb[0])) sb.Insert(0, '_');
        // IEC keywords reservadas — prefixar com underscore
        var reserved = new HashSet<string> { "VAR", "END_VAR", "PROGRAM", "FUNCTION_BLOCK", "END_FUNCTION_BLOCK",
            "IF", "THEN", "ELSE", "ELSIF", "END_IF", "FOR", "TO", "BY", "DO", "END_FOR",
            "WHILE", "END_WHILE", "REPEAT", "UNTIL", "TRUE", "FALSE", "AND", "OR", "NOT", "XOR",
            "MOD", "RETURN", "EXIT", "CONTINUE", "CASE", "OF", "END_CASE" };
        var s = sb.ToString();
        return reserved.Contains(s) ? "_" + s : s;
    }

    /// <summary>Traduz a unidade de engenharia para um comentário ST.</summary>
    public static string UnitComment(string symbol) => string.IsNullOrEmpty(symbol) ? "" : $" (* unit: {symbol} *)";

    /// <summary>Texto acumulado.</summary>
    public string Build() => _sb.ToString();

    private void EnsureIndent()
    {
        if (_lineStartPending)
        {
            for (int i = 0; i < _indent; i++) _sb.Append(_indentUnit);
            _lineStartPending = false;
        }
    }
}
