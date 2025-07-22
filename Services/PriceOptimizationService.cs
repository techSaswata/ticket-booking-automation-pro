using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public class PriceOptimizationService : IPriceOptimizationService
    {
        private readonly Dictionary<string, List<PriceHistory>> _priceHistoryDatabase = new();
        private readonly Dictionary<string, decimal> _currentPrices = new();
        private readonly Dictionary<string, List<string>> _userPriceAlerts = new();

        public event EventHandler<PriceAnalysis> PriceDropDetected;

        public PriceOptimizationService()
        {
            InitializePriceData();
        }

        public async Task<List<PriceAnalysis>> GetPriceAnalysisAsync(string route, SeatClass seatClass)
        {
            await Task.Delay(200);
            
            var key = $"{route}_{seatClass}";
            var history = _priceHistoryDatabase.GetValueOrDefault(key, new List<PriceHistory>());
            
            if (!history.Any())
            {
                // Generate mock historical data
                history = GenerateMockPriceHistory(route, seatClass);
                _priceHistoryDatabase[key] = history;
            }

            var analysis = new PriceAnalysis
            {
                Route = route,
                Class = seatClass,
                MinPrice = history.Min(h => h.Price),
                MaxPrice = history.Max(h => h.Price),
                AveragePrice = history.Average(h => h.Price),
                CurrentPrice = _currentPrices.GetValueOrDefault(key, history.LastOrDefault()?.Price ?? 1000),
                LastUpdated = DateTime.Now,
                History = history.TakeLast(30).ToList()
            };

            return new List<PriceAnalysis> { analysis };
        }

        public async Task<decimal> PredictPriceAsync(string route, DateTime travelDate, SeatClass seatClass)
        {
            await Task.Delay(300);
            
            var key = $"{route}_{seatClass}";
            var basePrice = _currentPrices.GetValueOrDefault(key, 1000m);
            
            // Apply prediction factors
            var daysFuture = (travelDate - DateTime.Today).Days;
            var multiplier = 1.0m;

            // Seasonal factors
            var season = GetSeason(travelDate);
            multiplier *= season switch
            {
                "Summer" => 1.15m,
                "Winter" => 1.25m,
                "Monsoon" => 0.95m,
                _ => 1.0m
            };

            // Advance booking factors
            multiplier *= daysFuture switch
            {
                < 3 => 1.4m,   // Last minute surge
                < 7 => 1.2m,   // Week before
                < 14 => 1.0m,  // Sweet spot
                < 30 => 1.1m,  // Month ahead
                _ => 1.3m      // Too early booking
            };

            // Weekend factors
            if (IsWeekend(travelDate))
            {
                multiplier *= 1.15m;
            }

            // Holiday factors (simplified)
            if (IsNearHoliday(travelDate))
            {
                multiplier *= 1.3m;
            }

            var predictedPrice = basePrice * multiplier;
            
            // Add some randomness to simulate market volatility
            var variance = (decimal)(Random.Shared.NextDouble() * 0.1 - 0.05); // ±5%
            predictedPrice *= (1 + variance);

            return Math.Round(predictedPrice, 0);
        }

        public async Task<List<DateTime>> GetBestBookingDatesAsync(string route, int flexibilityDays)
        {
            await Task.Delay(250);
            
            var bestDates = new List<DateTime>();
            var baseDate = DateTime.Today.AddDays(1);

            // Analyze price predictions for flexible dates
            var priceData = new List<(DateTime date, decimal price)>();
            
            for (int i = 0; i <= flexibilityDays; i++)
            {
                var date = baseDate.AddDays(i);
                var predictedPrice = await PredictPriceAsync(route, date, SeatClass.ThirdAC);
                priceData.Add((date, predictedPrice));
            }

            // Return dates with lowest predicted prices
            bestDates = priceData
                .OrderBy(x => x.price)
                .Take(Math.Min(5, flexibilityDays + 1))
                .Select(x => x.date)
                .ToList();

            return bestDates;
        }

        public async Task<bool> ShouldWaitForBetterPriceAsync(BookingRequest request)
        {
            await Task.Delay(200);
            
            var currentPrice = await PredictPriceAsync(request.Source + "-" + request.Destination, 
                                                     request.TravelDate, request.PreferredClass);
            
            // Analyze price trend
            var analysis = await GetPriceAnalysisAsync(request.Source + "-" + request.Destination, 
                                                     request.PreferredClass);
            
            if (analysis.Any())
            {
                var priceAnalysis = analysis.First();
                var avgPrice = priceAnalysis.AveragePrice;
                
                // Don't wait if:
                // 1. Current price is below average
                // 2. Travel date is within 7 days
                // 3. It's a weekend or holiday
                
                var daysUntilTravel = (request.TravelDate - DateTime.Today).Days;
                
                if (currentPrice < avgPrice * 0.9m) return false; // Good price
                if (daysUntilTravel <= 7) return false; // Too close to travel
                if (IsWeekend(request.TravelDate)) return false; // Weekend travel
                if (IsNearHoliday(request.TravelDate)) return false; // Holiday travel
                
                // Consider waiting if price is significantly above average and we have time
                return currentPrice > avgPrice * 1.15m && daysUntilTravel > 14;
            }

            return false;
        }

        public async Task<decimal> GetPriceTrendAsync(string route, SeatClass seatClass, TimeSpan period)
        {
            await Task.Delay(150);
            
            var key = $"{route}_{seatClass}";
            var history = _priceHistoryDatabase.GetValueOrDefault(key, new List<PriceHistory>());
            
            if (history.Count < 2) return 0;

            var cutoffDate = DateTime.Now.Subtract(period);
            var relevantHistory = history.Where(h => h.Date >= cutoffDate).OrderBy(h => h.Date).ToList();
            
            if (relevantHistory.Count < 2) return 0;

            var firstPrice = relevantHistory.First().Price;
            var lastPrice = relevantHistory.Last().Price;
            
            return ((lastPrice - firstPrice) / firstPrice) * 100; // Return percentage change
        }

        public async Task<List<PriceHistory>> GetPriceHistoryAsync(string route, SeatClass seatClass, DateTime fromDate)
        {
            await Task.Delay(100);
            
            var key = $"{route}_{seatClass}";
            var history = _priceHistoryDatabase.GetValueOrDefault(key, new List<PriceHistory>());
            
            return history.Where(h => h.Date >= fromDate).OrderBy(h => h.Date).ToList();
        }

        public async Task UpdatePriceHistoryAsync(string route, SeatClass seatClass, decimal price, int availableSeats)
        {
            await Task.Delay(50);
            
            var key = $"{route}_{seatClass}";
            var history = _priceHistoryDatabase.GetValueOrDefault(key, new List<PriceHistory>());
            
            history.Add(new PriceHistory
            {
                Date = DateTime.Now,
                Price = price,
                AvailableSeats = availableSeats
            });

            // Keep only last 90 days of history
            var cutoffDate = DateTime.Now.AddDays(-90);
            history.RemoveAll(h => h.Date < cutoffDate);
            
            _priceHistoryDatabase[key] = history;
            _currentPrices[key] = price;

            // Check for price drops
            await CheckForPriceDrops(route, seatClass, price);
        }

        public async Task<List<string>> GetCheaperAlternativeRoutesAsync(string source, string destination)
        {
            await Task.Delay(200);
            
            // This would typically query a route optimization database
            // For demo, return some mock alternative routes
            var alternatives = new List<string>();
            
            // Add junction routes
            var junctions = new[] { "NDLS", "BCT", "HWH", "MAS", "JP" };
            
            foreach (var junction in junctions)
            {
                if (junction != source && junction != destination)
                {
                    alternatives.Add($"{source} → {junction} → {destination}");
                }
            }

            return alternatives.Take(3).ToList();
        }

        public async Task<Dictionary<SeatClass, decimal>> ComparePricesAcrossClassesAsync(string route, DateTime travelDate)
        {
            await Task.Delay(200);
            
            var prices = new Dictionary<SeatClass, decimal>();
            var classes = Enum.GetValues<SeatClass>();
            
            foreach (var seatClass in classes)
            {
                var price = await PredictPriceAsync(route, travelDate, seatClass);
                prices[seatClass] = price;
            }

            return prices;
        }

        public async Task<bool> SetPriceAlertAsync(string route, SeatClass seatClass, decimal targetPrice, string userId)
        {
            await Task.Delay(100);
            
            var alertKey = $"{route}_{seatClass}_{targetPrice}_{userId}";
            
            if (!_userPriceAlerts.ContainsKey(userId))
            {
                _userPriceAlerts[userId] = new List<string>();
            }
            
            _userPriceAlerts[userId].Add(alertKey);
            return true;
        }

        public async Task<List<string>> GetActivePriceAlertsAsync(string userId)
        {
            await Task.Delay(50);
            return _userPriceAlerts.GetValueOrDefault(userId, new List<string>());
        }

        public async Task ProcessPriceAlertsAsync()
        {
            await Task.Delay(100);
            
            // This would run periodically to check if any price alerts should be triggered
            // For demo purposes, we'll simulate processing alerts
            
            foreach (var userAlerts in _userPriceAlerts)
            {
                foreach (var alert in userAlerts.Value)
                {
                    // Parse alert and check current price
                    // If price drops below target, trigger alert
                    if (Random.Shared.NextDouble() < 0.1) // 10% chance for demo
                    {
                        var parts = alert.Split('_');
                        if (parts.Length >= 4)
                        {
                            var route = parts[0];
                            var seatClass = Enum.Parse<SeatClass>(parts[1]);
                            var targetPrice = decimal.Parse(parts[2]);
                            
                            var analysis = new PriceAnalysis
                            {
                                Route = route,
                                Class = seatClass,
                                CurrentPrice = targetPrice - 50, // Simulate price drop
                                LastUpdated = DateTime.Now
                            };
                            
                            PriceDropDetected?.Invoke(this, analysis);
                        }
                    }
                }
            }
        }

        private async Task CheckForPriceDrops(string route, SeatClass seatClass, decimal currentPrice)
        {
            var key = $"{route}_{seatClass}";
            var history = _priceHistoryDatabase.GetValueOrDefault(key, new List<PriceHistory>());
            
            if (history.Count < 5) return; // Need some history to detect drops
            
            var recentPrices = history.TakeLast(5).Select(h => h.Price).ToList();
            var avgRecentPrice = recentPrices.Average();
            
            // If current price is significantly lower than recent average
            if (currentPrice < avgRecentPrice * 0.85m)
            {
                var analysis = new PriceAnalysis
                {
                    Route = route,
                    Class = seatClass,
                    CurrentPrice = currentPrice,
                    AveragePrice = avgRecentPrice,
                    LastUpdated = DateTime.Now
                };
                
                PriceDropDetected?.Invoke(this, analysis);
            }
        }

        private List<PriceHistory> GenerateMockPriceHistory(string route, SeatClass seatClass)
        {
            var history = new List<PriceHistory>();
            var basePrice = GetBasePriceForClass(seatClass);
            var startDate = DateTime.Today.AddDays(-60);
            
            for (int i = 0; i < 60; i++)
            {
                var date = startDate.AddDays(i);
                var variance = (decimal)(Random.Shared.NextDouble() * 0.3 - 0.15); // ±15% variance
                var price = basePrice * (1 + variance);
                
                // Add seasonal variations
                if (IsWeekend(date))
                    price *= 1.1m;
                
                if (IsNearHoliday(date))
                    price *= 1.2m;
                
                history.Add(new PriceHistory
                {
                    Date = date,
                    Price = Math.Round(price, 0),
                    AvailableSeats = Random.Shared.Next(10, 100)
                });
            }
            
            return history;
        }

        private decimal GetBasePriceForClass(SeatClass seatClass)
        {
            return seatClass switch
            {
                SeatClass.GeneralSeating => 250m,
                SeatClass.SleeperClass => 400m,
                SeatClass.ThirdAC => 850m,
                SeatClass.SecondAC => 1200m,
                SeatClass.FirstAC => 2100m,
                SeatClass.ExecutiveChair => 1800m,
                _ => 1000m
            };
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        private bool IsNearHoliday(DateTime date)
        {
            // Simplified holiday detection
            var holidays = new[]
            {
                new DateTime(date.Year, 1, 26), // Republic Day
                new DateTime(date.Year, 8, 15), // Independence Day
                new DateTime(date.Year, 10, 2),  // Gandhi Jayanti
                new DateTime(date.Year, 12, 25)  // Christmas
            };
            
            return holidays.Any(h => Math.Abs((date - h).Days) <= 3);
        }

        private string GetSeason(DateTime date)
        {
            var month = date.Month;
            return month switch
            {
                >= 3 and <= 5 => "Summer",
                >= 6 and <= 9 => "Monsoon",
                >= 10 and <= 2 => "Winter",
                _ => "Unknown"
            };
        }

        private void InitializePriceData()
        {
            // Initialize with some current prices
            var routes = new[] { "NDLS-BCT", "NDLS-MAS", "NDLS-BLR", "BCT-MAS" };
            var classes = Enum.GetValues<SeatClass>();
            
            foreach (var route in routes)
            {
                foreach (var seatClass in classes)
                {
                    var key = $"{route}_{seatClass}";
                    _currentPrices[key] = GetBasePriceForClass(seatClass);
                }
            }
        }
    }
} 