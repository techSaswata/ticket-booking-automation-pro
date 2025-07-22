using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public interface IPriceOptimizationService
    {
        Task<List<PriceAnalysis>> GetPriceAnalysisAsync(string route, SeatClass seatClass);
        Task<decimal> PredictPriceAsync(string route, DateTime travelDate, SeatClass seatClass);
        Task<List<DateTime>> GetBestBookingDatesAsync(string route, int flexibilityDays);
        Task<bool> ShouldWaitForBetterPriceAsync(BookingRequest request);
        Task<decimal> GetPriceTrendAsync(string route, SeatClass seatClass, TimeSpan period);
        Task<List<PriceHistory>> GetPriceHistoryAsync(string route, SeatClass seatClass, DateTime fromDate);
        Task UpdatePriceHistoryAsync(string route, SeatClass seatClass, decimal price, int availableSeats);
        Task<List<string>> GetCheaperAlternativeRoutesAsync(string source, string destination);
        Task<Dictionary<SeatClass, decimal>> ComparePricesAcrossClassesAsync(string route, DateTime travelDate);
        Task<bool> SetPriceAlertAsync(string route, SeatClass seatClass, decimal targetPrice, string userId);
        Task<List<string>> GetActivePriceAlertsAsync(string userId);
        Task ProcessPriceAlertsAsync();
        event EventHandler<PriceAnalysis> PriceDropDetected;
    }
} 