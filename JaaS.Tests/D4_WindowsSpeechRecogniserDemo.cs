using Azure;
using System.Speech.Recognition;

namespace JaaS.Demos;

public class D4_WindowsSpeechRecogniserDemo
{
    private SpeechRecognitionEngine? _speechRecognizerWindows;
    private string _response;
    private float _confidence;

    [SetUp]
    public void Setup()
    {
        _response = "";
        _speechRecognizerWindows = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-GB"));
        _speechRecognizerWindows.SetInputToDefaultAudioDevice();
        GrammarBuilder builder = new GrammarBuilder();
        builder.Culture = _speechRecognizerWindows.RecognizerInfo.Culture;
        builder.Append("hello");
        var grammar = new Grammar(builder);
        _speechRecognizerWindows.LoadGrammar(grammar);
        _speechRecognizerWindows.BabbleTimeout = TimeSpan.FromSeconds(3);
        _speechRecognizerWindows.InitialSilenceTimeout = TimeSpan.FromSeconds(5);
        _speechRecognizerWindows.RecognizeCompleted += WindowsRecognizeCompleted;
    }

    [Test]
    [TestCase("Hello")]
    public async Task RecogniseVoice(string expectedWord)
    {
        // Arrange
        Assert.IsNotNull(_speechRecognizerWindows);

        // Act
        _speechRecognizerWindows.RecognizeAsync();
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(1000);
        }

        // Assert
        Assert.That(_response, Is.EqualTo(expectedWord), $"Found {_response} with confidence {_confidence * 100}");
    }
    private void WindowsRecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
    {
        if (e.Result != null)
        {
            _response = e.Result.Text;
            _confidence = e.Result.Confidence;
        }
    }
}