using System.Collections.Generic;
using System.Threading.Tasks;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public interface IAIRecommendationService
    {
        Task<List<AIRecommendation>> GetRouteRecommendationsAsync(BookingRequest request);
        Task<List<AIRecommendation>> GetSeatRecommendationsAsync(Train train, List<Passenger> passengers);
        Task<List<AIRecommendation>> GetPriceOptimizationAsync(string route, SeatClass seatClass);
        Task<List<AIRecommendation>> GetTimeOptimizationAsync(BookingRequest request);
        Task<List<Train>> GetAlternativeTrainsAsync(BookingRequest request);
        Task<AIRecommendation> GetBestBookingTimeAsync(string route);
        Task<List<AIRecommendation>> GetClassUpgradeRecommendationsAsync(BookingRequest request);
        Task<double> PredictBookingSuccessRateAsync(BookingRequest request);
        Task<List<AIRecommendation>> GetPersonalizedRecommendationsAsync(string userId);
        Task LearnFromBookingAsync(BookingResult result);
        Task<List<AIRecommendation>> GetSeasonalRecommendationsAsync(BookingRequest request);
    }
} 