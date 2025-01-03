using System;
using System.Linq;

namespace CollaborativeSheets.Domain.Models
{
    public record Cell
    {
        public string Value { get; init; }

        private Cell(string value) => Value = value;

        public static Option<Cell> Create(string value) =>
            string.IsNullOrEmpty(value)
                ? Option<Cell>.None
                : Option<Cell>.Some(new Cell(value));

        public Option<decimal> Evaluate() =>
            TryEvaluateExpression()
                .Map(result => Math.Round(result, 4));

        private Option<decimal> TryEvaluateExpression()
        {
            try
            {
                var tokens = Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return tokens.Length == 1
                    ? ParseSingleValue(tokens[0])
                    : EvaluateOperation(tokens);
            }
            catch
            {
                return Option<decimal>.None;
            }
        }

        private static Option<decimal> ParseSingleValue(string token) =>
            decimal.TryParse(token, out var result)
                ? Option<decimal>.Some(result)
                : Option<decimal>.None;

        private static Option<decimal> EvaluateOperation(string[] tokens) =>
            tokens.Length == 3 &&
            decimal.TryParse(tokens[0], out var left) &&
            decimal.TryParse(tokens[2], out var right)
                ? CalculateResult(left, tokens[1], right)
                : Option<decimal>.None;

        private static Option<decimal> CalculateResult(decimal left, string op, decimal right) =>
            op switch
            {
                "+" => Option<decimal>.Some(left + right),
                "-" => Option<decimal>.Some(left - right),
                "*" => Option<decimal>.Some(left * right),
                "/" => right != 0 ? Option<decimal>.Some(left / right) : Option<decimal>.None,
                _ => Option<decimal>.None
            };
    }

    public class Option<T>
    {
        private readonly T value;
        private readonly bool hasValue;

        private Option(T value, bool hasValue)
        {
            this.value = value;
            this.hasValue = hasValue;
        }

        public static Option<T> Some(T value) => new Option<T>(value, true);
        public static Option<T> None => new Option<T>(default!, false);

        public Option<TResult> Map<TResult>(Func<T, TResult> mapper) =>
            hasValue ? Option<TResult>.Some(mapper(value)) : Option<TResult>.None;

        public T GetOrDefault(T defaultValue) => hasValue ? value : defaultValue;
    }
}