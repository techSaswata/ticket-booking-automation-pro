using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrainBookingAutomation.Models;

namespace TrainBookingAutomation.Services
{
    public class BookingEngine : IBookingEngine
    {
        private readonly ITrainDataService _trainDataService;
        private readonly IAIRecommendationService _aiService;
        private readonly IPriceOptimizationService _priceService;
        private readonly INotificationService _notificationService;
        
        private readonly List<BookingResult> _bookingHistory = new();
        private readonly Dictionary<string, CancellationTokenSource> _activeAutomations = new();
        private readonly Dictionary<string, BookingRequest> _pendingBookings = new();

        public event EventHandler<BookingResult> BookingCompleted;
        public event EventHandler<BookingResult> BookingFailed;
        public event EventHandler<string> BookingStatusChanged;

        public BookingEngine(
            ITrainDataService trainDataService,
            IAIRecommendationService aiService,
            IPriceOptimizationService priceService,
            INotificationService notificationService)
        {
            _trainDataService = trainDataService;
            _aiService = aiService;
            _priceService = priceService;
            _notificationService = notificationService;
            
            InitializeBookingHistory();
        }

        public async Task<BookingResult> BookTicketAsync(BookingRequest request)
        {
            var startTime = DateTime.Now;
            BookingStatusChanged?.Invoke(this, "Initiating booking process...");

            try
            {
                // Step 1: Validate request
                if (!await ValidateBookingRequestAsync(request))
                {
                    return CreateFailedResult(request, "Invalid booking request");
                }

                // Step 2: Get available trains
                var availableTrains = await _trainDataService.SearchTrainsAsync(
                    request.Source, request.Destination, request.TravelDate);

                if (!availableTrains.Any())
                {
                    return CreateFailedResult(request, "No trains available for selected route");
                }

                // Step 3: Select best train using AI
                var selectedTrain = await SelectOptimalTrainAsync(request, availableTrains);
                if (selectedTrain == null)
                {
                    return CreateFailedResult(request, "No suitable train found");
                }

                BookingStatusChanged?.Invoke(this, $"Selected {selectedTrain.TrainName}");

                // Step 4: Optimize seat allocation
                var seatAllocations = await OptimizeSeatAllocationAsync(request, selectedTrain);
                if (!seatAllocations.Any())
                {
                    return CreateFailedResult(request, "No seats available in preferred class");
                }

                // Step 5: Calculate pricing
                var totalAmount = CalculateTotalAmount(seatAllocations, selectedTrain);
                
                // Step 6: Simulate booking process
                BookingStatusChanged?.Invoke(this, "Processing payment...");
                await Task.Delay(2000); // Simulate booking API call

                // Step 7: Generate booking result
                var result = new BookingResult
                {
                    PNR = GeneratePNR(),
                    Request = request,
                    SelectedTrain = selectedTrain,
                    SeatAllocations = seatAllocations,
                    TotalAmount = totalAmount,
                    TaxAmount = totalAmount * 0.05m,
                    ConvenienceFee = 40m,
                    Status = BookingStatus.Confirmed,
                    PaymentStatus = PaymentStatus.Completed,
                    BookingDuration = DateTime.Now - startTime,
                    AttemptNumber = 1,
                    Channel = BookingChannel.Desktop,
                    QRCode = GenerateQRCode(),
                    Messages = new List<string> { "Booking confirmed successfully" }
                };

                _bookingHistory.Add(result);
                BookingCompleted?.Invoke(this, result);
                
                await _notificationService.SendBookingConfirmationAsync(result);
                
                return result;
            }
            catch (Exception ex)
            {
                var failureResult = CreateFailedResult(request, ex.Message);
                BookingFailed?.Invoke(this, failureResult);
                await _notificationService.SendBookingFailureNotificationAsync(request, ex.Message);
                return failureResult;
            }
        }

        public async Task<BookingResult> AutomatedBookingAsync(BookingRequest request)
        {
            var maxAttempts = request.MaxRetries;
            var retryInterval = request.RetryInterval;
            var attempt = 1;

            BookingStatusChanged?.Invoke(this, "Starting automated booking...");

            while (attempt <= maxAttempts)
            {
                try
                {
                    BookingStatusChanged?.Invoke(this, $"Attempt {attempt}/{maxAttempts}");
                    
                    // Check if better price is available if price tracking is enabled
                    if (request.AutomationSettings.EnablePriceTracking)
                    {
                        var shouldWait = await _priceService.ShouldWaitForBetterPriceAsync(request);
                        if (shouldWait && attempt < maxAttempts)
                        {
                            BookingStatusChanged?.Invoke(this, "Waiting for better price...");
                            await Task.Delay(retryInterval);
                            attempt++;
                            continue;
                        }
                    }

                    var result = await BookTicketAsync(request);
                    result.AttemptNumber = attempt;

                    if (result.Status == BookingStatus.Confirmed)
                    {
                        return result;
                    }

                    if (result.Status == BookingStatus.Waitlisted && 
                        request.AutomationSettings.EnableWaitlistMonitoring)
                    {
                        BookingStatusChanged?.Invoke(this, "Monitoring waitlist...");
                        _ = Task.Run(() => MonitorWaitlistAsync(result.PNR));
                        return result;
                    }

                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(retryInterval);
                    }
                }
                catch (Exception ex)
                {
                    BookingStatusChanged?.Invoke(this, $"Attempt {attempt} failed: {ex.Message}");
                    
                    if (attempt == maxAttempts)
                    {
                        return CreateFailedResult(request, $"All {maxAttempts} attempts failed. Last error: {ex.Message}");
                    }
                    
                    await Task.Delay(retryInterval);
                }

                attempt++;
            }

            return CreateFailedResult(request, $"Booking failed after {maxAttempts} attempts");
        }

        public async Task<List<BookingResult>> BulkBookingAsync(List<BookingRequest> requests)
        {
            var results = new List<BookingResult>();
            var semaphore = new SemaphoreSlim(3, 3); // Limit concurrent bookings

            var tasks = requests.Select(async request =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await AutomatedBookingAsync(request);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            results.AddRange(await Task.WhenAll(tasks));
            return results;
        }

        public async Task StartAutomationAsync(BookingRequest request)
        {
            var cancellationSource = new CancellationTokenSource();
            _activeAutomations[request.Id] = cancellationSource;
            _pendingBookings[request.Id] = request;

            BookingStatusChanged?.Invoke(this, $"Automation started for {request.Id}");

            _ = Task.Run(async () =>
            {
                try
                {
                    // Wait for booking window
                    var timeUntilBooking = request.TravelDate.Subtract(DateTime.Now)
                                                  .Subtract(request.AutomationSettings.BookingWindow);
                    
                    if (timeUntilBooking > TimeSpan.Zero)
                    {
                        await Task.Delay(timeUntilBooking, cancellationSource.Token);
                    }

                    // Start booking attempts
                    var result = await AutomatedBookingAsync(request);
                    
                    _activeAutomations.Remove(request.Id);
                    _pendingBookings.Remove(request.Id);
                }
                catch (OperationCanceledException)
                {
                    BookingStatusChanged?.Invoke(this, $"Automation cancelled for {request.Id}");
                }
            }, cancellationSource.Token);
        }

        public async Task StopAutomationAsync(string bookingId)
        {
            if (_activeAutomations.TryGetValue(bookingId, out var cancellationSource))
            {
                cancellationSource.Cancel();
                _activeAutomations.Remove(bookingId);
                _pendingBookings.Remove(bookingId);
                
                BookingStatusChanged?.Invoke(this, $"Automation stopped for {bookingId}");
            }
            await Task.CompletedTask;
        }

        public async Task<List<BookingRequest>> GetActiveAutomationsAsync()
        {
            await Task.CompletedTask;
            return _pendingBookings.Values.ToList();
        }

        public async Task<List<SeatAllocation>> OptimizeSeatAllocationAsync(BookingRequest request, Train train)
        {
            var allocations = new List<SeatAllocation>();
            var availableCoaches = train.Coaches.Where(c => c.Class == request.PreferredClass).ToList();

            if (!availableCoaches.Any())
            {
                // Try alternative classes
                availableCoaches = train.Coaches.Where(c => c.AvailableSeats > 0).ToList();
            }

            foreach (var passenger in request.Passengers)
            {
                var bestSeat = await FindBestSeatAsync(passenger, availableCoaches, allocations);
                if (bestSeat != null)
                {
                    allocations.Add(new SeatAllocation
                    {
                        Passenger = passenger,
                        Coach = bestSeat.coach,
                        Seat = bestSeat.seat,
                        Fare = train.Prices.GetValueOrDefault(bestSeat.coach.Class, 0),
                        IsConfirmed = true
                    });

                    // Remove allocated seat from available seats
                    bestSeat.seat.Status = SeatStatus.Booked;
                    bestSeat.coach.AvailableSeats--;
                }
            }

            return allocations;
        }

        private async Task<(Coach coach, Seat seat)?> FindBestSeatAsync(
            Passenger passenger, 
            List<Coach> coaches, 
            List<SeatAllocation> existingAllocations)
        {
            await Task.Delay(100); // Simulate AI processing

            foreach (var coach in coaches.OrderBy(c => (int)c.Class))
            {
                var availableSeats = coach.Seats.Where(s => s.Status == SeatStatus.Available).ToList();
                
                // Apply passenger preferences
                var preferredSeats = availableSeats.Where(s =>
                    passenger.SeatPreference == SeatPreference.NoPreference ||
                    (passenger.SeatPreference == SeatPreference.Window && s.IsWindow) ||
                    (passenger.SeatPreference == SeatPreference.Aisle && s.IsAisle) ||
                    (passenger.SeatPreference == SeatPreference.Lower && s.Type == SeatType.LowerBerth) ||
                    (passenger.SeatPreference == SeatPreference.Upper && s.Type == SeatType.UpperBerth)
                ).ToList();

                // Try to keep family/group together
                if (preferredSeats.Any())
                {
                    var bestSeat = preferredSeats.First();
                    return (coach, bestSeat);
                }

                if (availableSeats.Any())
                {
                    return (coach, availableSeats.First());
                }
            }

            return null;
        }

        private async Task<Train> SelectOptimalTrainAsync(BookingRequest request, List<Train> availableTrains)
        {
            // Use AI service to get recommendations
            var recommendations = await _aiService.GetRouteRecommendationsAsync(request);
            
            // Score trains based on multiple factors
            var scoredTrains = availableTrains.Select(train => new
            {
                Train = train,
                Score = CalculateTrainScore(train, request)
            }).OrderByDescending(x => x.Score);

            return scoredTrains.FirstOrDefault()?.Train;
        }

        private double CalculateTrainScore(Train train, BookingRequest request)
        {
            double score = 0;

            // Rating weight (30%)
            score += train.Rating * 0.3;

            // Price weight (25%)
            var price = train.Prices.GetValueOrDefault(request.PreferredClass, decimal.MaxValue);
            var priceScore = Math.Max(0, (2000 - (double)price) / 2000) * 25;
            score += priceScore;

            // Availability weight (25%)
            var availability = train.AvailableSeats.GetValueOrDefault(request.PreferredClass, 0);
            var availabilityScore = Math.Min(availability / (double)request.Passengers.Count, 1) * 25;
            score += availabilityScore;

            // Time preference weight (20%)
            var departureHour = train.DepartureTime.Hour;
            var timeScore = departureHour >= 6 && departureHour <= 22 ? 20 : 10;
            score += timeScore;

            return score;
        }

        private decimal CalculateTotalAmount(List<SeatAllocation> allocations, Train train)
        {
            return allocations.Sum(a => a.Fare);
        }

        private async Task MonitorWaitlistAsync(string pnr)
        {
            var maxMonitoringTime = TimeSpan.FromHours(24);
            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < maxMonitoringTime)
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
                
                // Simulate waitlist status check
                if (Random.Shared.NextDouble() < 0.1) // 10% chance of confirmation
                {
                    BookingStatusChanged?.Invoke(this, $"Waitlist confirmed for PNR {pnr}");
                    await _notificationService.SendWaitlistUpdateAsync(pnr, "Confirmed");
                    break;
                }
            }
        }

        private async Task<bool> ValidateBookingRequestAsync(BookingRequest request)
        {
            if (string.IsNullOrEmpty(request.Source) || string.IsNullOrEmpty(request.Destination))
                return false;

            if (request.TravelDate <= DateTime.Today)
                return false;

            if (!request.Passengers.Any())
                return false;

            return await Task.FromResult(true);
        }

        private BookingResult CreateFailedResult(BookingRequest request, string message)
        {
            return new BookingResult
            {
                Request = request,
                Status = BookingStatus.Failed,
                Messages = new List<string> { message },
                BookingDuration = TimeSpan.Zero
            };
        }

        private string GeneratePNR()
        {
            var random = Random.Shared;
            return $"{random.Next(1000, 9999)}{random.Next(100000, 999999)}";
        }

        private string GenerateQRCode()
        {
            return Guid.NewGuid().ToString("N")[..8].ToUpper();
        }

        public async Task<List<BookingResult>> GetBookingHistoryAsync(string userId)
        {
            await Task.Delay(100);
            return _bookingHistory.Where(b => b.Request.UserId == userId || userId == "default_user").ToList();
        }

        public async Task<BookingResult> GetBookingDetailsAsync(string pnr)
        {
            await Task.Delay(100);
            return _bookingHistory.FirstOrDefault(b => b.PNR == pnr);
        }

        public async Task<BookingAnalytics> GetBookingAnalyticsAsync(DateTime fromDate, DateTime toDate)
        {
            await Task.Delay(200);
            
            var bookingsInRange = _bookingHistory.Where(b => 
                b.BookedAt >= fromDate && b.BookedAt <= toDate).ToList();

            return new BookingAnalytics
            {
                TotalBookings = bookingsInRange.Count,
                SuccessfulBookings = bookingsInRange.Count(b => b.Status == BookingStatus.Confirmed),
                FailedBookings = bookingsInRange.Count(b => b.Status == BookingStatus.Failed),
                WaitlistedBookings = bookingsInRange.Count(b => b.Status == BookingStatus.Waitlisted),
                SuccessRate = bookingsInRange.Any() ? 
                    (decimal)bookingsInRange.Count(b => b.Status == BookingStatus.Confirmed) / bookingsInRange.Count : 0,
                AverageBookingTime = bookingsInRange.Any() ? 
                    TimeSpan.FromMilliseconds(bookingsInRange.Average(b => b.BookingDuration.TotalMilliseconds)) : TimeSpan.Zero,
                TotalAmountBooked = bookingsInRange.Sum(b => b.TotalAmount)
            };
        }

        public async Task<BookingResult> RetryBookingAsync(string bookingId)
        {
            var originalBooking = _bookingHistory.FirstOrDefault(b => b.BookingId == bookingId);
            if (originalBooking?.Request != null)
            {
                return await BookTicketAsync(originalBooking.Request);
            }
            return null;
        }

        public async Task<bool> CancelBookingAsync(string pnr)
        {
            await Task.Delay(500);
            var booking = _bookingHistory.FirstOrDefault(b => b.PNR == pnr);
            if (booking != null)
            {
                booking.Status = BookingStatus.Cancelled;
                return true;
            }
            return false;
        }

        public async Task<BookingResult> ModifyBookingAsync(string pnr, BookingRequest newRequest)
        {
            await CancelBookingAsync(pnr);
            return await BookTicketAsync(newRequest);
        }

        public async Task<bool> ProcessWaitlistAsync(string pnr)
        {
            await Task.Delay(1000);
            return Random.Shared.NextDouble() < 0.7; // 70% success rate
        }

        public async Task<bool> MonitorBookingStatusAsync(string bookingId)
        {
            await Task.Delay(100);
            return _activeAutomations.ContainsKey(bookingId);
        }

        private void InitializeBookingHistory()
        {
            // Add some sample booking history
            _bookingHistory.AddRange(new[]
            {
                new BookingResult
                {
                    PNR = "2346789012",
                    BookedAt = DateTime.Now.AddDays(-5),
                    Status = BookingStatus.Confirmed,
                    TotalAmount = 1250m,
                    Request = new BookingRequest { Source = "NDLS", Destination = "BCT" },
                    SelectedTrain = new Train { TrainName = "Rajdhani Express" }
                },
                new BookingResult
                {
                    PNR = "3456789013",
                    BookedAt = DateTime.Now.AddDays(-3),
                    Status = BookingStatus.Confirmed,
                    TotalAmount = 950m,
                    Request = new BookingRequest { Source = "NDLS", Destination = "AGC" },
                    SelectedTrain = new Train { TrainName = "Shatabdi Express" }
                }
            });
        }
    }
} 