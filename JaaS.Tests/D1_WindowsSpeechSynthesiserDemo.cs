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

    [Test]
    [TestCase("David", "Hello and welcome to Leeds Sharp")]
    [TestCase("Hazel", "Hello and welcome to I O T North")]
    [TestCase("Zira", "Hello and welcome to Leeds Data Science meet up")]
    public async Task SayWithVoice(string chosenVoice, string speechText)
    {
        // Arrange
        Assert.IsNotNull(_speechSynthesizerWindows);
        ChooseVoice(chosenVoice);

        // Act
        _speechSynthesizerWindows.Speak(speechText);

        // Assert
        Assert.Pass();
    }

    private void ChooseVoice(string voice)
    {
        var installedVoices = _speechSynthesizerWindows.GetInstalledVoices().ToList();
        var chosenVoice = installedVoices.FirstOrDefault(x => x.VoiceInfo.Name == $"Microsoft {voice} Desktop");
        if (chosenVoice != null)
        {
            _speechSynthesizerWindows.SelectVoice(chosenVoice.VoiceInfo.Name);
        }
    }

}