using System.Configuration;

namespace JaaS.Models;

public class AppConfiguration
{
    public string? SpeechSubscriptionKey { get; set; }
    public string? SpeechRegion { get; set; }

    public AppConfiguration()
    {
        SpeechSubscriptionKey = ConfigurationSettings.AppSettings.Get("SpeechSubscriptionKey");
        SpeechRegion = ConfigurationSettings.AppSettings.Get("SpeechRegion");
    }
}
