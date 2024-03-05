using JaaS.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace JaaS.Demos;

public class D5_AzureSpeechRecogniserDemo
{
    private AppConfiguration _configuration;
    SpeechConfig azureSpeechConfig = null;
    private SpeechRecognizer? _speechRecognizerAzure;

    [SetUp]
    public void Setup()
    {
        _configuration = new AppConfiguration();
        azureSpeechConfig = SpeechConfig.FromSubscription(_configuration.AzureSpeechSubscriptionKey, _configuration.AzureSpeechRegion);
        azureSpeechConfig.SpeechRecognitionLanguage = "en-GB";
        _speechRecognizerAzure = new SpeechRecognizer(azureSpeechConfig, AudioConfig.FromDefaultMicrophoneInput());

    }

    [Test]
    [TestCase("Hello")]
    public async Task RecogniseVoice(string expectedWord)
    {
        // Arrange
        Assert.IsNotNull(_speechRecognizerAzure);
        var response = "";

        // Act
        var result = await _speechRecognizerAzure.RecognizeOnceAsync();
        if (result.Reason == ResultReason.RecognizedSpeech)
        {             
            response = result.Text.Trim('.');
        }

        // Assert
        Assert.That(response, Is.EqualTo(expectedWord));
    }
}