// using SkinMonitor.Models;
//
// namespace SkinMonitor.Services;
//
// public partial interface IAIAnalysisService
// {
//     Task<WoundAnalysisResult> AnalyzeWoundAsync(int woundId, List<WoundPhoto> photos, string primaryImagePath);
//     Task<double> CalculateWoundAreaAsync(string imagePath);
//     Task<InfectionRiskAssessment> AssessInfectionRiskAsync(string imagePath, HealingStageLog? latestLog = null);
//     Task<HealingPrediction> PredictHealingProgressAsync(int woundId, List<WoundPhoto> photos);
//     Task<WoundClassification> ClassifyWoundTypeAsync(string imagePath);
// }