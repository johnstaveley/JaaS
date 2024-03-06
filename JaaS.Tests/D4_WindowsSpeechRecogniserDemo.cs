using JaaS.Demos.Utility;
using System.Speech.AudioFormat;
using System.Speech.Recognition;

namespace JaaS.Demos;

public class D4_WindowsSpeechRecogniserDemo
{
    private SpeechRecognitionEngine? _speechRecognizerWindows;
    private string _response;
    private float _confidence;
    private bool completed = false;

    [SetUp]
    public void Setup()
    {
        _response = "";
        _confidence = 0.0f;
        _speechRecognizerWindows = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-GB"));
        _speechRecognizerWindows.SetInputToDefaultAudioDevice();
        GrammarBuilder builder = new GrammarBuilder();
        builder.Culture = _speechRecognizerWindows.RecognizerInfo.Culture;
        builder.Append("speakers");
        //builder.Append("who are the speakers this week");
        var grammar = new Grammar(builder);
        _speechRecognizerWindows.LoadGrammar(grammar);
        _speechRecognizerWindows.BabbleTimeout = TimeSpan.FromSeconds(3);
        _speechRecognizerWindows.InitialSilenceTimeout = TimeSpan.FromSeconds(5);
        _speechRecognizerWindows.RecognizeCompleted += WindowsRecognizeCompleted;
    }

    /// <summary>
    /// These tests illustrate the effect of the grammar on the recognition
    /// </summary>
    /// <param name="file"></param>
    /// <param name="expectedWord"></param>
    /// <returns></returns>
    [Test]
    [TestCase("Speakers", "speakers")]
    [TestCase("WhoAreTheSpeakers", "who are the speakers this week?")]
    public async Task RecogniseVoice(string file, string expectedWord)
    {
        // Arrange
        Assert.IsNotNull(_speechRecognizerWindows);
        var stream = FileUtility.LoadStreamFromEmbeddedResource($"JaaS.Demos.Resources.{file}.wav");
        _speechRecognizerWindows.SetInputToAudioStream(stream, new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Mono));

        // Act
        _speechRecognizerWindows.RecognizeAsync(RecognizeMode.Multiple);
        var counter = 0;
        while (!completed && counter < 20)
        {
            await Task.Delay(333);
            counter++;
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
            completed = true;
        }
    }

}