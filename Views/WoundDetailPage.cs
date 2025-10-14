namespace SkinMonitor.Views;

[QueryProperty(nameof(WoundId), "woundId")]
public class WoundDetailPage : ContentPage
{
    private string _woundId = string.Empty;
    private Label _woundIdLabel;
    
    public string WoundId 
    { 
        get => _woundId;
        set
        {
            _woundId = value;
            _woundIdLabel.Text = $"Wound ID: {value}";
        }
    }

    public WoundDetailPage()
    {
        Title = "Wound Detail";
        BackgroundColor = Colors.White;

        _woundIdLabel = new Label
        {
            Text = "Loading...",
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center
        };

        var titleLabel = new Label
        {
            Text = "Wound Detail Page",
            FontSize = 32,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var backButton = new Button
        {
            Text = "Go Back",
            BackgroundColor = Color.FromArgb("#512BD4"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(20, 10),
            HorizontalOptions = LayoutOptions.Center
        };
        backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");

        var photosButton = new Button
        {
            Text = "View Photos",
            BackgroundColor = Color.FromArgb("#4CAF50"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(20, 10),
            HorizontalOptions = LayoutOptions.Center
        };
        photosButton.Clicked += async (s, e) => 
            await DisplayAlert("Photos", "Photos viewer coming soon!", "OK");

        var analysisButton = new Button
        {
            Text = "View Analysis",
            BackgroundColor = Color.FromArgb("#2196F3"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(20, 10),
            HorizontalOptions = LayoutOptions.Center
        };
        analysisButton.Clicked += async (s, e) => 
            await Shell.Current.GoToAsync($"Analysis?woundId={WoundId}");

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(30),
                Spacing = 20,
                Children =
                {
                    titleLabel,
                    _woundIdLabel,
                    new BoxView { HeightRequest = 1, Color = Colors.LightGray, Margin = new Thickness(0, 10) },
                    photosButton,
                    analysisButton,
                    backButton
                }
            }
        };
    }
}
