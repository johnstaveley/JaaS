using System.Speech.Synthesis;

namespace JaaS.Demos;

public class D1_WindowsSpeechSynthesiserDemo
{
    private SpeechSynthesizer? _speechSynthesizerWindows;

    [SetUp]
    public void Setup()
    {
        _speechSynthesizerWindows = new SpeechSynthesizer();
        _speechSynthesizerWindows.SetOutputToDefaultAudioDevice();
        _speechSynthesizerWindows.Rate = 0;
    }

    [TearDown]
    public void Teardown()
    {
        if (_speechSynthesizerWindows != null)
        {
            _speechSynthesizerWindows.Dispose();
        }
    }

    [Test]
    [TestCase("David", "Hello and welcome to Leeds Sharp")]
    [TestCase("Hazel", "Hello and welcome to I O T North")]
    [TestCase("Zira", "Hello and welcome to Leeds Data Science meet up")]
    public void SayWithVoice(string chosenVoice, string speechText)
    {
        // Arrange
        Assert.That(_speechSynthesizerWindows, Is.Not.Null);
        ChooseVoice(chosenVoice);

        // Act
        _speechSynthesizerWindows.Speak(speechText);

        // Assert
        Assert.Pass();
    }

    private void ChooseVoice(string voice)
    {
        Assert.That(_speechSynthesizerWindows, Is.Not.Null);
        var installedVoices = _speechSynthesizerWindows.GetInstalledVoices().ToList();
        var chosenVoice = installedVoices.FirstOrDefault(x => x.VoiceInfo.Name == $"Microsoft {voice} Desktop");
        if (chosenVoice != null)
        {
            _speechSynthesizerWindows.SelectVoice(chosenVoice.VoiceInfo.Name);
        }
    }

}