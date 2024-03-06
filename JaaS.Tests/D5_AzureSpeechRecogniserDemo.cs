using JaaS.Demos.Utility;
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
    }

    [Test]
    [TestCase("Speakers", "speakers")]
    [TestCase("WhoAreTheSpeakers", "who are the speakers this week?")]
    public async Task RecogniseVoice(string file, string expectedWord)
    {
        // Arrange
        var bytes = FileUtility.LoadBytesFromEmbeddedResource($"JaaS.Demos.Resources.{file}.wav");
        var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(44100, 16, 1));
        audioInputStream.Write(bytes);
        _speechRecognizerAzure = new SpeechRecognizer(azureSpeechConfig, AudioConfig.FromStreamInput(audioInputStream));
        Assert.IsNotNull(_speechRecognizerAzure);
        var response = "";

        // Act
        var result = await _speechRecognizerAzure.RecognizeOnceAsync();
        if (result.Reason == ResultReason.RecognizedSpeech)
        {             
            response = result.Text.Trim('.').ToLower();
        }

        // Assert
        Assert.That(response, Is.EqualTo(expectedWord));
    }
}