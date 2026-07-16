namespace portfolio.DataAccess.Models.Views;

public class SessionCountView
{
    public int Id { get; set; }
    public DateOnly SessionDate { get; set; }
    public int SimulationCount { get; set; }
}