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
        //builder.Append("speakers");
        builder.Append("who are the speakers this week");
        var grammar = new Grammar(builder);
        _speechRecognizerWindows.LoadGrammar(grammar);
        _speechRecognizerWindows.BabbleTimeout = TimeSpan.FromSeconds(3);
        _speechRecognizerWindows.InitialSilenceTimeout = TimeSpan.FromSeconds(5);
        _speechRecognizerWindows.RecognizeCompleted += WindowsRecognizeCompleted;
    }

    [TearDown]
    public void Teardown()
    {
        if (_speechRecognizerWindows != null)
        {
            _speechRecognizerWindows.Dispose();
        }
    }

    /// <summary>
    /// These demos illustrate the effect of the grammar on the recognition
    /// </summary>
    [Test]
    [TestCase("Speakers", "speakers")]
    [TestCase("WhoAreTheSpeakers", "who are the speakers this week", Description = "This one won't work without changes to the grammar")]
    public async Task RecogniseVoice(string file, string expectedWord)
    {
        // Arrange
        Assert.That(_speechRecognizerWindows, Is.Not.Null);
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