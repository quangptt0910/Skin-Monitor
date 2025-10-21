using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Vision;
using SkinMonitor.Models;
using System.Text.Json;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using SkinMonitor.Models;
using SkiaSharp;



namespace SkinMonitor.Services;

public interface IAIAnalysisService
{
    Task<WoundAnalysisResult> AnalyzeWoundAsync(int woundId, List<WoundPhoto> woundPhotos, string imagePath);
    Task<double> CalculateWoundAreaAsync(string imagePath);
    Task<InfectionRiskAssessment> AssessInfectionRiskAsync(string imagePath, HealingStageLog? latestLog = null);
    Task<HealingPrediction> PredictHealingProgressAsync(int woundId, List<WoundPhoto> photos);
    Task<WoundClassification> ClassifyWoundTypeAsync(string imagePath);
}

public class AIAnalysisOptions
{
    public string ModelsPath { get; set; } = "Models";
    public bool EnableModelCaching { get; set; } = true;
    public double DefaultConfidenceThreshold { get; set; } = 0.5;
    public int MaxImageSizeMB { get; set; } = 10;
}

public class AIAnalysisService : IAIAnalysisService, IDisposable
{
    private readonly MLContext _mlContext;
    private readonly ILogger<AIAnalysisService> _logger;
    private readonly AIAnalysisOptions _options;
    private ITransformer? _woundClassificationModel;
    private ITransformer? _infectionDetectionModel;
    private bool _disposed = false;
    private readonly IWoundRepository _woundRepository;

    public AIAnalysisService(
        ILogger<AIAnalysisService> logger,
        IOptions<AIAnalysisOptions> options,
        IWoundRepository woundRepository)
    {
        _logger = logger;
        _options = options.Value;
        _woundRepository = woundRepository;

        _mlContext = new MLContext(seed: 0);
        // Initialize models
        _ = Task.Run(LoadModels);
    }

    private async Task LoadModels()
    {
        try
        {
            _logger.LogInformation("Loading AI models...");

            // Load pre-trained models (these would be trained separately)
            await LoadWoundClassificationModel();
            await LoadInfectionDetectionModel();
            
            _logger.LogInformation("AI models loaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load AI models");
        }
    }

    private async Task LoadWoundClassificationModel()
    {
        try
        {
            //TODO: Load actual ONNX model when available
            // In a real implementation, you would load a pre-trained ONNX model

            var modelPath = Path.Combine(_options.ModelsPath, "wound_classification.onnx");

            if (File.Exists(modelPath))
            {
                _logger.LogInformation("Wound classification model found at {ModelPath}", modelPath);
            }
            else
            {
                _logger.LogWarning("Wound classification model not found at");
            }
            await Task.Delay(100).ConfigureAwait(false); // Simulate loading time
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load wound classification model");
        }
        
    }

    private async Task LoadInfectionDetectionModel()
    {
        try
        {
            var modelPath = Path.Combine(_options.ModelsPath, "infection_detection.onnx");
            
            if (File.Exists(modelPath))
            {
                // _infectionDetectionModel = _mlContext.Model.Load(modelPath, out _);
                _logger.LogInformation("Infection detection model loaded");
            }
            else
            {
                _logger.LogWarning("Infection detection model not found at {ModelPath}", modelPath);
            }
            
            await Task.Delay(100).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load infection detection model");
        }
    }

    public async Task<WoundAnalysisResult> AnalyzeWoundAsync(int woundId, List<WoundPhoto> woundPhotos, string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            throw new ArgumentException("Image path cannot be null or empty", nameof(imagePath));

        if (!File.Exists(imagePath))
            throw new FileNotFoundException($"Image file not found: {imagePath}");
        
        try
        {
            _logger.LogInformation("Starting wound analysis for image: {ImagePath}", imagePath);

            // Validate image size
            var fileInfo = new FileInfo(imagePath);
            if (fileInfo.Length > _options.MaxImageSizeMB * 1024 * 1024)
            {
                throw new InvalidOperationException($"Image file too large: {fileInfo.Length / (1024 * 1024)}MB. Maximum allowed: {_options.MaxImageSizeMB}MB");
            }
            
            var areaTask = CalculateWoundAreaAsync(imagePath);
            var classificationTask = ClassifyWoundTypeAsync(imagePath);
            var infectionRiskTask = AssessInfectionRiskAsync(imagePath);

            await Task.WhenAll(areaTask, classificationTask, infectionRiskTask);
            
            var area = await areaTask;
            var classification = await classificationTask;
            var infectionRisk = await infectionRiskTask;
            var healingPrediction = await PredictHealingProgressAsync(woundId, woundPhotos);

            var result = new WoundAnalysisResult
            {
                WoundAreaCm2 = area,
                Classification = classification,
                InfectionRisk = infectionRisk,
                AnalysisDate = DateTime.UtcNow,
                ConfidenceScore = CalculateOverallConfidence(classification, infectionRisk),
                HealingPrediction = healingPrediction

            };
            _logger.LogInformation("Wound analysis completed successfully. Area: {Area}cm², Confidence: {Confidence:P2}", 
                area, result.ConfidenceScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing wound image: {ImagePath}", imagePath);
            return new WoundAnalysisResult
            {
                AnalysisDate = DateTime.Now,
                ConfidenceScore = 0.0,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<double> CalculateWoundAreaAsync(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            throw new ArgumentException("Image path cannot be null or empty", nameof(imagePath));

        if (!File.Exists(imagePath))
            throw new FileNotFoundException($"Image file not found: {imagePath}");
        
        try
        {
            // Placeholder implementation using image processing
            // In a real app, this would use computer vision to detect wound edges
            // and calculate the actual area based on a reference object
            _logger.LogDebug("Calculating wound area for image: {ImagePath}", imagePath);

            using var bitmap = SkiaSharp.SKBitmap.Decode(imagePath);
            if (bitmap == null) 
            {
                _logger.LogWarning("Failed to decode image: {ImagePath}", imagePath);
                return 0.0;
            }

            // Simulate area calculation based on image analysis
            // This would involve edge detection, segmentation, and measurement
            var simulatedArea = Random.Shared.NextDouble() * 10.0; // 0-10 cm²
            
            await Task.Delay(500).ConfigureAwait(false); // Simulate processing time
            var result = Math.Round(simulatedArea, 2);
            _logger.LogDebug("Calculated wound area: {Area}cm² for image: {ImagePath}", result, imagePath);
        
            return result;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error calculating wound area for image: {ImagePath}", imagePath);
            return 0.0;
        }
    }

    public async Task<InfectionRiskAssessment> AssessInfectionRiskAsync(string imagePath, HealingStageLog? latestLog = null)
{
    if (string.IsNullOrEmpty(imagePath))
        throw new ArgumentException("Image path cannot be null or empty", nameof(imagePath));

    if (!File.Exists(imagePath))
        throw new FileNotFoundException($"Image file not found: {imagePath}");

    try
    {
        _logger.LogDebug("Assessing infection risk for image: {ImagePath}", imagePath);

        await Task.Delay(300).ConfigureAwait(false); // Simulate processing time
        
        var riskFactors = new List<string>();
        var riskScore = 0.0;

        // Analyze based on healing stage log if available
        if (latestLog != null)
        {
            if (latestLog.HasRedness)
            {
                riskFactors.Add("Redness detected");
                riskScore += 0.3;
            }
            
            if (latestLog.HasSwelling)
            {
                riskFactors.Add("Swelling present");
                riskScore += 0.2;
            }
            
            if (latestLog.HasDrainage && latestLog.DrainageType == "Pus")
            {
                riskFactors.Add("Purulent drainage");
                riskScore += 0.4;
            }
        }

        // Simulate image-based analysis
        var imageRisk = Random.Shared.NextDouble() * 0.5;
        riskScore += imageRisk;
        riskScore = Math.Min(riskScore, 1.0);

        var result = new InfectionRiskAssessment
        {
            RiskScore = riskScore,
            RiskLevel = GetRiskLevel(riskScore),
            RiskFactors = riskFactors,
            Recommendations = GenerateRecommendations(riskScore, riskFactors)
        };

        _logger.LogDebug("Infection risk assessment completed. Risk level: {RiskLevel}, Score: {Score:F2}", 
            result.RiskLevel, result.RiskScore);

        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error assessing infection risk for image: {ImagePath}", imagePath);
        
        return new InfectionRiskAssessment
        {
            RiskScore = 0.0,
            RiskLevel = "Unknown",
            RiskFactors = new List<string> { $"Analysis error: {ex.Message}" },
            Recommendations = new List<string> { "Please consult a healthcare professional" }
        };
    }
}

    public async Task<HealingPrediction> PredictHealingProgressAsync(int woundId, List<WoundPhoto> photos)
    {
        if (photos == null)
            throw new ArgumentNullException(nameof(photos));
        
        try
        {
            _logger.LogDebug("Predicting healing progress for wound {WoundId} with {PhotoCount} photos", 
                woundId, photos.Count);

            if (photos.Count < 2)
            {
                _logger.LogWarning("Insufficient photos for prediction. Need at least 2, got {Count}", photos.Count);
                
                return new HealingPrediction
                {
                    PredictedHealingDays = 0,
                    ConfidenceLevel = 0.0,
                    TrendAnalysis = "Insufficient data for prediction"
                };
            }

            await Task.Delay(400).ConfigureAwait(false); // Simulate processing time

            // Analyze healing trend based on wound area changes
            var photosWithArea = photos.Where(p => p.WoundAreaCm2.HasValue)
                                      .OrderBy(p => p.DateTaken)
                                      .ToList();

            if (photosWithArea.Count < 2)
            {
                return new HealingPrediction
                {
                    PredictedHealingDays = 0,
                    ConfidenceLevel = 0.0,
                    TrendAnalysis = "No area measurements available"
                };
            }

            var areas = photosWithArea.Select(p => p.WoundAreaCm2!.Value).ToList();
            
            // Calculate healing rate
            var timeSpan = photosWithArea.Last().DateTaken - photosWithArea.First().DateTaken;
            var areaReduction = areas.First() - areas.Last();
            var dailyReductionRate = areaReduction / Math.Max(timeSpan.TotalDays, 1);

            var remainingArea = areas.Last();
            var predictedDays = remainingArea / Math.Max(dailyReductionRate, 0.001);

            var result = new HealingPrediction
            {
                PredictedHealingDays = Math.Max(0, (int)Math.Round(predictedDays)),
                ConfidenceLevel = CalculatePredictionConfidence(areas),
                TrendAnalysis = AnalyzeTrend(areas),
                DailyReductionRate = dailyReductionRate
            };

            _logger.LogDebug("Healing prediction completed for wound {WoundId}. Predicted days: {Days}, Confidence: {Confidence:P2}", 
                woundId, result.PredictedHealingDays, result.ConfidenceLevel);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting healing progress for wound {WoundId}", woundId);
            
            return new HealingPrediction
            {
                PredictedHealingDays = 0,
                ConfidenceLevel = 0.0,
                TrendAnalysis = $"Prediction error: {ex.Message}"
            };
        }
    }

    
    public async Task<WoundClassification> ClassifyWoundTypeAsync(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            throw new ArgumentException("Image path cannot be null or empty", nameof(imagePath));

        if (!File.Exists(imagePath))
            throw new FileNotFoundException($"Image file not found: {imagePath}");

        try
        {
            _logger.LogDebug("Classifying wound type for image: {ImagePath}", imagePath);

            await Task.Delay(200).ConfigureAwait(false); // Simulate processing time
        
            // Placeholder implementation - would use actual ML model in production
            var woundTypes = Enum.GetValues<WoundType>().Cast<WoundType>().ToArray();
            var randomType = woundTypes[Random.Shared.Next(woundTypes.Length)];
            var confidence = 0.6 + Random.Shared.NextDouble() * 0.3; // 60-90%

            var result = new WoundClassification
            {
                PrimaryType = randomType,
                Confidence = confidence,
                AlternativeTypes = woundTypes.Where(t => t != randomType)
                    .Take(2)
                    .ToDictionary(t => t, t => Random.Shared.NextDouble() * 0.4)
            };

            _logger.LogDebug("Wound classification completed. Type: {Type}, Confidence: {Confidence:P2}", 
                result.PrimaryType, result.Confidence);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying wound type for image: {ImagePath}", imagePath);
        
            return new WoundClassification
            {
                PrimaryType = WoundType.Other,
                Confidence = 0.0,
                AlternativeTypes = new Dictionary<WoundType, double>(),
                ErrorMessage = ex.Message
            };
        }
    }

    private double CalculateOverallConfidence(WoundClassification classification, InfectionRiskAssessment infectionRisk)
    {
        return (classification.Confidence + (1.0 - Math.Abs(infectionRisk.RiskScore - 0.5))) / 2.0;
    }

    private string GetRiskLevel(double riskScore)
    {
        return riskScore switch
        {
            >= 0.7 => "High",
            >= 0.4 => "Moderate",
            >= 0.2 => "Low",
            _ => "Very Low"
        };
    }

    private List<string> GenerateRecommendations(double riskScore, List<string> riskFactors)
    {
        var recommendations = new List<string>();

        if (riskScore >= 0.7)
        {
            recommendations.Add("Seek immediate medical attention");
            recommendations.Add("Monitor for worsening symptoms");
        }
        else if (riskScore >= 0.4)
        {
            recommendations.Add("Consult with healthcare provider");
            recommendations.Add("Increase monitoring frequency");
        }
        else
        {
            recommendations.Add("Continue current care routine");
            recommendations.Add("Monitor for changes");
        }

        if (riskFactors.Contains("Purulent drainage"))
        {
            recommendations.Add("Clean wound thoroughly");
        }

        return recommendations;
    }

    private double CalculatePredictionConfidence(List<double> areas)
    {
        if (areas.Count < 3) return 0.5;
        
        // Calculate consistency of healing trend
        var differences = new List<double>();
        for (int i = 1; i < areas.Count; i++)
        {
            differences.Add(areas[i-1] - areas[i]);
        }
        
        var avgDifference = differences.Average();
        var variance = differences.Select(d => Math.Pow(d - avgDifference, 2)).Average();
        var consistency = 1.0 / (1.0 + variance);
        
        return Math.Min(0.95, Math.Max(0.1, consistency));
    }

    private string AnalyzeTrend(List<double> areas)
    {
        if (areas.Count < 2) return "Insufficient data";
        
        var totalReduction = areas.First() - areas.Last();
        var reductionPercentage = (totalReduction / areas.First()) * 100;
        
        return reductionPercentage switch
        {
            >= 30 => "Excellent healing progress",
            >= 15 => "Good healing progress", 
            >= 5 => "Moderate healing progress",
            >= 0 => "Slow healing progress",
            _ => "Wound appears to be getting larger"
        };
    }
    
    
    public void Dispose()
    {
        if (!_disposed)
        {
            // _woundClassificationModel?.Dispose();
            // _infectionDetectionModel?.Dispose();
            // _mlContext?.Dispose();
            
            _disposed = true;
            _logger.LogInformation("AIAnalysisService disposed");
        }
    }
}
