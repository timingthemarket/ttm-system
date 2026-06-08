namespace TTM.Shared.Constants;

public static class SystemVariables
{
    public static string Version { get; set; } = DateTime.UtcNow.ToString("yyyyMMddThhmmss");
}