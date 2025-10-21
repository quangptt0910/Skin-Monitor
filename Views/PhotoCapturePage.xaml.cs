using Microsoft.Maui.Media;

namespace SkinMonitor.Views;

[QueryProperty(nameof(WoundId), "woundId")]
public partial class PhotoCapturePage : ContentPage
{
    public string WoundId { get; set; } = string.Empty;

    public PhotoCapturePage()
    {
        InitializeComponent();
    }

    private async void OnCaptureClicked(object sender, EventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();
                    PreviewImage.Source = ImageSource.FromStream(() => stream);
                    await DisplayAlert("Success", "Photo captured successfully.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Camera not supported on this device.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSelectClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();
                PreviewImage.Source = ImageSource.FromStream(() => stream);
                await DisplayAlert("Success", "Photo loaded successfully.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
