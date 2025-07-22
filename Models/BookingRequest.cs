using System;
using System.Collections.Generic;

namespace TrainBookingAutomation.Models
{
    public class BookingRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Source { get; set; }
        public string Destination { get; set; }
        public DateTime TravelDate { get; set; }
        public List<Passenger> Passengers { get; set; } = new();
        public SeatClass PreferredClass { get; set; }
        public List<string> PreferredTrains { get; set; } = new();
        public BookingPriority Priority { get; set; }
        public AutomationSettings AutomationSettings { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string UserId { get; set; }
        public bool FlexibleDate { get; set; }
        public int FlexibilityDays { get; set; }
        public decimal MaxPrice { get; set; }
        public bool AutoRetry { get; set; } = true;
        public int MaxRetries { get; set; } = 5;
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMinutes(5);
    }

    public class Passenger
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public string IdType { get; set; }
        public string IdNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsChild { get; set; }
        public bool IsSeniorCitizen { get; set; }
        public FoodPreference FoodPreference { get; set; }
        public SeatPreference SeatPreference { get; set; }
    }

    public class AutomationSettings
    {
        public bool EnablePriceTracking { get; set; } = true;
        public bool EnableAutoBooking { get; set; } = false;
        public bool EnableWaitlistMonitoring { get; set; } = true;
        public bool EnableSeatUpgrade { get; set; } = false;
        public bool EnableNotifications { get; set; } = true;
        public bool EnableMultipleAttempts { get; set; } = true;
        public TimeSpan BookingWindow { get; set; } = TimeSpan.FromHours(2);
        public List<NotificationType> NotificationTypes { get; set; } = new();
    }

    public enum BookingPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public enum BookingStatus
    {
        Pending,
        InProgress,
        Confirmed,
        Failed,
        Cancelled,
        Waitlisted,
        Refunded
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public enum FoodPreference
    {
        Vegetarian,
        NonVegetarian,
        Jain,
        Vegan,
        None
    }

    public enum SeatPreference
    {
        Window,
        Aisle,
        Middle,
        Upper,
        Lower,
        NoPreference
    }

    public enum NotificationType
    {
        Email,
        SMS,
        PushNotification,
        Desktop
    }
} 