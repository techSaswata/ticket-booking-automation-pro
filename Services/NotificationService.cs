using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public class NotificationService : INotificationService
    {
        private readonly Dictionary<string, List<NotificationType>> _userPreferences = new();
        private readonly List<string> _notificationHistory = new();

        public NotificationService()
        {
            InitializeDefaultPreferences();
        }

        public async Task SendBookingConfirmationAsync(BookingResult booking)
        {
            await Task.Delay(100);
            
            var title = "🎉 Booking Confirmed!";
            var message = $"Your booking is confirmed!\n" +
                         $"PNR: {booking.PNR}\n" +
                         $"Train: {booking.SelectedTrain.TrainName}\n" +
                         $"Date: {booking.Request.TravelDate:dd/MM/yyyy}\n" +
                         $"Amount: ₹{booking.TotalAmount:N0}";

            await SendMultiChannelNotificationAsync(booking.Request.UserId, title, message, NotificationType.Desktop);
            
            // Also send email notification
            await SendEmailNotificationAsync(booking);
            
            // Add to history
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Booking confirmation sent for PNR {booking.PNR}");
        }

        public async Task SendBookingFailureNotificationAsync(BookingRequest request, string reason)
        {
            await Task.Delay(100);
            
            var title = "❌ Booking Failed";
            var message = $"Booking attempt failed for {request.Source} → {request.Destination}\n" +
                         $"Date: {request.TravelDate:dd/MM/yyyy}\n" +
                         $"Reason: {reason}\n" +
                         $"Auto-retry is {(request.AutoRetry ? "enabled" : "disabled")}";

            await SendMultiChannelNotificationAsync(request.UserId, title, message, NotificationType.Desktop);
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Booking failure notification sent: {reason}");
        }

        public async Task SendPriceAlertAsync(string userId, PriceAnalysis priceAnalysis)
        {
            await Task.Delay(100);
            
            var savings = priceAnalysis.AveragePrice - priceAnalysis.CurrentPrice;
            var title = "💰 Price Drop Alert!";
            var message = $"Great news! Price dropped for {priceAnalysis.Route}\n" +
                         $"Class: {priceAnalysis.Class}\n" +
                         $"New Price: ₹{priceAnalysis.CurrentPrice:N0}\n" +
                         $"You save: ₹{savings:N0} ({(savings/priceAnalysis.AveragePrice)*100:F1}%)\n" +
                         $"Book now to get this price!";

            await SendMultiChannelNotificationAsync(userId, title, message, NotificationType.Desktop);
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Price alert sent for {priceAnalysis.Route}");
        }

        public async Task SendWaitlistUpdateAsync(string pnr, string status)
        {
            await Task.Delay(100);
            
            var title = status == "Confirmed" ? "✅ Waitlist Confirmed!" : "📋 Waitlist Update";
            var message = $"Waitlist status update for PNR {pnr}\n" +
                         $"Status: {status}\n" +
                         $"Time: {DateTime.Now:dd/MM/yyyy HH:mm}";

            if (status == "Confirmed")
            {
                message += "\n🎉 Congratulations! Your ticket is now confirmed!";
            }

            await SendDesktopNotificationAsync(title, message, NotificationType.Desktop);
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Waitlist update sent for PNR {pnr}: {status}");
        }

        public async Task SendTrainDelayNotificationAsync(string trainNumber, int delayMinutes)
        {
            await Task.Delay(100);
            
            var title = "⏰ Train Delay Update";
            var message = $"Train {trainNumber} is delayed by {delayMinutes} minutes\n" +
                         $"Updated departure time will be announced\n" +
                         $"Please plan accordingly";

            await SendDesktopNotificationAsync(title, message, NotificationType.Desktop);
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Delay notification sent for {trainNumber}: {delayMinutes} mins");
        }

        public async Task SendDesktopNotificationAsync(string title, string message, NotificationType type)
        {
            await Task.Delay(50);
            
            // For WPF, we'll show a message box or use toast notifications
            Application.Current?.Dispatcher.Invoke(() =>
            {
                var icon = type switch
                {
                    NotificationType.Desktop => MessageBoxImage.Information,
                    _ => MessageBoxImage.Information
                };

                // In a real application, you might use a toast notification library
                // For demo purposes, we'll use MessageBox
                var result = MessageBox.Show(message, title, MessageBoxButton.OK, icon);
            });
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Desktop notification: {title}");
        }

        public async Task SendBulkNotificationAsync(List<string> userIds, string message, NotificationType type)
        {
            await Task.Delay(200);
            
            var tasks = userIds.Select(async userId =>
            {
                await SendCustomNotificationAsync(userId, "Bulk Notification", message, type);
            });

            await Task.WhenAll(tasks);
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Bulk notification sent to {userIds.Count} users");
        }

        public async Task<bool> ConfigureNotificationPreferencesAsync(string userId, List<NotificationType> preferences)
        {
            await Task.Delay(50);
            
            _userPreferences[userId] = preferences;
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Preferences updated for user {userId}");
            return true;
        }

        public async Task<List<NotificationType>> GetNotificationPreferencesAsync(string userId)
        {
            await Task.Delay(50);
            
            return _userPreferences.GetValueOrDefault(userId, 
                new List<NotificationType> { NotificationType.Desktop, NotificationType.Email });
        }

        public async Task ScheduleNotificationAsync(string message, DateTime scheduledTime, NotificationType type)
        {
            await Task.Delay(50);
            
            // In a real application, you would use a job scheduler
            var delay = scheduledTime - DateTime.Now;
            
            if (delay > TimeSpan.Zero)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(delay);
                    await SendDesktopNotificationAsync("Scheduled Notification", message, type);
                });
            }
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Notification scheduled for {scheduledTime:dd/MM/yyyy HH:mm}");
        }

        public async Task<List<string>> GetNotificationHistoryAsync(string userId)
        {
            await Task.Delay(50);
            
            // Return last 50 notifications for demo
            return _notificationHistory.TakeLast(50).ToList();
        }

        public async Task MarkNotificationAsReadAsync(string notificationId)
        {
            await Task.Delay(25);
            
            // In a real system, you would update notification status in database
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Notification {notificationId} marked as read");
        }

        public async Task SendCustomNotificationAsync(string userId, string title, string message, NotificationType type)
        {
            await Task.Delay(50);
            
            var preferences = await GetNotificationPreferencesAsync(userId);
            
            if (preferences.Contains(type))
            {
                switch (type)
                {
                    case NotificationType.Desktop:
                        await SendDesktopNotificationAsync(title, message, type);
                        break;
                        
                    case NotificationType.Email:
                        await SendEmailNotificationInternalAsync(userId, title, message);
                        break;
                        
                    case NotificationType.SMS:
                        await SendSMSNotificationAsync(userId, message);
                        break;
                        
                    case NotificationType.PushNotification:
                        await SendPushNotificationAsync(userId, title, message);
                        break;
                }
            }
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Custom notification sent to {userId}: {title}");
        }

        private async Task SendMultiChannelNotificationAsync(string userId, string title, string message, NotificationType primaryType)
        {
            var preferences = await GetNotificationPreferencesAsync(userId);
            
            var tasks = preferences.Select(async type =>
            {
                await SendCustomNotificationAsync(userId, title, message, type);
            });

            await Task.WhenAll(tasks);
        }

        private async Task SendEmailNotificationAsync(BookingResult booking)
        {
            await Task.Delay(100);
            
            // Simulate email sending
            var emailContent = GenerateBookingEmailContent(booking);
            
            // In a real application, you would use an email service like SendGrid, SMTP, etc.
            await Task.Delay(500); // Simulate email sending delay
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Email sent for booking {booking.PNR}");
        }

        private async Task SendEmailNotificationInternalAsync(string userId, string title, string message)
        {
            await Task.Delay(200);
            
            // Simulate email sending
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Email sent to {userId}: {title}");
        }

        private async Task SendSMSNotificationAsync(string userId, string message)
        {
            await Task.Delay(150);
            
            // Simulate SMS sending via SMS gateway
            var truncatedMessage = message.Length > 160 ? message[..157] + "..." : message;
            
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] SMS sent to {userId}: {truncatedMessage}");
        }

        private async Task SendPushNotificationAsync(string userId, string title, string message)
        {
            await Task.Delay(100);
            
            // Simulate push notification via FCM, APNS, etc.
            _notificationHistory.Add($"[{DateTime.Now:HH:mm:ss}] Push notification sent to {userId}: {title}");
        }

        private string GenerateBookingEmailContent(BookingResult booking)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #FF6B35; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .ticket {{ background-color: white; padding: 15px; margin: 10px 0; border-left: 4px solid #FF6B35; }}
        .footer {{ text-align: center; color: #888; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🚄 Booking Confirmation</h1>
        <h2>PNR: {booking.PNR}</h2>
    </div>
    
    <div class='content'>
        <div class='ticket'>
            <h3>Journey Details</h3>
            <p><strong>Train:</strong> {booking.SelectedTrain.TrainNumber} - {booking.SelectedTrain.TrainName}</p>
            <p><strong>Route:</strong> {booking.SelectedTrain.Source} → {booking.SelectedTrain.Destination}</p>
            <p><strong>Date:</strong> {booking.Request.TravelDate:dd/MM/yyyy}</p>
            <p><strong>Departure:</strong> {booking.SelectedTrain.DepartureTime:HH:mm}</p>
            <p><strong>Arrival:</strong> {booking.SelectedTrain.ArrivalTime:HH:mm}</p>
        </div>
        
        <div class='ticket'>
            <h3>Passenger Details</h3>
            {string.Join("", booking.SeatAllocations.Select((a, i) => 
                $"<p><strong>Passenger {i + 1}:</strong> {a.Passenger.Name} - {a.Coach.CoachNumber}/{a.Seat.SeatNumber}</p>"))}
        </div>
        
        <div class='ticket'>
            <h3>Payment Summary</h3>
            <p><strong>Base Fare:</strong> ₹{booking.TotalAmount:N0}</p>
            <p><strong>Taxes:</strong> ₹{booking.TaxAmount:N0}</p>
            <p><strong>Convenience Fee:</strong> ₹{booking.ConvenienceFee:N0}</p>
            <p><strong>Total Amount:</strong> ₹{(booking.TotalAmount + booking.TaxAmount + booking.ConvenienceFee):N0}</p>
        </div>
    </div>
    
    <div class='footer'>
        <p>Thank you for using TrainBooking Pro!</p>
        <p>For support, contact us at support@trainbookingpro.com</p>
    </div>
</body>
</html>";
        }

        private void InitializeDefaultPreferences()
        {
            // Set default preferences for demo user
            _userPreferences["default_user"] = new List<NotificationType>
            {
                NotificationType.Desktop,
                NotificationType.Email
            };

            // Add some initial history
            _notificationHistory.Add($"[{DateTime.Now.AddMinutes(-30):HH:mm:ss}] Service initialized");
            _notificationHistory.Add($"[{DateTime.Now.AddMinutes(-25):HH:mm:ss}] Default preferences configured");
            _notificationHistory.Add($"[{DateTime.Now.AddMinutes(-20):HH:mm:ss}] Notification channels ready");
        }
    }
} 