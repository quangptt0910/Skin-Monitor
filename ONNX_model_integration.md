AI Model Integration Guide (ONNX)

This guide details how to integrate the powerful ONNX-based AIAnalysisService into the SkinMonitor application.

1. Get the ONNX Models

This implementation requires two specific ONNX models. You will need to find or train models that perform these functions and name them accordingly:

Classification Model: efficientnet_b0_wound_classifier.onnx

This model should take a [1, 3, 224, 224] float tensor as input. The image data should be normalized based on ImageNet standards.

It should output a raw score (logit) tensor of probabilities corresponding to the WoundType enum. The code includes a Softmax function to convert these scores to probabilities.

Segmentation Model: unet_wound_segmentation.onnx

This model should also take a [1, 3, 224, 224] float tensor as input (ImageNet normalized).

It should output a segmentation mask, ideally a [1, 1, 224, 224] float tensor where pixel values > 0.5 represent the wound.

2. Add Models to Your Project

In the Solution Explorer, navigate to the Resources/Raw folder. If it doesn't exist, create it.

Drag and drop your two .onnx model files into this folder.

Right-click on each model file (.onnx) and select Properties.

Set the Build Action to MauiAsset. This is a critical step that ensures the models are bundled with your application.

3. Install Necessary NuGet Packages

Your project requires the ONNX Runtime for inference and a powerful image processing library.

Right-click on the SkinMonitor project in the Solution Explorer and select Manage NuGet Packages....

Go to the Browse tab and search for and install the following packages:

Microsoft.ML.OnnxRuntime

SixLabors.ImageSharp

4. Register the Service for Dependency Injection

To make the AIAnalysisService available throughout your app, you must register it with the service container.

Open the MauiProgram.cs file.

In the CreateMauiApp method, add the following line along with your other service registrations:

// ... other services
builder.Services.AddSingleton<IAIAnalysisService, AIAnalysisService>();
// ...


By registering the IAIAnalysisService interface, you are following best practices for dependency injection, making your application more modular and easier to test. Your app is now fully configured to use the advanced, dual-model AI analysis service.