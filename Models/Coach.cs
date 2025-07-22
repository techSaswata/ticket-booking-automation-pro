using System.Collections.Generic;

namespace TrainBookingAutomation.Models
{
    public class Coach
    {
        public string CoachNumber { get; set; }
        public CoachType Type { get; set; }
        public SeatClass Class { get; set; }
        public List<Seat> Seats { get; set; } = new();
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public bool HasAC { get; set; }
        public bool HasToilet { get; set; }
        public bool HasPantry { get; set; }
        public int Position { get; set; } // Position in train
    }

    public class Seat
    {
        public string SeatNumber { get; set; }
        public SeatType Type { get; set; }
        public SeatStatus Status { get; set; }
        public decimal Price { get; set; }
        public string PassengerName { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsWindow { get; set; }
        public bool IsAisle { get; set; }
        public SeatPreference Preference { get; set; }
    }

    public class Station
    {
        public string StationCode { get; set; }
        public string StationName { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int PlatformNumber { get; set; }
        public bool HasWaitingRoom { get; set; }
        public bool HasParking { get; set; }
        public bool HasFood { get; set; }
        public StationType Type { get; set; }
    }

    public enum CoachType
    {
        Passenger,
        Sleeper,
        AC,
        Pantry,
        Luggage,
        Generator
    }

    public enum SeatType
    {
        Sitting,
        LowerBerth,
        MiddleBerth,
        UpperBerth,
        SideLower,
        SideUpper
    }

    public enum SeatStatus
    {
        Available,
        Booked,
        Blocked,
        Maintenance,
        Reserved
    }

    public enum StationType
    {
        Major,
        Junction,
        Terminal,
        Halt,
        Metro
    }
} 