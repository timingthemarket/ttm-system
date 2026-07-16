using System;
using portfolio.Domain.Handlers;
using Xunit;

namespace portfolio.Tests;

public class SessionDateHandlerTests
{
    [Fact]
    public void ShouldGetExactTimespan()
    {
        var ts = SessionDateHandler.GetExpirationTimeSpanNextMonday();
        
        var dateTimeNow = DateTime.UtcNow;
        var resultingDate = dateTimeNow.Add(ts);
        
        Assert.True(resultingDate.Day >= dateTimeNow.Day);
        Assert.Equal(DayOfWeek.Monday, resultingDate.DayOfWeek);
        Assert.Equal(5, resultingDate.Hour);
        Assert.Equal(0, resultingDate.Minute);
    }
}