namespace SkinMonitor.Views;

[QueryProperty(nameof(WoundId), "woundId")]
public class PhotoCapturePage : ContentPage
{
    public string WoundId { get; set; } = string.Empty;
    private Image _previewImage;
    private Button _captureButton;

    public PhotoCapturePage()
    {
        Title = "Capture Photo";
        BackgroundColor = Colors.White;

        var titleLabel = new Label
        {
            Text = "Take Wound Photo",
            FontSize = 32,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var instructionLabel = new Label
        {
            Text = "• Ensure good lighting\n• Keep camera steady\n• Position wound clearly\n• Use reference object if needed",
            FontSize = 14,
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 10, 0, 20)
        };

        _previewImage = new Image
        {
            HeightRequest = 300,
            WidthRequest = 300,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Colors.LightGray,
            HorizontalOptions = LayoutOptions.Center
        };

        _captureButton = new Button
        {
            Text = "📷 Take Photo",
            BackgroundColor = Color.FromArgb("#512BD4"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(30, 15),
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 10)
        };
        _captureButton.Clicked += OnTakePhotoClicked;

        var selectButton = new Button
        {
            Text = "🖼️ Choose from Gallery",
            BackgroundColor = Color.FromArgb("#4CAF50"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(30, 15),
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10)
        };
        selectButton.Clicked += OnSelectPhotoClicked;

        var saveButton = new Button
        {
            Text = "Save Photo",
            BackgroundColor = Color.FromArgb("#2196F3"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(30, 15),
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
            IsVisible = false,
            Margin = new Thickness(0, 20, 0, 10)
        };
        saveButton.Clicked += OnSavePhotoClicked;

        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#512BD4"),
            BorderColor = Color.FromArgb("#512BD4"),
            BorderWidth = 2,
            CornerRadius = 8,
            Padding = new Thickness(30, 12),
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center
        };
        cancelButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(30),
                Spacing = 15,
                Children =
                {
                    titleLabel,
                    instructionLabel,
                    _previewImage,
                    _captureButton,
                    selectButton,
                    saveButton,
                    cancelButton
                }
            }
        };
    }

    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();
                _previewImage.Source = ImageSource.FromStream(() => stream);
                await DisplayAlert("Success", "Photo captured!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to take photo: {ex.Message}", "OK");
        }
    }

    private async void OnSelectPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();
                _previewImage.Source = ImageSource.FromStream(() => stream);
                await DisplayAlert("Success", "Photo selected!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to select photo: {ex.Message}", "OK");
        }
    }

    private async void OnSavePhotoClicked(object sender, EventArgs e)
    {
        // TODO: Implement actual save logic
        await DisplayAlert("Success", "Photo saved to wound record!", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
