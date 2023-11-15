using System;
using System.Configuration;

namespace JaaS.Models;

public class AppConfiguration
{
    public string AzureOpenAiDeployment { get; set; }
    public string AzureOpenAiKey { get; set; }
    public string AzureOpenAiUrl { get; set; }
    public string AzureSpeechSubscriptionKey { get; set; }
    public string AzureSpeechRegion { get; set; }
    public SpeechStrategyKind SpeechRecognizerStrategy { get; set; }
    public SpeechStrategyKind SpeechSynthesiserStrategy { get; set; }
    public bool UseOpenAi => !string.IsNullOrEmpty(AzureOpenAiKey) && AzureOpenAiKey != "CHANGEME" &&
        !string.IsNullOrEmpty(AzureOpenAiUrl) && AzureOpenAiUrl != "CHANGEME" &&
        !string.IsNullOrEmpty(AzureOpenAiDeployment) && AzureOpenAiDeployment != "CHANGEME";


    public AppConfiguration()
    {
        AzureOpenAiDeployment = ConfigurationManager.AppSettings.Get("AzureOpenAiDeployment") ?? "";
        AzureOpenAiKey = ConfigurationManager.AppSettings.Get("AzureOpenAiKey") ?? "";
        AzureOpenAiUrl = ConfigurationManager.AppSettings.Get("AzureOpenAiUrl") ?? "";
        AzureSpeechSubscriptionKey = ConfigurationManager.AppSettings.Get("AzureSpeechSubscriptionKey") ?? "";
        AzureSpeechRegion = ConfigurationManager.AppSettings.Get("AzureSpeechRegion") ?? "";
        SpeechRecognizerStrategy = (SpeechStrategyKind) Enum.Parse(typeof(SpeechStrategyKind), ConfigurationManager.AppSettings.Get("SpeechRecognizerStrategy") ?? "", true);
        SpeechSynthesiserStrategy = (SpeechStrategyKind) Enum.Parse(typeof(SpeechStrategyKind), ConfigurationManager.AppSettings.Get("SpeechSynthesiserStrategy") ?? "", true);
    }
}
