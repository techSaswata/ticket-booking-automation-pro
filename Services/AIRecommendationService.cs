using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public class AIRecommendationService : IAIRecommendationService
    {
        private readonly ITrainDataService _trainDataService;
        private readonly Random _random = Random.Shared;

        public AIRecommendationService(ITrainDataService trainDataService)
        {
            _trainDataService = trainDataService;
        }

        public async Task<List<AIRecommendation>> GetRouteRecommendationsAsync(BookingRequest request)
        {
            await Task.Delay(300); // Simulate AI processing
            
            var recommendations = new List<AIRecommendation>();

            // Route optimization recommendation
            recommendations.Add(new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Type = "Route Optimization",
                Title = "🚄 Fastest Route Detected",
                Description = "Based on historical data, Rajdhani Express offers the best speed-to-comfort ratio for this route. Average delay: 12 minutes.",
                Confidence = 0.89,
                Category = RecommendationCategory.RouteOptimization,
                Data = new Dictionary<string, object>
                {
                    ["recommended_train"] = "12951",
                    ["time_saved"] = "2.5 hours",
                    ["confidence_factors"] = new[] { "punctuality", "speed", "passenger_rating" }
                }
            });

            // Time optimization
            recommendations.Add(new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Type = "Time Optimization",
                Title = "⏰ Optimal Departure Time",
                Description = "Trains departing between 6:00-8:00 AM have 94% on-time performance. Consider morning departures for better punctuality.",
                Confidence = 0.94,
                Category = RecommendationCategory.TimeOptimization,
                Data = new Dictionary<string, object>
                {
                    ["best_departure_window"] = "06:00-08:00",
                    ["punctuality_score"] = 94,
                    ["delay_probability"] = 0.06
                }
            });

            // Class upgrade recommendation
            if (request.PreferredClass == SeatClass.ThirdAC)
            {
                recommendations.Add(new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Class Upgrade",
                    Title = "💎 Smart Upgrade Suggestion",
                    Description = "2AC is only ₹350 more expensive but offers 60% more comfort. Limited seats available - book now!",
                    Confidence = 0.76,
                    Category = RecommendationCategory.ClassUpgrade,
                    Data = new Dictionary<string, object>
                    {
                        ["upgrade_to"] = "SecondAC",
                        ["additional_cost"] = 350,
                        ["comfort_improvement"] = 60,
                        ["availability"] = "limited"
                    }
                });
            }

            // Price optimization
            recommendations.Add(new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Type = "Price Alert",
                Title = "💰 Price Drop Prediction",
                Description = "AI predicts 15% price drop in next 3 days. Set price alert to save ₹180 on average.",
                Confidence = 0.67,
                Category = RecommendationCategory.PriceOptimization,
                Data = new Dictionary<string, object>
                {
                    ["predicted_savings"] = 180,
                    ["drop_probability"] = 0.67,
                    ["monitoring_period"] = "3 days"
                }
            });

            // Seasonal recommendation
            if (IsWeekend(request.TravelDate))
            {
                recommendations.Add(new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Weekend Travel",
                    Title = "🏖️ Weekend Travel Insight",
                    Description = "Weekend travel detected. Book 2-3 days earlier to avoid 40% price surge. Consider Friday evening departure.",
                    Confidence = 0.82,
                    Category = RecommendationCategory.TimeOptimization,
                    Data = new Dictionary<string, object>
                    {
                        ["surge_risk"] = 0.40,
                        ["optimal_booking"] = "2-3 days earlier",
                        ["alternative_dates"] = new[] { "Friday evening", "Monday morning" }
                    }
                });
            }

            return recommendations;
        }

        public async Task<List<AIRecommendation>> GetSeatRecommendationsAsync(Train train, List<Passenger> passengers)
        {
            await Task.Delay(200);
            
            var recommendations = new List<AIRecommendation>();

            // Family seating recommendation
            if (passengers.Count > 1)
            {
                recommendations.Add(new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Family Seating",
                    Title = "👨‍👩‍👧‍👦 Family-Friendly Seating",
                    Description = "Coach A3 has adjacent seats available. Perfect for families with 95% passenger satisfaction rating.",
                    Confidence = 0.95,
                    Category = RecommendationCategory.SeatSelection,
                    Data = new Dictionary<string, object>
                    {
                        ["recommended_coach"] = "A3",
                        ["adjacent_seats"] = passengers.Count,
                        ["family_rating"] = 4.8
                    }
                });
            }

            // Comfort recommendation based on passenger age
            var hasElderly = passengers.Any(p => p.Age > 60);
            var hasChildren = passengers.Any(p => p.Age < 12);

            if (hasElderly || hasChildren)
            {
                recommendations.Add(new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Comfort Optimization",
                    Title = "🛏️ Comfort-Optimized Seating",
                    Description = hasElderly ? 
                        "Lower berth recommended for elderly passengers. Coach B1 has easy washroom access." :
                        "Child-friendly seating in coach C2 with play area nearby and extra safety features.",
                    Confidence = 0.88,
                    Category = RecommendationCategory.SeatSelection,
                    Data = new Dictionary<string, object>
                    {
                        ["recommended_berth"] = hasElderly ? "Lower" : "Middle",
                        ["special_features"] = hasElderly ? "washroom_access" : "child_friendly",
                        ["safety_rating"] = 4.9
                    }
                });
            }

            return recommendations;
        }

        public async Task<List<AIRecommendation>> GetPriceOptimizationAsync(string route, SeatClass seatClass)
        {
            await Task.Delay(250);
            
            var recommendations = new List<AIRecommendation>
            {
                new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Price Prediction",
                    Title = "📈 Price Trend Analysis",
                    Description = "Historical data shows 23% price increase during peak season. Current price is 12% below average.",
                    Confidence = 0.81,
                    Category = RecommendationCategory.PriceOptimization,
                    Data = new Dictionary<string, object>
                    {
                        ["current_vs_average"] = -0.12,
                        ["peak_season_increase"] = 0.23,
                        ["recommendation"] = "book_now"
                    }
                },
                new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Alternative Routes",
                    Title = "🔄 Cost-Effective Alternatives",
                    Description = "Via junction route saves ₹420 with only 45 minutes extra travel time. 87% passenger satisfaction.",
                    Confidence = 0.73,
                    Category = RecommendationCategory.RouteOptimization,
                    Data = new Dictionary<string, object>
                    {
                        ["savings"] = 420,
                        ["extra_time"] = 45,
                        ["satisfaction"] = 0.87
                    }
                }
            };

            return recommendations;
        }

        public async Task<List<AIRecommendation>> GetTimeOptimizationAsync(BookingRequest request)
        {
            await Task.Delay(200);
            
            var recommendations = new List<AIRecommendation>();

            // Departure time optimization
            var optimalHour = GetOptimalDepartureHour(request.TravelDate);
            recommendations.Add(new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Type = "Departure Timing",
                Title = "⏰ Optimal Departure Window",
                Description = $"Best departure time: {optimalHour}:00-{optimalHour + 2}:00. 91% on-time performance in this window.",
                Confidence = 0.91,
                Category = RecommendationCategory.TimeOptimization,
                Data = new Dictionary<string, object>
                {
                    ["optimal_window"] = $"{optimalHour}:00-{optimalHour + 2}:00",
                    ["punctuality"] = 0.91,
                    ["delay_risk"] = "low"
                }
            });

            // Booking timing recommendation
            var daysUntilTravel = (request.TravelDate - DateTime.Today).Days;
            if (daysUntilTravel > 7)
            {
                recommendations.Add(new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Booking Timing",
                    Title = "🎯 Optimal Booking Window",
                    Description = "Book 5-7 days before travel for best prices. You're in the sweet spot!",
                    Confidence = 0.84,
                    Category = RecommendationCategory.PriceOptimization,
                    Data = new Dictionary<string, object>
                    {
                        ["days_until_travel"] = daysUntilTravel,
                        ["price_advantage"] = 0.15,
                        ["availability_risk"] = "low"
                    }
                });
            }

            return recommendations;
        }

        public async Task<List<Train>> GetAlternativeTrainsAsync(BookingRequest request)
        {
            var allTrains = await _trainDataService.SearchTrainsAsync(
                request.Source, request.Destination, request.TravelDate);
            
            // Return alternative trains with different departure times
            return allTrains.Where(t => !request.PreferredTrains.Contains(t.TrainNumber))
                           .OrderBy(t => Math.Abs((t.DepartureTime.Hour - 8))) // Prefer morning departures
                           .Take(3)
                           .ToList();
        }

        public async Task<AIRecommendation> GetBestBookingTimeAsync(string route)
        {
            await Task.Delay(150);
            
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Type = "Booking Window",
                Title = "🎯 Optimal Booking Time",
                Description = "Best booking window: 7-10 days before travel. 23% average savings and 94% availability guarantee.",
                Confidence = 0.89,
                Category = RecommendationCategory.PriceOptimization,
                Data = new Dictionary<string, object>
                {
                    ["optimal_window"] = "7-10 days",
                    ["average_savings"] = 0.23,
                    ["availability"] = 0.94
                }
            };
        }

        public async Task<List<AIRecommendation>> GetClassUpgradeRecommendationsAsync(BookingRequest request)
        {
            await Task.Delay(180);
            
            var recommendations = new List<AIRecommendation>();

            if (request.PreferredClass == SeatClass.ThirdAC)
            {
                recommendations.Add(new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Class Upgrade",
                    Title = "⬆️ Smart Upgrade to 2AC",
                    Description = "Only ₹400 more for 2AC. Includes meals, better bedding, and less crowded. 96% recommendation rate.",
                    Confidence = 0.86,
                    Category = RecommendationCategory.ClassUpgrade,
                    Data = new Dictionary<string, object>
                    {
                        ["upgrade_cost"] = 400,
                        ["benefits"] = new[] { "meals", "better_bedding", "less_crowded" },
                        ["recommendation_rate"] = 0.96
                    }
                });
            }

            if (request.Passengers.Count > 2 && request.PreferredClass != SeatClass.FirstAC)
            {
                recommendations.Add(new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Group Upgrade",
                    Title = "👥 Group Upgrade Benefits",
                    Description = "1AC offers private coupe for your group. Total upgrade cost ₹1200 for enhanced privacy and comfort.",
                    Confidence = 0.78,
                    Category = RecommendationCategory.ClassUpgrade,
                    Data = new Dictionary<string, object>
                    {
                        ["total_upgrade_cost"] = 1200,
                        ["benefits"] = new[] { "privacy", "space", "premium_service" },
                        ["group_size"] = request.Passengers.Count
                    }
                });
            }

            return recommendations;
        }

        public async Task<double> PredictBookingSuccessRateAsync(BookingRequest request)
        {
            await Task.Delay(100);
            
            double successRate = 0.85; // Base success rate

            // Adjust based on various factors
            var daysUntilTravel = (request.TravelDate - DateTime.Today).Days;
            
            if (daysUntilTravel < 3)
                successRate -= 0.20; // Last-minute booking
            else if (daysUntilTravel > 30)
                successRate -= 0.10; // Too early
            
            if (IsWeekend(request.TravelDate))
                successRate -= 0.15; // Weekend travel
            
            if (request.Passengers.Count > 4)
                successRate -= 0.10; // Large group
            
            if (request.PreferredClass == SeatClass.FirstAC)
                successRate -= 0.05; // Premium class
            
            return Math.Max(0.30, Math.Min(0.98, successRate));
        }

        public async Task<List<AIRecommendation>> GetPersonalizedRecommendationsAsync(string userId)
        {
            await Task.Delay(200);
            
            // This would typically use user's booking history and preferences
            return new List<AIRecommendation>
            {
                new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Personal Preference",
                    Title = "🎯 Based on Your History",
                    Description = "You prefer morning departures and window seats. Rajdhani Express at 6:15 AM has 8 window seats available.",
                    Confidence = 0.92,
                    Category = RecommendationCategory.SeatSelection,
                    Data = new Dictionary<string, object>
                    {
                        ["preference_match"] = 0.92,
                        ["available_preferences"] = new[] { "morning_departure", "window_seat" },
                        ["train_suggestion"] = "Rajdhani Express"
                    }
                }
            };
        }

        public async Task LearnFromBookingAsync(BookingResult result)
        {
            await Task.Delay(50);
            // This would update ML models based on booking outcomes
            // For demo purposes, we'll just simulate the learning process
        }

        public async Task<List<AIRecommendation>> GetSeasonalRecommendationsAsync(BookingRequest request)
        {
            await Task.Delay(150);
            
            var recommendations = new List<AIRecommendation>();
            var season = GetSeason(request.TravelDate);

            switch (season)
            {
                case "Summer":
                    recommendations.Add(new AIRecommendation
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = "Seasonal Advice",
                        Title = "☀️ Summer Travel Tips",
                        Description = "High AC demand in summer. Book AC classes early. Carry extra water and prefer day journeys.",
                        Confidence = 0.88,
                        Category = RecommendationCategory.RouteOptimization
                    });
                    break;
                    
                case "Monsoon":
                    recommendations.Add(new AIRecommendation
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = "Seasonal Advice",
                        Title = "🌧️ Monsoon Travel Alert",
                        Description = "Monsoon season detected. 25% higher delay probability. Consider flexible tickets and avoid flood-prone routes.",
                        Confidence = 0.91,
                        Category = RecommendationCategory.TimeOptimization
                    });
                    break;
                    
                case "Winter":
                    recommendations.Add(new AIRecommendation
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = "Seasonal Advice",
                        Title = "❄️ Winter Travel Benefits",
                        Description = "Best travel season! 95% on-time performance and comfortable weather. Perfect for sightseeing routes.",
                        Confidence = 0.93,
                        Category = RecommendationCategory.TimeOptimization
                    });
                    break;
            }

            return recommendations;
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        private int GetOptimalDepartureHour(DateTime travelDate)
        {
            // Return optimal hour based on day of week and other factors
            return IsWeekend(travelDate) ? 8 : 6; // Later on weekends
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
    }
} 