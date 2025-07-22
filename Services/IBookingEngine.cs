using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public interface IBookingEngine
    {
        Task<BookingResult> BookTicketAsync(BookingRequest request);
        Task<BookingResult> AutomatedBookingAsync(BookingRequest request);
        Task<List<BookingResult>> BulkBookingAsync(List<BookingRequest> requests);
        Task<BookingResult> RetryBookingAsync(string bookingId);
        Task<bool> CancelBookingAsync(string pnr);
        Task<BookingResult> ModifyBookingAsync(string pnr, BookingRequest newRequest);
        Task<List<BookingResult>> GetBookingHistoryAsync(string userId);
        Task<BookingResult> GetBookingDetailsAsync(string pnr);
        Task<BookingAnalytics> GetBookingAnalyticsAsync(DateTime fromDate, DateTime toDate);
        Task<bool> ProcessWaitlistAsync(string pnr);
        Task<List<SeatAllocation>> OptimizeSeatAllocationAsync(BookingRequest request, Train train);
        Task<bool> MonitorBookingStatusAsync(string bookingId);
        Task StartAutomationAsync(BookingRequest request);
        Task StopAutomationAsync(string bookingId);
        Task<List<BookingRequest>> GetActiveAutomationsAsync();
        event EventHandler<BookingResult> BookingCompleted;
        event EventHandler<BookingResult> BookingFailed;
        event EventHandler<string> BookingStatusChanged;
    }
} 