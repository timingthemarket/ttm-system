using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using portfolio.DataAccess.Extensions;
using portfolio.DataAccess.Interfaces;
using portfolio.Domain.Constants;

namespace portfolio.Domain.Handlers;

public class SessionDateHandler(ILogger<SessionDateHandler> logger, ISimulationRepository simulationRepository)
{
    public async Task ToggleSessionDate()
    {
        const int maxRetries = 3;
        const int delaySeconds = 5;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await ToggleSessionDateCore();
                return; // Success, exit the retry loop
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while toggling session date (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                
                if (attempt == maxRetries)
                {
                    logger.LogError("Failed to toggle session date after {MaxRetries} attempts", maxRetries);
                    throw; // Re-throw the exception after all retries are exhausted
                }
                
                logger.LogInformation("Retrying in {DelaySeconds} seconds...", delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }

    private async Task ToggleSessionDateCore()
    {
        var sessionDate = DateOnly.FromDateTime(DateTime.Today);

        var cacheExpiration = TimeSpan.FromDays(7);

        var latestSession = await simulationRepository.GetLatestSession();
        if (latestSession != null && latestSession.SessionDate != sessionDate)
        {
            await simulationRepository.SaveSession(sessionDate);
            logger.LogInformation("Session date set to {Date} and timespan set to {Ts}", sessionDate,
                cacheExpiration);
        }
    }
    
    public static TimeSpan GetExpirationTimeSpanNextMonday()
    {
        var dateTimeNow = DateTime.UtcNow;
        
        var dateToday = DateOnly.FromDateTime(dateTimeNow.Date);

        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)dateToday.DayOfWeek + 7) % 7;

        var mondayDate = dateToday.AddDays(daysUntilMonday);
        // 05:00 in the morning
        var mondayDateAndTime = mondayDate.ToDateTime(new TimeOnly(5, 0, 0), DateTimeKind.Utc);
        
        var timeSpan = mondayDateAndTime - dateTimeNow;
        return timeSpan;
    }
}