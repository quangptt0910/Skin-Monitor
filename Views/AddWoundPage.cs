namespace SkinMonitor.Views;

public class AddWoundPage : ContentPage
{
    private Entry _woundNameEntry;
    private Entry _bodyLocationEntry;
    private Picker _woundTypePicker;

    public AddWoundPage()
    {
        Title = "Add New Wound";
        BackgroundColor = Colors.White;

        var titleLabel = new Label
        {
            Text = "Add New Wound",
            FontSize = 32,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };

        _woundNameEntry = new Entry
        {
            Placeholder = "Wound Name (e.g., Knee Scrape)",
            FontSize = 16,
            Margin = new Thickness(0, 10)
        };

        _bodyLocationEntry = new Entry
        {
            Placeholder = "Body Location (e.g., Left Knee)",
            FontSize = 16,
            Margin = new Thickness(0, 10)
        };

        _woundTypePicker = new Picker
        {
            Title = "Select Wound Type",
            FontSize = 16,
            Margin = new Thickness(0, 10)
        };
        _woundTypePicker.Items.Add("Surgical");
        _woundTypePicker.Items.Add("Burn");
        _woundTypePicker.Items.Add("Diabetic");
        _woundTypePicker.Items.Add("Traumatic");
        _woundTypePicker.Items.Add("Pressure");
        _woundTypePicker.Items.Add("Other");

        var notesEditor = new Editor
        {
            Placeholder = "Additional notes...",
            HeightRequest = 100,
            FontSize = 16,
            Margin = new Thickness(0, 10)
        };

        var saveButton = new Button
        {
            Text = "Save Wound",
            BackgroundColor = Color.FromArgb("#512BD4"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(20, 12),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 20, 0, 10)
        };
        saveButton.Clicked += OnSaveClicked;

        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#512BD4"),
            BorderColor = Color.FromArgb("#512BD4"),
            BorderWidth = 2,
            CornerRadius = 8,
            Padding = new Thickness(20, 12),
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Fill
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
                    new Label { Text = "Wound Name", FontAttributes = FontAttributes.Bold },
                    _woundNameEntry,
                    new Label { Text = "Body Location", FontAttributes = FontAttributes.Bold },
                    _bodyLocationEntry,
                    new Label { Text = "Wound Type", FontAttributes = FontAttributes.Bold },
                    _woundTypePicker,
                    new Label { Text = "Notes", FontAttributes = FontAttributes.Bold },
                    notesEditor,
                    saveButton,
                    cancelButton
                }
            }
        };
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_woundNameEntry.Text))
        {
            await DisplayAlert("Validation Error", "Please enter a wound name", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(_bodyLocationEntry.Text))
        {
            await DisplayAlert("Validation Error", "Please enter a body location", "OK");
            return;
        }

        // TODO: Implement actual save logic with database
        await DisplayAlert("Success", "Wound saved successfully!", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
