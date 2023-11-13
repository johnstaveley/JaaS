using System;
using System.Configuration;

namespace JaaS.Models;

public class AppConfiguration
{
    public string? AzureSpeechSubscriptionKey { get; set; }
    public string? AzureSpeechRegion { get; set; }
    public SpeechStrategyKind SpeechRecognizerStrategy { get; set; }
    public SpeechStrategyKind SpeechSynthesiserStrategy { get; set; }


    public AppConfiguration()
    {
        SpeechRecognizerStrategy = (SpeechStrategyKind) Enum.Parse(typeof(SpeechStrategyKind), ConfigurationManager.AppSettings.Get("SpeechRecognizerStrategy"), true);
        SpeechSynthesiserStrategy = (SpeechStrategyKind) Enum.Parse(typeof(SpeechStrategyKind), ConfigurationManager.AppSettings.Get("SpeechRecognizerStrategy"), true);
        AzureSpeechSubscriptionKey = ConfigurationManager.AppSettings.Get("SpeechSubscriptionKey");
        AzureSpeechRegion = ConfigurationManager.AppSettings.Get("SpeechRegion");

    }
}
