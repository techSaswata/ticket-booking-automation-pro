using System;
using System.Collections.Generic;

namespace TrainBookingAutomation.Models
{
    public class BookingResult
    {
        public string BookingId { get; set; } = Guid.NewGuid().ToString();
        public string PNR { get; set; }
        public BookingRequest Request { get; set; }
        public Train SelectedTrain { get; set; }
        public List<SeatAllocation> SeatAllocations { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ConvenienceFee { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime BookedAt { get; set; } = DateTime.Now;
        public string PaymentId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public List<string> Messages { get; set; } = new();
        public BookingChannel Channel { get; set; }
        public int AttemptNumber { get; set; }
        public TimeSpan BookingDuration { get; set; }
        public string QRCode { get; set; }
        public string TicketPath { get; set; }
    }

    public class SeatAllocation
    {
        public Passenger Passenger { get; set; }
        public Coach Coach { get; set; }
        public Seat Seat { get; set; }
        public decimal Fare { get; set; }
        public bool IsConfirmed { get; set; }
        public string WaitlistPosition { get; set; }
    }

    public class BookingAnalytics
    {
        public int TotalBookings { get; set; }
        public int SuccessfulBookings { get; set; }
        public int FailedBookings { get; set; }
        public int WaitlistedBookings { get; set; }
        public decimal SuccessRate { get; set; }
        public TimeSpan AverageBookingTime { get; set; }
        public decimal TotalAmountBooked { get; set; }
        public Dictionary<string, int> PopularRoutes { get; set; } = new();
        public Dictionary<SeatClass, int> ClassDistribution { get; set; } = new();
        public Dictionary<TrainType, int> TrainTypePreference { get; set; } = new();
        public List<BookingTrend> MonthlyTrends { get; set; } = new();
        public List<PriceAnalysis> PriceAnalytics { get; set; } = new();
    }

    public class BookingTrend
    {
        public DateTime Month { get; set; }
        public int TotalBookings { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal SuccessRate { get; set; }
    }

    public class PriceAnalysis
    {
        public string Route { get; set; }
        public SeatClass Class { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<PriceHistory> History { get; set; } = new();
    }

    public class PriceHistory
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
    }

    public class AIRecommendation
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Confidence { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public RecommendationCategory Category { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Refunded,
        Cancelled
    }

    public enum BookingChannel
    {
        Desktop,
        Mobile,
        API,
        Automation
    }

    public enum RecommendationCategory
    {
        RouteOptimization,
        PriceOptimization,
        SeatSelection,
        TimeOptimization,
        ClassUpgrade,
        AlternativeTrains
    }
} 