using Microsoft.Extensions.Logging;
using portfolio.DataAccess.Interfaces;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;
using portfolio.Domain.Queue;
using portfolio.Domain.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using portfolio.Domain.Constants;
using portfolio.Domain.Services;
using TTM.Shared.Extensions;

namespace portfolio.Domain.Handlers;

public class HistoricalExplorerHandler(
    ILogger<HistoricalExplorerHandler> logger,
    ISimulationRepository simulationRepository,
    IPortfolioExplorerHandler portfolioExplorerHandler,
    HistoricalExplorerQueueCache queueCache,
    IPublishEndpoint endpoint)
    : IHistoricalExplorerHandler
{

    public async Task<bool> ProcessHistoricalExplorerFromQueue(CancellationToken cancellationToken = default)
    {
        var request = queueCache.DequeueAndGetItem();
        if (request == null)
        {
            return false; // No items to process
        }

        var requestKey = queueCache.SetCurrentlyRunning(request);
        
        logger.LogInformation("Processing historical exploration for session date {SessionDate} with {NrIterations} iterations", 
            request.SessionDate, request.NrIterations);
        
        try
        {
            // Step 1: Check if session already exists, if not create it
            var session = await simulationRepository.GetSessionByDate(request.SessionDate);
            
            if (session == null)
            {
                logger.LogInformation("Creating new session for date {SessionDate}", request.SessionDate);
                session = await simulationRepository.SaveSession(request.SessionDate);
                
                if (session == null)
                {
                    logger.LogError("Failed to create session for date {SessionDate}", request.SessionDate);
                    throw new InvalidOperationException($"Could not create session for date {request.SessionDate}");
                }
            }
            else
            {
                logger.LogInformation("Using existing session ID {SessionId} for date {SessionDate}", session.Id, request.SessionDate);
            }
            
            logger.LogInformation("Using session ID {SessionId} for date {SessionDate}", session.Id, request.SessionDate);
            
            // Step 2: Generate all indicator combinations
            var allCombinations = IndicatorCombinationGenerator.GenerateAllIndicatorCombinations();
            logger.LogInformation("Generated {CombinationCount} indicator combinations", allCombinations.Count);
            
            // Step 3: Process each combination through PortfolioExplorerService
            var processedCount = 0;
            var totalCombinations = Math.Min(allCombinations.Count, request.NrIterations);

            var sessionHashesList =
                await simulationRepository.GetPortfolioHashesFromSessionDate(session.SessionDate);
            var sessionHashes = new HashSet<string>(sessionHashesList);

            var index = 0;
            while (index < allCombinations.Count) // Loop through all combos
            {
                if (processedCount >= totalCombinations)
                {
                    logger.LogInformation("Reached maximum iterations ({MaxIterations}). Stopping processing.",
                        request.NrIterations);
                    break; // Stop if we reached the max iterations
                }
                
                try
                {
                    List<PortfolioInputIndicatorVariable> indicatorCombination;
                    if (request.ProcessDirection == IndicatorSearchSpace.Random)
                    {
                        indicatorCombination = IndicatorCombinationGenerator.GenerateIndicators();
                    }
                    else if (request.ProcessDirection == IndicatorSearchSpace.Start)
                    {
                        indicatorCombination = allCombinations[index];
                    }
                    else if (request.ProcessDirection == IndicatorSearchSpace.End)
                    {
                        var indexBackwards = index + 1;
                        indicatorCombination = allCombinations[^indexBackwards];
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid search space: {request.ProcessDirection}");
                    }
                    
                    var initMoney = 50_000;

                    var hasProcessed = await portfolioExplorerHandler.HandlePortfolioDiscover(session.Id,
                        request.SessionDate, indicatorCombination, sessionHashes, initMoney, cancellationToken);
                    index++;
                    if (!hasProcessed) continue;

                    processedCount++;

                    await endpoint.Increment(Metrics.HistoricalPorfolioComputed);

                    if (processedCount % 1000 == 0)
                    {
                        logger.LogInformation(
                            "Processed {ProcessedCount}/{TotalCombinations} combinations for session {SessionDate}",
                            processedCount, totalCombinations, request.SessionDate);
                    }

                    if (processedCount % 10 == 0)
                    {
                        queueCache.UpdateCurrentlyRunning(requestKey, processedCount);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Error processing indicator combination {CombinationIndex} for session {SessionDate}",
                        processedCount + 1, request.SessionDate);
                    throw;
                }
            }
            
            logger.LogInformation("Completed historical exploration for session date {SessionDate}. Processed {ProcessedCount}/{TotalCombinations} combinations", 
                request.SessionDate, processedCount, totalCombinations);

            return true; // Successfully processed an item
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process historical exploration for session date {SessionDate}", request.SessionDate);
            throw;
        }
        finally
        {
            queueCache.ClearCurrentlyRunning(requestKey);
        }
    }
}