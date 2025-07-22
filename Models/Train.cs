using System;
using System.Collections.Generic;

namespace TrainBookingAutomation.Models
{
    public class Train
    {
        public string TrainNumber { get; set; }
        public string TrainName { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public TimeSpan Duration { get; set; }
        public TrainType Type { get; set; }
        public List<Coach> Coaches { get; set; } = new();
        public Dictionary<SeatClass, decimal> Prices { get; set; } = new();
        public Dictionary<SeatClass, int> AvailableSeats { get; set; } = new();
        public string Platform { get; set; }
        public TrainStatus Status { get; set; }
        public List<Station> IntermediateStations { get; set; } = new();
        public bool HasFood { get; set; }
        public bool HasWiFi { get; set; }
        public bool HasAC { get; set; }
        public double Rating { get; set; }
        public int DelayMinutes { get; set; }
        public string ImageUrl { get; set; }
    }

    public enum TrainType
    {
        Express,
        SuperFast,
        Local,
        Metro,
        HighSpeed,
        Luxury
    }

    public enum TrainStatus
    {
        OnTime,
        Delayed,
        Cancelled,
        Departed,
        Arrived
    }

    public enum SeatClass
    {
        GeneralSeating,
        SecondSitting,
        SleeperClass,
        ThirdAC,
        SecondAC,
        FirstAC,
        ExecutiveChair,
        Business
    }
} 