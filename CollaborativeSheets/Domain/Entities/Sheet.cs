namespace CollaborativeSheets.Domain.Entities
{
    public record Sheet(string Name, string Owner, Dictionary<(int, int), Cell> Cells);
}
