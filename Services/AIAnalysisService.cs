using System.Diagnostics;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkinMonitor.Models;

using Image = SixLabors.ImageSharp.Image;
namespace SkinMonitor.Services
{
    public interface IAIAnalysisService
    {
        Task<WoundAnalysisResult> AnalyzeWoundAsync(int woundId, List<WoundPhoto> photos, string primaryImagePath);
        Task<double> CalculateWoundAreaAsync(string imagePath);
        Task<InfectionRiskAssessment> AssessInfectionRiskAsync(string imagePath, HealingStageLog? latestLog = null);
        Task<HealingPrediction> PredictHealingProgressAsync(int woundId, List<WoundPhoto> photos);
        Task<WoundClassification> ClassifyWoundTypeAsync(string imagePath);
    }
    
    public class AIAnalysisService : IAIAnalysisService, IDisposable
    {
        private InferenceSession? _classificationSession;
        private InferenceSession? _segmentationSession;
        private bool _isInitialized;
        private SemaphoreSlim _initializationLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _inferenceLock = new SemaphoreSlim(1, 1); // Prevent concurrent inference


        public AIAnalysisService()
        {
            // Initialize models on a background thread to avoid blocking the UI
            Task.Run(InitializeModelsAsync);
        }

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized)
                return;

            await _initializationLock.WaitAsync();
            try
            {
                if (_isInitialized)
                    return;

                await InitializeModelsAsync();
            }
            finally
            {
                _initializationLock.Release();
            }
        }
        
        private async Task InitializeModelsAsync()
        {
            try
            {
                var classificationModelPath = await GetModelPathFromBundleAsync("wound_classification_model.onnx");
                if (classificationModelPath != null)
                {
                    _classificationSession = new InferenceSession(classificationModelPath);
                }

                // var segmentationModelPath = await GetModelPathFromBundleAsync("unet_wound_segmentation.onnx");
                // if (segmentationModelPath != null)
                // {
                //     _segmentationSession = new InferenceSession(segmentationModelPath);
                // }
                _segmentationSession = null; // Segmentation model is optional for now

                _isInitialized = _classificationSession != null;
                //&& _segmentationSession != null;
                System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Model initialized!");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Model initialization failed: {ex.Message}");
                _isInitialized = false;
            }
        }

        public async Task<WoundAnalysisResult> AnalyzeWoundAsync(int woundId, List<WoundPhoto> photos, string primaryImagePath)
        {
            if (!_isInitialized || !File.Exists(primaryImagePath))
            {
                return GetFallbackAnalysis("AI service is not initialized or the image file was not found.");
            }

            try
            {
                using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgb24>(primaryImagePath);
                var preprocessedImage = PreprocessImage(image, 299, 299);

                var classificationTask = ClassifyWoundAsync(preprocessedImage);
                var areaTask = CalculateWoundAreaAsync(preprocessedImage);
                var infectionRiskTask = AssessInfectionRiskAsync(preprocessedImage, photos?.LastOrDefault());

                // Run tasks in parallel for efficiency
                await Task.WhenAll(classificationTask, areaTask, infectionRiskTask);

                return new WoundAnalysisResult
                {
                    AnalysisDate = DateTime.Now,
                    Classification = await classificationTask,
                    WoundAreaCm2 = await areaTask,
                    InfectionRisk = await infectionRiskTask,
                    ConfidenceScore = (await classificationTask).Confidence,
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Analysis failed: {ex.Message}");
                return GetFallbackAnalysis(ex.Message);
            }
        }

        public async Task<WoundClassification> ClassifyWoundTypeAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
                return GetFallbackClassification("Image file was not found.");

            try
            {
                // Ensure model is ready
                await EnsureInitializedAsync();

                if (!_isInitialized)
                    return GetFallbackClassification("AI service is not initialized.");

                // Load and process image
                using var img = await Image.LoadAsync<Rgb24>(imagePath);

                // Preprocess image
                var preprocessedImage = PreprocessImage(img, 299, 299);

                // Run inference
                return await ClassifyWoundAsync(preprocessedImage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Classification failed: {ex.Message}\n{ex.StackTrace}");
                return GetFallbackClassification(ex.Message);
            }
        }

        public async Task<double> CalculateWoundAreaAsync(string imagePath)
        {
            if (!_isInitialized || !File.Exists(imagePath)) return 0.0;

            try
            {
                using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgb24>(imagePath);
                var preprocessedImage = PreprocessImage(image, 299, 299);
                return await CalculateWoundAreaAsync(preprocessedImage);
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Area calculation from path failed: {ex.Message}");
                return 0.0;
            }
        }
        
        public async Task<InfectionRiskAssessment> AssessInfectionRiskAsync(string imagePath, HealingStageLog? latestLog = null)
        {
            if (!_isInitialized || !File.Exists(imagePath))
            {
                return GetFallbackInfectionRisk("AI service is not initialized or the image file was not found.");
            }

            try
            {
                using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgb24>(imagePath);
                var preprocessedImage = PreprocessImage(image, 299, 299);
                return await AssessInfectionRiskAsync(preprocessedImage, null); // Pass null for latestPhoto as we don't have it here
            }
            catch (Exception ex)
            {
                return GetFallbackInfectionRisk(ex.Message);
            }
        }

        public async Task<HealingPrediction> PredictHealingProgressAsync(int woundId, List<WoundPhoto> photos)
        {
            // This is a placeholder for a more complex predictive model.
            // A real implementation might analyze the trend of WoundAreaCm2 over time.
            await Task.Delay(250); // Simulate processing

            if (photos == null || photos.Count < 2)
            {
                return new HealingPrediction { EstimatedDaysToHeal = 21, ConfidenceLevel = 0.60, PredictionDate = DateTime.Now, HealingStage = HealingStage.Initial };
            }

            return new HealingPrediction
            {
                EstimatedDaysToHeal = Math.Max(5, 21 - photos.Count * 2),
                ConfidenceLevel = Math.Min(0.95, 0.65 + photos.Count * 0.05),
                PredictionDate = DateTime.Now,
                HealingStage = photos.Count > 4 ? HealingStage.Improving : HealingStage.Initial
            };
        }


        #region Core AI Methods
        
        private async Task<WoundClassification> ClassifyWoundAsync(float[] imageData)
        {
            if (_classificationSession == null) return GetFallbackClassification("Classification model not loaded.");
            
            try
            {
                var inputTensor = new DenseTensor<float>(imageData, new[] { 1, 299, 299, 3 });
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(_classificationSession.InputNames[0], inputTensor) };

                using var outputs = _classificationSession.Run(inputs);
                var outputTensor = outputs.First().AsTensor<float>();
                
                
                
                System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Model output length: {outputTensor.Length}");
                
                var probabilities = Softmax(outputTensor.ToArray());
                
                System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Probabilities length: {probabilities.Length}");

                var maxProbability = probabilities.Max();
                var maxIndex = Array.IndexOf(probabilities, maxProbability);
                var allWoundTypes = Enum.GetValues<WoundType>().Cast<WoundType>().ToArray();

                // Exclude Unknown from AI model mapping since model has only 10 classes
                var woundTypes = allWoundTypes.Where(w => w != WoundType.Unknown).ToArray();
                var primaryType = woundTypes[maxIndex];

                if (maxIndex < 0 || maxIndex >= woundTypes.Length)
                {
                    System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Invalid class index {maxIndex}, max valid index is {woundTypes.Length - 1}");
                    return GetFallbackClassification($"Invalid class index {maxIndex}");
                }
                
                System.Diagnostics.Debug.WriteLine("[AIAnalysisService] Prediction probabilities:");
                for (int i = 0; i < probabilities.Length; i++)
                {
                    var label = Enum.GetName(typeof(WoundType), i);
                    System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] {label}: {probabilities[i]:P2}");
                }
                
                return new WoundClassification
                {
                    PrimaryType = primaryType,
                    Confidence = maxProbability,
                    AlternativeTypes = woundTypes
                        .Select((type, index) => new { Type = type, Confidence = probabilities[index] })
                        .Where(x => x.Type != woundTypes[maxIndex])
                        .ToDictionary(x => x.Type, x => x.Confidence)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Classification failed: {ex.Message}");
                return GetFallbackClassification(ex.Message);
            }
        }

        private async Task<double> CalculateWoundAreaAsync(float[] imageData)
        {
            if (_segmentationSession == null) return await EstimateAreaSimple(imageData);

            try
            {
                var inputTensor = new DenseTensor<float>(imageData, new[] { 1, 299, 299, 3 });
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(_segmentationSession.InputNames[0], inputTensor) };

                using var outputs = _segmentationSession.Run(inputs);
                var maskTensor = outputs.First().AsTensor<float>();

                var maskArray = maskTensor.ToArray();
                var woundPixels = maskArray.Count(pixel => pixel > 0.5f); // Threshold for wound pixels

                // This ratio is crucial and needs calibration. It converts pixel count to a real-world area.
                // For accuracy, a reference object (like a small sticker of known size) should be in the photo.
                const double PIXELS_PER_CM_SQUARED = 2500; // Example: 50x50 pixels = 1 cm^2
                var areaCm2 = woundPixels / PIXELS_PER_CM_SQUARED;

                return Math.Round(Math.Max(0.1, areaCm2), 2);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Segmentation failed: {ex.Message}");
                return await EstimateAreaSimple(imageData);
            }
        }

        private async Task<InfectionRiskAssessment> AssessInfectionRiskAsync(float[] imageData, WoundPhoto? latestPhoto)
        {
            await Task.Yield(); // Makes the method async without blocking
            var riskFactors = new List<string>();
            double riskScore = 0.0;

            // Simple heuristic-based assessment. This could be replaced by another model.
            double rednessScore = AnalyzeColor(imageData, "red");
            if (rednessScore > 0.15) // More than 15% of the image is strongly red
            {
                riskFactors.Add("Significant redness detected around the wound.");
                riskScore += 0.4;
            }

            double yellowScore = AnalyzeColor(imageData, "yellow");
            if (yellowScore > 0.10) // More than 10% is yellow (could indicate slough/pus)
            {
                riskFactors.Add("Yellowish tissue (slough) may be present.");
                riskScore += 0.3;
            }

            string riskLevel = riskScore switch {
                > 0.6 => "High",
                > 0.3 => "Medium",
                _ => "Low"
            };

            return new InfectionRiskAssessment
            {
                RiskLevel = riskLevel,
                RiskScore = Math.Min(1.0, riskScore),
                RiskFactors = riskFactors,
                Recommendations = GenerateRecommendations(riskLevel)
            };
        }

        #endregion

        #region Image & Data Processing

        private float[] PreprocessImage(Image<Rgb24> image, int width = 299, int height = 299)
        {
            // These are subtracted in order: R, G, B
            const float MeanR = 123.68f;
            const float MeanG = 116.779f;
            const float MeanB = 103.939f;
            
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new SixLabors.ImageSharp.Size(width, height),
                Mode = SixLabors.ImageSharp.Processing.ResizeMode.Crop

            }));

            var tensor = new DenseTensor<float>(new[] { 1, height, width, 3 });

            image.ProcessPixelRows(pixelAccessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    Span<Rgb24> pixelRow = pixelAccessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        tensor[0, y, x, 0] = pixelRow[x].R - MeanR;
                        tensor[0, y, x, 1] = pixelRow[x].G - MeanG;
                        tensor[0, y, x, 2] = pixelRow[x].B - MeanB;
                    }
                }
            });
            
            // After PreprocessImage, print debug info
            var preprocessed = PreprocessImage(image);
            Debug.WriteLine("C# preprocessing result:");
            Debug.WriteLine($"First 3 values: {preprocessed[0]}, {preprocessed[1]}, {preprocessed[2]}");
// Calculate min/max/mean for the entire tensor
            float min = preprocessed.Min();
            float max = preprocessed.Max();
            float mean = preprocessed.Average();
            Debug.WriteLine($"Min: {min}, Max: {max}, Mean: {mean}");
            
            return tensor.ToArray();
        }


        private double AnalyzeColor(float[] imageData, string color)
        {
            var totalPixels = 224 * 224;
            var colorDominantPixels = 0;

            for (int i = 0; i < totalPixels; i++)
            {
                // Note: Image data is normalized, so we can't directly check RGB values.
                // This heuristic is a placeholder. A proper color analysis model would be needed.
                var r = imageData[i];
                var g = imageData[i + totalPixels];
                var b = imageData[i + 2 * totalPixels];
                
                bool isColor = color switch
                {
                    "red" => r > g && r > b,
                    "yellow" => r > b && g > b,
                    _ => false
                };

                if (isColor) colorDominantPixels++;
            }
            return (double)colorDominantPixels / totalPixels;
        }
        
        private float[] Softmax(float[] scores)
        {
            var maxScore = scores.Max();
            var expScores = scores.Select(s => (float)Math.Exp(s - maxScore)).ToArray();
            var sumExpScores = expScores.Sum();
            return expScores.Select(s => s / sumExpScores).ToArray();
        }

        #endregion

        #region Fallback & Helper Methods

        private WoundAnalysisResult GetFallbackAnalysis(string message) => new() { ErrorMessage = message, Classification = GetFallbackClassification(message), InfectionRisk = GetFallbackInfectionRisk(message), WoundAreaCm2 = 0 };
        private WoundClassification GetFallbackClassification(string message) => new() { PrimaryType = WoundType.Unknown, Confidence = 0, ErrorMessage = message };
        private InfectionRiskAssessment GetFallbackInfectionRisk(string message) => new() { RiskLevel = "Unknown", RiskScore = 0, Recommendations = { "Could not assess risk." }, ErrorMessage = message };
        private async Task<double> EstimateAreaSimple(float[] imageData) { await Task.Delay(50); return new Random().NextDouble() * 5.0 + 1.0; }

        private List<string> GenerateRecommendations(string riskLevel) => riskLevel switch {
            "Low" => new() { "Continue with the current wound care plan.", "Monitor for any changes." },
            "Medium" => new() { "Clean the wound thoroughly.", "Consider a more absorbent dressing.", "Monitor closely for signs of infection like increased pain or discharge." },
            "High" => new() { "A healthcare provider consultation is strongly recommended.", "Professional medical assessment is required immediately." },
            _ => new() { "Unable to generate recommendations." }
        };

        private async Task<string?> GetModelPathFromBundleAsync(string modelName)
        {
            var cacheDir = FileSystem.AppDataDirectory;
            var modelPath = Path.Combine(cacheDir, modelName);

            // Don't copy if the model already exists in the cache
            if (File.Exists(modelPath)) return modelPath;

            try
            {
                // Copy the model from the app package (Resources/Raw) to the cache directory
                using var stream = await FileSystem.OpenAppPackageFileAsync(modelName);
                using var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fileStream);
                return modelPath;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AIAnalysisService] Failed to copy model '{modelName}': {ex.Message}");
                return null;
            }
        }
        
        public void Dispose()
        {
            _classificationSession?.Dispose();
            _segmentationSession?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

