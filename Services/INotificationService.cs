using System.Collections.Generic;
using System.Threading.Tasks;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public interface INotificationService
    {
        Task SendBookingConfirmationAsync(BookingResult booking);
        Task SendBookingFailureNotificationAsync(BookingRequest request, string reason);
        Task SendPriceAlertAsync(string userId, PriceAnalysis priceAnalysis);
        Task SendWaitlistUpdateAsync(string pnr, string status);
        Task SendTrainDelayNotificationAsync(string trainNumber, int delayMinutes);
        Task SendDesktopNotificationAsync(string title, string message, NotificationType type);
        Task SendBulkNotificationAsync(List<string> userIds, string message, NotificationType type);
        Task<bool> ConfigureNotificationPreferencesAsync(string userId, List<NotificationType> preferences);
        Task<List<NotificationType>> GetNotificationPreferencesAsync(string userId);
        Task ScheduleNotificationAsync(string message, System.DateTime scheduledTime, NotificationType type);
        Task<List<string>> GetNotificationHistoryAsync(string userId);
        Task MarkNotificationAsReadAsync(string notificationId);
        Task SendCustomNotificationAsync(string userId, string title, string message, NotificationType type);
    }
} 