namespace SkinMonitor.Views;

[QueryProperty(nameof(WoundId), "woundId")]
public class AnalysisPage : ContentPage
{
    public string WoundId { get; set; } = string.Empty;

    public AnalysisPage()
    {
        Title = "AI Analysis";
        BackgroundColor = Colors.White;

        var titleLabel = new Label
        {
            Text = "AI Wound Analysis",
            FontSize = 32,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var analysisCard = CreateAnalysisCard();
        var predictionsCard = CreatePredictionsCard();
        var recommendationsCard = CreateRecommendationsCard();

        var runAnalysisButton = new Button
        {
            Text = "🔬 Run AI Analysis",
            BackgroundColor = Color.FromArgb("#512BD4"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(30, 15),
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 10)
        };
        runAnalysisButton.Clicked += OnRunAnalysisClicked;

        var backButton = new Button
        {
            Text = "Go Back",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#512BD4"),
            BorderColor = Color.FromArgb("#512BD4"),
            BorderWidth = 2,
            CornerRadius = 8,
            Padding = new Thickness(30, 12),
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center
        };
        backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(30),
                Spacing = 20,
                Children =
                {
                    titleLabel,
                    analysisCard,
                    predictionsCard,
                    recommendationsCard,
                    runAnalysisButton,
                    backButton
                }
            }
        };
    }

    private Frame CreateAnalysisCard()
    {
        return new Frame
        {
            BackgroundColor = Colors.White,
            BorderColor = Colors.LightGray,
            CornerRadius = 10,
            Padding = new Thickness(20),
            HasShadow = true,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label { Text = "📊 Analysis Results", FontSize = 20, FontAttributes = FontAttributes.Bold },
                    new BoxView { HeightRequest = 1, Color = Colors.LightGray, Margin = new Thickness(0, 5) },
                    CreateInfoRow("Wound Area:", "12.5 cm²"),
                    CreateInfoRow("Infection Risk:", "Low (15%)", Color.FromArgb("#4CAF50")),
                    CreateInfoRow("Healing Stage:", "Proliferative"),
                    CreateInfoRow("Classification:", "Surgical Wound")
                }
            }
        };
    }

    private Frame CreatePredictionsCard()
    {
        return new Frame
        {
            BackgroundColor = Colors.White,
            BorderColor = Colors.LightGray,
            CornerRadius = 10,
            Padding = new Thickness(20),
            HasShadow = true,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label { Text = "🔮 Predictions", FontSize = 20, FontAttributes = FontAttributes.Bold },
                    new BoxView { HeightRequest = 1, Color = Colors.LightGray, Margin = new Thickness(0, 5) },
                    CreateInfoRow("Estimated Healing Time:", "14-21 days"),
                    CreateInfoRow("Progress Rate:", "Good"),
                    CreateInfoRow("Area Reduction:", "2.1 cm²/week")
                }
            }
        };
    }

    private Frame CreateRecommendationsCard()
    {
        return new Frame
        {
            BackgroundColor = Color.FromArgb("#E3F2FD"),
            BorderColor = Color.FromArgb("#2196F3"),
            CornerRadius = 10,
            Padding = new Thickness(20),
            HasShadow = true,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label { Text = "💡 Recommendations", FontSize = 20, FontAttributes = FontAttributes.Bold },
                    new BoxView { HeightRequest = 1, Color = Color.FromArgb("#2196F3"), Margin = new Thickness(0, 5) },
                    new Label { Text = "• Continue current care routine", FontSize = 14 },
                    new Label { Text = "• Monitor for changes in redness", FontSize = 14 },
                    new Label { Text = "• Take daily photos for tracking", FontSize = 14 },
                    new Label { Text = "• Keep wound clean and dry", FontSize = 14 }
                }
            }
        };
    }

    private HorizontalStackLayout CreateInfoRow(string label, string value, Color? valueColor = null)
    {
        return new HorizontalStackLayout
        {
            Spacing = 10,
            Children =
            {
                new Label 
                { 
                    Text = label, 
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 150
                },
                new Label 
                { 
                    Text = value,
                    TextColor = valueColor ?? Colors.Black,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };
    }

    private async void OnRunAnalysisClicked(object sender, EventArgs e)
    {
        await DisplayAlert("AI Analysis", 
            "Running AI analysis on latest wound photo...\n\nThis feature will use ML.NET for:\n" +
            "• Wound area measurement\n" +
            "• Infection detection\n" +
            "• Healing progress prediction\n" +
            "• Treatment recommendations", 
            "OK");
    }
}
