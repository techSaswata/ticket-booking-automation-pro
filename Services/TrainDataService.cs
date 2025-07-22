using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public class TrainDataService : ITrainDataService
    {
        private readonly List<Train> _trains;
        private readonly List<Station> _stations;

        public TrainDataService()
        {
            _stations = InitializeStations();
            _trains = InitializeTrains();
        }

        public async Task<List<Train>> SearchTrainsAsync(string source, string destination, DateTime travelDate)
        {
            await Task.Delay(500); // Simulate API call
            
            return _trains.Where(t => 
                t.Source.Equals(source, StringComparison.OrdinalIgnoreCase) &&
                t.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<Train> GetTrainDetailsAsync(string trainNumber)
        {
            await Task.Delay(200);
            return _trains.FirstOrDefault(t => t.TrainNumber == trainNumber);
        }

        public async Task<List<Station>> GetAllStationsAsync()
        {
            await Task.Delay(100);
            return _stations;
        }

        public async Task<List<Station>> SearchStationsAsync(string query)
        {
            await Task.Delay(150);
            return _stations.Where(s => 
                s.StationName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.StationCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.City.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();
        }

        public async Task<Dictionary<SeatClass, int>> GetAvailabilityAsync(string trainNumber, DateTime travelDate)
        {
            await Task.Delay(300);
            var train = _trains.FirstOrDefault(t => t.TrainNumber == trainNumber);
            return train?.AvailableSeats ?? new Dictionary<SeatClass, int>();
        }

        public async Task<Dictionary<SeatClass, decimal>> GetPricingAsync(string trainNumber, string source, string destination)
        {
            await Task.Delay(250);
            var train = _trains.FirstOrDefault(t => t.TrainNumber == trainNumber);
            return train?.Prices ?? new Dictionary<SeatClass, decimal>();
        }

        public async Task<List<Train>> GetRecommendedTrainsAsync(BookingRequest request)
        {
            await Task.Delay(400);
            var trains = await SearchTrainsAsync(request.Source, request.Destination, request.TravelDate);
            
            return trains.OrderByDescending(t => t.Rating)
                        .ThenBy(t => t.Prices.GetValueOrDefault(request.PreferredClass, decimal.MaxValue))
                        .Take(5)
                        .ToList();
        }

        public async Task<TrainStatus> GetTrainStatusAsync(string trainNumber)
        {
            await Task.Delay(100);
            var train = _trains.FirstOrDefault(t => t.TrainNumber == trainNumber);
            return train?.Status ?? TrainStatus.OnTime;
        }

        public async Task<List<Coach>> GetCoachLayoutAsync(string trainNumber)
        {
            await Task.Delay(200);
            var train = _trains.FirstOrDefault(t => t.TrainNumber == trainNumber);
            return train?.Coaches ?? new List<Coach>();
        }

        public async Task<bool> CheckSeatAvailabilityAsync(string trainNumber, string coachNumber, string seatNumber)
        {
            await Task.Delay(100);
            var train = _trains.FirstOrDefault(t => t.TrainNumber == trainNumber);
            var coach = train?.Coaches.FirstOrDefault(c => c.CoachNumber == coachNumber);
            var seat = coach?.Seats.FirstOrDefault(s => s.SeatNumber == seatNumber);
            return seat?.Status == SeatStatus.Available;
        }

        public async Task RefreshDataAsync()
        {
            await Task.Delay(1000);
            // Simulate refreshing data from external API
            foreach (var train in _trains)
            {
                // Simulate real-time updates
                if (Random.Shared.NextDouble() < 0.1)
                {
                    train.DelayMinutes = Random.Shared.Next(0, 30);
                    train.Status = train.DelayMinutes > 0 ? TrainStatus.Delayed : TrainStatus.OnTime;
                }
            }
        }

        private List<Station> InitializeStations()
        {
            return new List<Station>
            {
                new() { StationCode = "NDLS", StationName = "New Delhi", City = "Delhi", State = "Delhi", Type = StationType.Major },
                new() { StationCode = "BCT", StationName = "Mumbai Central", City = "Mumbai", State = "Maharashtra", Type = StationType.Terminal },
                new() { StationCode = "BLR", StationName = "Bangalore City", City = "Bangalore", State = "Karnataka", Type = StationType.Major },
                new() { StationCode = "MAS", StationName = "Chennai Central", City = "Chennai", State = "Tamil Nadu", Type = StationType.Central },
                new() { StationCode = "HWH", StationName = "Howrah Junction", City = "Kolkata", State = "West Bengal", Type = StationType.Junction },
                new() { StationCode = "PUNE", StationName = "Pune Junction", City = "Pune", State = "Maharashtra", Type = StationType.Junction },
                new() { StationCode = "AGC", StationName = "Agra Cantt", City = "Agra", State = "Uttar Pradesh", Type = StationType.Major },
                new() { StationCode = "JP", StationName = "Jaipur", City = "Jaipur", State = "Rajasthan", Type = StationType.Major }
            };
        }

        private List<Train> InitializeTrains()
        {
            var trains = new List<Train>();
            var random = Random.Shared;

            // Premium trains
            trains.Add(CreateTrain("12951", "Rajdhani Express", "NDLS", "BCT", TrainType.SuperFast, 4.8m));
            trains.Add(CreateTrain("12301", "Howrah Rajdhani", "NDLS", "HWH", TrainType.SuperFast, 4.9m));
            trains.Add(CreateTrain("12621", "Tamil Nadu Express", "NDLS", "MAS", TrainType.Express, 4.6m));
            trains.Add(CreateTrain("12431", "Trivandrum Rajdhani", "NDLS", "TVC", TrainType.SuperFast, 4.7m));
            trains.Add(CreateTrain("22691", "Rajdhani Express", "NDLS", "BLR", TrainType.SuperFast, 4.8m));

            // Express trains
            trains.Add(CreateTrain("12002", "Shatabdi Express", "NDLS", "AGC", TrainType.Express, 4.5m));
            trains.Add(CreateTrain("12011", "Kalka Shatabdi", "NDLS", "KLK", TrainType.Express, 4.4m));
            trains.Add(CreateTrain("12723", "Telangana Express", "HYB", "NDLS", TrainType.Express, 4.3m));

            foreach (var train in trains)
            {
                GenerateCoaches(train);
                GeneratePricing(train);
                UpdateAvailability(train);
            }

            return trains;
        }

        private Train CreateTrain(string number, string name, string source, string destination, TrainType type, decimal rating)
        {
            var random = Random.Shared;
            var departureTime = DateTime.Today.AddHours(random.Next(6, 22));
            var duration = TimeSpan.FromHours(random.Next(8, 24));

            return new Train
            {
                TrainNumber = number,
                TrainName = name,
                Source = source,
                Destination = destination,
                Type = type,
                DepartureTime = departureTime,
                ArrivalTime = departureTime.Add(duration),
                Duration = duration,
                Rating = (double)rating,
                Status = TrainStatus.OnTime,
                Platform = random.Next(1, 12).ToString(),
                HasFood = true,
                HasWiFi = type == TrainType.SuperFast || type == TrainType.HighSpeed,
                HasAC = true,
                DelayMinutes = 0
            };
        }

        private void GenerateCoaches(Train train)
        {
            var coaches = new List<Coach>();
            var coachTypes = new[] { SeatClass.FirstAC, SeatClass.SecondAC, SeatClass.ThirdAC, SeatClass.SleeperClass };

            for (int i = 0; i < coachTypes.Length; i++)
            {
                var coach = new Coach
                {
                    CoachNumber = $"{coachTypes[i].ToString().Substring(0, 2)}{i + 1}",
                    Class = coachTypes[i],
                    Type = CoachType.AC,
                    HasAC = coachTypes[i] != SeatClass.SleeperClass,
                    HasToilet = true,
                    Position = i + 1
                };

                GenerateSeats(coach);
                coaches.Add(coach);
            }

            train.Coaches = coaches;
        }

        private void GenerateSeats(Coach coach)
        {
            var seats = new List<Seat>();
            int seatCount = coach.Class == SeatClass.FirstAC ? 18 : 
                           coach.Class == SeatClass.SecondAC ? 46 : 64;

            for (int i = 1; i <= seatCount; i++)
            {
                seats.Add(new Seat
                {
                    SeatNumber = $"{i}",
                    Type = i % 4 == 1 || i % 4 == 2 ? SeatType.LowerBerth : SeatType.UpperBerth,
                    Status = Random.Shared.NextDouble() < 0.7 ? SeatStatus.Available : SeatStatus.Booked,
                    Row = (i - 1) / 4 + 1,
                    Column = (i - 1) % 4 + 1,
                    IsWindow = i % 4 == 1 || i % 4 == 0,
                    IsAisle = i % 4 == 2 || i % 4 == 3
                });
            }

            coach.Seats = seats;
            coach.TotalSeats = seatCount;
            coach.AvailableSeats = seats.Count(s => s.Status == SeatStatus.Available);
        }

        private void GeneratePricing(Train train)
        {
            var basePrices = new Dictionary<SeatClass, decimal>
            {
                { SeatClass.GeneralSeating, 250 },
                { SeatClass.SleeperClass, 400 },
                { SeatClass.ThirdAC, 850 },
                { SeatClass.SecondAC, 1200 },
                { SeatClass.FirstAC, 2100 },
                { SeatClass.ExecutiveChair, 1800 }
            };

            var multiplier = train.Type == TrainType.SuperFast ? 1.3m : 1.0m;
            train.Prices = basePrices.ToDictionary(kvp => kvp.Key, kvp => kvp.Value * multiplier);
        }

        private void UpdateAvailability(Train train)
        {
            train.AvailableSeats = train.Coaches
                .GroupBy(c => c.Class)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.AvailableSeats));
        }
    }
} 