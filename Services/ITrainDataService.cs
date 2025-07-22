using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public interface ITrainDataService
    {
        Task<List<Train>> SearchTrainsAsync(string source, string destination, DateTime travelDate);
        Task<Train> GetTrainDetailsAsync(string trainNumber);
        Task<List<Station>> GetAllStationsAsync();
        Task<List<Station>> SearchStationsAsync(string query);
        Task<Dictionary<SeatClass, int>> GetAvailabilityAsync(string trainNumber, DateTime travelDate);
        Task<Dictionary<SeatClass, decimal>> GetPricingAsync(string trainNumber, string source, string destination);
        Task<List<Train>> GetRecommendedTrainsAsync(BookingRequest request);
        Task<TrainStatus> GetTrainStatusAsync(string trainNumber);
        Task<List<Coach>> GetCoachLayoutAsync(string trainNumber);
        Task<bool> CheckSeatAvailabilityAsync(string trainNumber, string coachNumber, string seatNumber);
        Task RefreshDataAsync();
    }
} 