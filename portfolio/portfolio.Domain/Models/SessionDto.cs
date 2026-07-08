namespace portfolio.Domain.Models;

public class SessionDto
{
    public int Id { get; set; }
    public DateOnly SessionDate { get; set; }
    public int SimulationCount { get; set; }
}