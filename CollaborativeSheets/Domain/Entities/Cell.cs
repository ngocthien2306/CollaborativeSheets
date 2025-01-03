namespace CollaborativeSheets.Domain.Entities
{
    public record Cell(string Expression, double Value)
    {
        public override string ToString() => Value.ToString();
    }
}
