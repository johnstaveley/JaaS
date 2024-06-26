using JaaS.Models;
using Microsoft.CognitiveServices.Speech;

namespace JaaS.Demos;

public class D2_AzureSpeechSynthesiserDemo
{
    private AppConfiguration _configuration;
    SpeechConfig azureSpeechConfig = null;
    private SpeechSynthesizer? _speechSynthesizerAzure;

    [SetUp]
    public void Setup()
    {
        _configuration = new AppConfiguration();
        azureSpeechConfig = SpeechConfig.FromSubscription(_configuration.AzureSpeechSubscriptionKey, _configuration.AzureSpeechRegion);
        azureSpeechConfig.SpeechSynthesisLanguage = "en-GB";
    }

    [TearDown]
    public void Teardown()
    {
        if (_speechSynthesizerAzure != null)
        {
            _speechSynthesizerAzure.Dispose();
        }
    }

    [Test]
    [TestCase("Oliver", "Our speakers tonight are a round table of industry luminaries.")]
    [TestCase("Sonia", "We are currently looking for sponsors for the meetup, please contact us if you would like to sponsor")]
    [TestCase("Alfie", "We talk all about things .Net, I O T or Data science related")]
    public async Task SayWithVoice(string chosenVoice, string speechText)
    {
        // Arrange
        ChooseVoice(chosenVoice);
        Assert.That(_speechSynthesizerAzure, Is.Not.Null);

        // Act
        await _speechSynthesizerAzure.SpeakTextAsync(speechText);

        // Assert
        Assert.Pass();
    }

    private void ChooseVoice(string voiceName)
    {
        // Microsoft Server Speech Text to Speech Voice (en-GB, [Voice]Neural) Where voice is one of: Thomas, Oliver, Ethan, Noah, Elliot, Alfie, Ryan
        azureSpeechConfig.SpeechSynthesisVoiceName = "Microsoft Server Speech Text to Speech Voice (en-GB, OliverNeural)";
        _speechSynthesizerAzure = new SpeechSynthesizer(azureSpeechConfig);
        using var voices = _speechSynthesizerAzure.GetVoicesAsync(azureSpeechConfig.SpeechSynthesisLanguage).Result;
        var voice = voices.Voices.FirstOrDefault(a => a.Name == $"Microsoft Server Speech Text to Speech Voice (en-GB, {voiceName}Neural)");
        azureSpeechConfig.SpeechSynthesisVoiceName = voice.Name;
        _speechSynthesizerAzure = new SpeechSynthesizer(azureSpeechConfig);
    }

}