namespace CollaborativeSheets.Domain.ValueObjects
{
    public class Option<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        private Option(T value, bool hasValue)
        {
            _value = value;
            _hasValue = hasValue;
        }

        public static Option<T> Some(T value) => new(value, true);
        public static Option<T> None() => new(default!, false);

        public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) =>
            _hasValue ? some(_value) : none();
    }

    public static class Option
    {
        public static Option<T> Some<T>(T value) => Option<T>.Some(value);
        public static Option<T> None<T>() => Option<T>.None();
    }

}
