using JaaS.Models;
using Microsoft.CognitiveServices.Speech;

namespace JaaS.Demos;

public class D3_NeuralVoiceSpeechSynthesiserDemo
{
    private AppConfiguration _configuration;
    SpeechConfig azureSpeechConfig = null;
    private SpeechSynthesizer? _speechSynthesizerNeural;

    [SetUp]
    public void Setup()
    {
        _configuration = new AppConfiguration();
        azureSpeechConfig = SpeechConfig.FromSubscription(_configuration.AzureSpeechSubscriptionKey, _configuration.AzureSpeechRegion);
        azureSpeechConfig.SpeechRecognitionLanguage = "en-GB";
        azureSpeechConfig.SpeechSynthesisLanguage = "en-GB";
        azureSpeechConfig.EndpointId = _configuration.AzureNeuralVoiceEndpointId;
        azureSpeechConfig.SpeechSynthesisVoiceName = _configuration.AzureNeuralVoiceName;
        _speechSynthesizerNeural = new SpeechSynthesizer(azureSpeechConfig);
    }

    [Test]
    [TestCase("Our speakers tonight are a round table of industry luminaries.")]
    [TestCase("We are currently looking for sponsors for the meetup, please contact us if you would like to sponsor")]
    [TestCase("We talk all about things .Net related")]
    public async Task SayWithVoice(string speechText)
    {
        // Arrange
        Assert.IsNotNull(_speechSynthesizerNeural);

        // Act
        var speechSynthesisResult = await _speechSynthesizerNeural.SpeakTextAsync(speechText);
        OutputSpeechSynthesisResult(speechSynthesisResult, speechText);

        // Assert
        Assert.Pass();
    }

    static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
    {
        switch (speechSynthesisResult.Reason)
        {
            case ResultReason.SynthesizingAudioCompleted:
                Assert.Pass($"Speech synthesized for text: [{text}]");
                break;
            case ResultReason.Canceled:
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Assert.Fail($"CANCELED: ErrorCode={cancellation.ErrorCode} ErrorDetails=[{cancellation.ErrorDetails}] Did you set the speech resource key and region values?");
                }
                break;
            default:
                break;
        }
    }
}