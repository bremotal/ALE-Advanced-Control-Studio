using ALE.ACS.Engine.Fuzzy.Rules;

namespace ALE.ACS.Engine.Fuzzy.Expressions;

/// <summary>
/// Parses a fuzzy rule expression into an AST.
/// </summary>
public sealed class FuzzyRuleParser : IFuzzyRuleParser
{
    /// <inheritdoc />
    public FuzzyExpressionNode Parse(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        var tokens = Tokenize(expression);
        var index = 0;

        var node = ParseOrExpression(tokens, ref index);

        if (index != tokens.Count)
        {
            throw new ValidationException($"Unexpected token '{tokens[index]}' at position {index}.");
        }

        return node;
    }

    private static List<string> Tokenize(string expression)
    {
        var tokens = new List<string>();
        var current = string.Empty;

        foreach (var ch in expression)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current);
                    current = string.Empty;
                }

                continue;
            }

            if (ch is '(' or ')')
            {
                if (current.Length > 0)
                {
                    tokens.Add(current);
                    current = string.Empty;
                }

                tokens.Add(ch.ToString());
                continue;
            }

            current += ch;
        }

        if (current.Length > 0)
        {
            tokens.Add(current);
        }

        return tokens;
    }

    private static FuzzyExpressionNode ParseOrExpression(IReadOnlyList<string> tokens, ref int index)
    {
        var node = ParseAndExpression(tokens, ref index);

        while (index < tokens.Count && string.Equals(tokens[index], "OR", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            var right = ParseAndExpression(tokens, ref index);
            node = new OperatorNode("OR", [node, right]);
        }

        return node;
    }

    private static FuzzyExpressionNode ParseAndExpression(IReadOnlyList<string> tokens, ref int index)
    {
        var node = ParseUnaryExpression(tokens, ref index);

        while (index < tokens.Count && string.Equals(tokens[index], "AND", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            var right = ParseUnaryExpression(tokens, ref index);
            node = new OperatorNode("AND", [node, right]);
        }

        return node;
    }

    private static FuzzyExpressionNode ParseUnaryExpression(IReadOnlyList<string> tokens, ref int index)
    {
        if (index < tokens.Count && string.Equals(tokens[index], "NOT", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            var child = ParseUnaryExpression(tokens, ref index);
            return new OperatorNode("NOT", [child]);
        }

        return ParsePrimaryExpression(tokens, ref index);
    }

    private static FuzzyExpressionNode ParsePrimaryExpression(IReadOnlyList<string> tokens, ref int index)
    {
        if (index >= tokens.Count)
        {
            throw new ValidationException("Unexpected end of expression.");
        }

        var token = tokens[index];

        if (token == "(")
        {
            index++;
            var node = ParseOrExpression(tokens, ref index);
            if (index >= tokens.Count || tokens[index] != ")")
            {
                throw new ValidationException("Missing closing parenthesis.");
            }

            index++;
            return node;
        }

        if (token == ")")
        {
            throw new ValidationException("Unexpected closing parenthesis.");
        }

        if (token.Equals("IS", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Invalid clause syntax.");
        }

        if (index + 2 < tokens.Count && string.Equals(tokens[index + 1], "IS", StringComparison.OrdinalIgnoreCase))
        {
            var variableName = tokens[index];
            var termName = tokens[index + 2];
            index += 3;
            return new ClauseNode(variableName, termName);
        }

        throw new ValidationException($"Unsupported token '{token}'.");
    }
}

/// <summary>
/// Defines the contract for parsing rule expressions into AST nodes.
/// </summary>
public interface IFuzzyRuleParser
{
    /// <summary>
    /// Parses an expression into a fuzzy expression AST.
    /// </summary>
    /// <param name="expression">The expression to parse.</param>
    /// <returns>The root AST node.</returns>
    FuzzyExpressionNode Parse(string expression);
}

/// <summary>
/// Represents a validation failure in the fuzzy rule engine.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValidationException(string message) : base(message)
    {
    }
}
