using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TrainBookingAutomation.Models;
using TrainBookingAutomation.Services;

namespace TrainBookingAutomation.Windows
{
    public partial class MainWindow : Window
    {
        private readonly ITrainDataService _trainDataService;
        private readonly IBookingEngine _bookingEngine;
        private readonly IAIRecommendationService _aiService;
        private readonly IPriceOptimizationService _priceService;
        private readonly INotificationService _notificationService;

        private List<Train> _currentSearchResults = new();
        private List<Station> _allStations = new();

        public MainWindow(
            ITrainDataService trainDataService,
            IBookingEngine bookingEngine,
            IAIRecommendationService aiService,
            IPriceOptimizationService priceService,
            INotificationService notificationService)
        {
            InitializeComponent();
            
            _trainDataService = trainDataService;
            _bookingEngine = bookingEngine;
            _aiService = aiService;
            _priceService = priceService;
            _notificationService = notificationService;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadStationsAsync();
            await LoadInitialDataAsync();
            SetupEventHandlers();
            StartRealTimeUpdates();
        }

        private async Task LoadStationsAsync()
        {
            try
            {
                _allStations = await _trainDataService.GetAllStationsAsync();
                
                FromStationCombo.ItemsSource = _allStations;
                FromStationCombo.DisplayMemberPath = "StationName";
                
                ToStationCombo.ItemsSource = _allStations;
                ToStationCombo.DisplayMemberPath = "StationName";
                
                // Set default values
                TravelDatePicker.SelectedDate = DateTime.Today.AddDays(1);
                
                // Populate class combo
                ClassCombo.ItemsSource = Enum.GetValues(typeof(SeatClass));
                ClassCombo.SelectedItem = SeatClass.ThirdAC;
            }
            catch (Exception ex)
            {
                await ShowErrorMessageAsync($"Failed to load stations: {ex.Message}");
            }
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                // Load sample recommendations
                var sampleRequest = new BookingRequest
                {
                    Source = "NDLS",
                    Destination = "BCT",
                    TravelDate = DateTime.Today.AddDays(1),
                    PreferredClass = SeatClass.ThirdAC
                };

                var recommendations = await _aiService.GetRouteRecommendationsAsync(sampleRequest);
                RecommendationsList.ItemsSource = recommendations;

                // Load booking history
                var bookingHistory = await _bookingEngine.GetBookingHistoryAsync("default_user");
                BookingHistoryGrid.ItemsSource = bookingHistory;

                // Update stats
                var analytics = await _bookingEngine.GetBookingAnalyticsAsync(DateTime.Today.AddDays(-30), DateTime.Today);
                UpdateStatsDisplay(analytics);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Warning: {ex.Message}";
            }
        }

        private void UpdateStatsDisplay(BookingAnalytics analytics)
        {
            SuccessRateText.Text = $"{analytics.SuccessRate:P1}";
            AvgTimeText.Text = $"{analytics.AverageBookingTime.TotalSeconds:F1}s";
            ActiveBookingsText.Text = "12"; // This would be from active automations
        }

        private void SetupEventHandlers()
        {
            _bookingEngine.BookingCompleted += OnBookingCompleted;
            _bookingEngine.BookingFailed += OnBookingFailed;
            _bookingEngine.BookingStatusChanged += OnBookingStatusChanged;
        }

        private async void StartRealTimeUpdates()
        {
            // Start background task for real-time updates
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));
                    
                    Dispatcher.Invoke(() =>
                    {
                        LastUpdateText.Text = DateTime.Now.ToString("HH:mm:ss");
                    });

                    // Refresh train data periodically
                    await _trainDataService.RefreshDataAsync();
                }
            });
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSearchInputs())
                return;

            ShowLoadingOverlay(true);

            try
            {
                var fromStation = ((Station)FromStationCombo.SelectedItem)?.StationCode;
                var toStation = ((Station)ToStationCombo.SelectedItem)?.StationCode;
                var travelDate = TravelDatePicker.SelectedDate ?? DateTime.Today.AddDays(1);

                _currentSearchResults = await _trainDataService.SearchTrainsAsync(fromStation, toStation, travelDate);
                
                // Animate results
                TrainResultsList.ItemsSource = _currentSearchResults;
                AnimateSearchResults();

                // Load AI recommendations for this search
                var request = new BookingRequest
                {
                    Source = fromStation,
                    Destination = toStation,
                    TravelDate = travelDate,
                    PreferredClass = (SeatClass)ClassCombo.SelectedItem
                };

                var recommendations = await _aiService.GetRouteRecommendationsAsync(request);
                RecommendationsList.ItemsSource = recommendations;

                StatusText.Text = $"Found {_currentSearchResults.Count} trains";
                
                await _notificationService.SendDesktopNotificationAsync(
                    "Search Complete", 
                    $"Found {_currentSearchResults.Count} trains for your route", 
                    NotificationType.Desktop);
            }
            catch (Exception ex)
            {
                await ShowErrorMessageAsync($"Search failed: {ex.Message}");
            }
            finally
            {
                ShowLoadingOverlay(false);
            }
        }

        private void AnimateSearchResults()
        {
            var storyboard = (Storyboard)FindResource("SearchAnimation");
            storyboard.Begin(SearchResultsScroll);
        }

        private bool ValidateSearchInputs()
        {
            if (FromStationCombo.SelectedItem == null)
            {
                ShowErrorMessage("Please select a departure station");
                return false;
            }

            if (ToStationCombo.SelectedItem == null)
            {
                ShowErrorMessage("Please select a destination station");
                return false;
            }

            if (TravelDatePicker.SelectedDate == null || TravelDatePicker.SelectedDate < DateTime.Today)
            {
                ShowErrorMessage("Please select a valid travel date");
                return false;
            }

            var fromStation = (Station)FromStationCombo.SelectedItem;
            var toStation = (Station)ToStationCombo.SelectedItem;

            if (fromStation.StationCode == toStation.StationCode)
            {
                ShowErrorMessage("Departure and destination stations cannot be the same");
                return false;
            }

            return true;
        }

        private async void BookNowButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var train = button?.DataContext as Train;
            
            if (train == null) return;

            try
            {
                var request = CreateBookingRequest(train);
                
                if (AutomationToggle.IsChecked == true)
                {
                    // Start automated booking
                    await _bookingEngine.StartAutomationAsync(request);
                    StatusText.Text = $"Automation started for {train.TrainName}";
                    
                    await _notificationService.SendDesktopNotificationAsync(
                        "Automation Started", 
                        $"Auto-booking enabled for {train.TrainName}", 
                        NotificationType.Desktop);
                }
                else
                {
                    // Direct booking
                    ShowLoadingOverlay(true, $"Booking {train.TrainName}...");
                    
                    var result = await _bookingEngine.BookTicketAsync(request);
                    
                    if (result.Status == BookingStatus.Confirmed)
                    {
                        await ShowSuccessMessageAsync($"Booking confirmed! PNR: {result.PNR}");
                    }
                    else
                    {
                        await ShowErrorMessageAsync($"Booking failed: {string.Join(", ", result.Messages)}");
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowErrorMessageAsync($"Booking error: {ex.Message}");
            }
            finally
            {
                ShowLoadingOverlay(false);
            }
        }

        private BookingRequest CreateBookingRequest(Train train)
        {
            var passengers = new List<Passenger>();
            var passengerCount = int.TryParse(PassengerCountText.Text, out int count) ? count : 1;

            for (int i = 0; i < passengerCount; i++)
            {
                passengers.Add(new Passenger
                {
                    Name = $"Passenger {i + 1}",
                    Age = 30,
                    Gender = Gender.Male,
                    IdType = "Aadhar",
                    IdNumber = "1234567890",
                    SeatPreference = SeatPreference.Window
                });
            }

            return new BookingRequest
            {
                Source = train.Source,
                Destination = train.Destination,
                TravelDate = TravelDatePicker.SelectedDate ?? DateTime.Today.AddDays(1),
                Passengers = passengers,
                PreferredClass = (SeatClass)ClassCombo.SelectedItem,
                PreferredTrains = new List<string> { train.TrainNumber },
                Priority = BookingPriority.Normal,
                AutomationSettings = new AutomationSettings
                {
                    EnableAutoBooking = AutomationToggle.IsChecked == true,
                    EnablePriceTracking = true,
                    EnableWaitlistMonitoring = true,
                    EnableNotifications = true
                }
            };
        }

        private void TrainCard_MouseEnter(object sender, MouseEventArgs e)
        {
            // Add hover animation
            var card = sender as FrameworkElement;
            var scaleTransform = new ScaleTransform(1.02, 1.02);
            card.RenderTransform = scaleTransform;
            
            var animation = new DoubleAnimation(1.0, 1.02, TimeSpan.FromMilliseconds(200));
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        private void ShowLoadingOverlay(bool show, string message = "🚄 Processing...")
        {
            LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            if (show && LoadingOverlay.Children.Count > 0)
            {
                var stackPanel = LoadingOverlay.Children[0] as StackPanel;
                var textBlock = stackPanel?.Children[1] as TextBlock;
                if (textBlock != null)
                    textBlock.Text = message;
            }
        }

        private void ShowErrorMessage(string message)
        {
            StatusText.Text = $"Error: {message}";
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async Task ShowErrorMessageAsync(string message)
        {
            await Dispatcher.InvokeAsync(() => ShowErrorMessage(message));
        }

        private async Task ShowSuccessMessageAsync(string message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                StatusText.Text = message;
                MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        // Event handlers for booking engine events
        private async void OnBookingCompleted(object sender, BookingResult result)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                StatusText.Text = $"Booking completed: PNR {result.PNR}";
                await _notificationService.SendDesktopNotificationAsync(
                    "Booking Successful!", 
                    $"Your ticket is confirmed. PNR: {result.PNR}", 
                    NotificationType.Desktop);
                
                // Refresh booking history
                var history = await _bookingEngine.GetBookingHistoryAsync("default_user");
                BookingHistoryGrid.ItemsSource = history;
            });
        }

        private async void OnBookingFailed(object sender, BookingResult result)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                StatusText.Text = $"Booking failed: {string.Join(", ", result.Messages)}";
                await _notificationService.SendDesktopNotificationAsync(
                    "Booking Failed", 
                    $"Unable to complete booking: {string.Join(", ", result.Messages)}", 
                    NotificationType.Desktop);
            });
        }

        private async void OnBookingStatusChanged(object sender, string status)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                StatusText.Text = $"Status: {status}";
            });
        }

        // Window controls
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 