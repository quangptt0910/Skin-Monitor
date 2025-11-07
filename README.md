# SkinMonitor

A .NET MAUI mobile application for wound monitoring and classification using AI.

## Version 0.0.1

### Features

- **AI-Powered Wound Classification**: Automatically classify wound types from photos using a ResNet50-based ONNX model
  - Supports 10 wound types: Abrasions, Bruises, Burns, Cuts, Diabetic Wounds, Lacerations, Normal, Pressure Wounds, Surgical Wounds, Venous Wounds
  - High-confidence predictions with confidence scores
- **Photo Management**: Capture or pick photos from gallery
- **Wound Tracking**: Create and manage wound records with name, location, type, and notes
- **Local Storage**: SQLite database for offline data persistence

### Tech Stack

- **.NET MAUI**: Cross-platform mobile framework
- **ONNX Runtime**: AI model inference
- **ImageSharp**: Image processing
- **SQLite**: Local database
- **MVVM Pattern**: Clean architecture with CommunityToolkit.Mvvm

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or JetBrains Rider
- Android SDK (for Android deployment)
- Xcode (for iOS deployment, macOS only)

### Getting Started

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SkinMonitor
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Add the ONNX model**
   - Place `wound_classification_model.onnx` in the `Resources/Raw` folder
   - The model will be automatically copied to app storage on first run

4. **Run the application**
   ```bash
   dotnet build -t:Run -f net8.0-android
   ```
   or use Visual Studio/Rider to build and deploy

### Model Information

The app uses a ResNet50-based model trained on wound classification:
- **Input**: 299×299 RGB images
- **Preprocessing**: Caffe mode (BGR channel order, ImageNet mean subtraction)
- **Output**: 10-class probability distribution

### Project Structure

```
SkinMonitor/
├── Models/           # Data models (Wound, WoundPhoto, etc.)
├── ViewModels/       # MVVM view models
├── Views/            # XAML pages
├── Services/         # Business logic (AI, Repository)
├── Converters/       # Value converters
└── Resources/        # Images, styles, ONNX models
```

### Known Limitations (v0.0.1)

- AI classification only (no area calculation or infection risk assessment yet)
- Single photo per wound
- No healing progress tracking
- No data export/import

### Roadmap

- [ ] Wound area calculation using segmentation
- [ ] Infection risk assessment
- [ ] Multi-photo support per wound
- [ ] Healing progress tracking over time
- [ ] Data export to PDF/CSV
- [ ] Cloud sync

