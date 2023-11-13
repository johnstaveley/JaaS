using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using static System.Net.Mime.MediaTypeNames;

namespace JaaS;

public class MainViewModel : ViewModelBase
{
    private SpeechRecognitionEngine _recognizer;
    private SpeechSynthesizer _synthesizer;

    private string? _recognizedText;
    public string? ResponseText
    {
        get => _recognizedText;
        set
        {
            _recognizedText = value;
            RaisePropertyChanged();
        }
    }

    public MainViewModel()
    {
        _recognizer = new SpeechRecognitionEngine();
        _synthesizer = new SpeechSynthesizer();
        _recognizedText = string.Empty;
        InitialiseRecognitionEngine();
        InitialiseSpeechSynthesiserEngine();
    }

    public void Dispose()
    {
        if (_recognizer != null)
            _recognizer.Dispose();
    }
    private void InitialiseSpeechSynthesiserEngine()
    {
        _synthesizer.SetOutputToDefaultAudioDevice(); // Set output to speakers
    }

    private void InitialiseRecognitionEngine()
    {
        _recognizer.SetInputToDefaultAudioDevice();
        GrammarBuilder builder = new GrammarBuilder();
        builder.Append("Hello");
        _recognizer.LoadGrammar(new Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("Jars");
        _recognizer.LoadGrammar(new Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("Close");
        _recognizer.LoadGrammar(new Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("sponsor");
        _recognizer.LoadGrammar(new Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("next event");
        _recognizer.LoadGrammar(new Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("Open the pod bay doors");
        _recognizer.LoadGrammar(new Grammar(builder));
        _recognizer.BabbleTimeout = TimeSpan.FromSeconds(3);
        _recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(3);
        _recognizer.RecognizeCompleted += RecognizeCompleted;

    }

    private void RecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
    {
        if (e.Result != null)
        {
            var responseText = "";
            var speakResult = true;
            switch (e.Result.Text)
            {
                case "Hello":
                    responseText = "Hello, World!";
                    break;
                case "Jars":
                    responseText = "Jars is a great guy!";
                    break;
                case "Close":
                    responseText = "Closing the application.";
                    Close();
                    break;
                case "sponsor":
                    responseText = "This meetup is sponsored by Fruition IT, Bruntwood and JetBrains.";
                    break;
                case "next event":
                    responseText = "The next event is on the 25th of Jaunary by Michael Gray talking about what is the role of a principal engineer.";
                    break;
                case "Open the pod bay doors":
                    responseText = "I'm sorry Dave. I'm afraid I can't do that.";
                    break;
                default:
                    responseText = e.Result.Text;
                    speakResult = false;
                    break;
            }
            if (speakResult && e.Result.Confidence > 0.7) _synthesizer.SpeakAsync(responseText);
            ResponseText = responseText;
        }
        else
            ResponseText = "I have no idea what you just said.";
    }

    public void ActivateRecognition()
    {
        ResponseText = string.Empty;
        _recognizer.RecognizeAsync();
    }
    public event Action RequestClose;
    public virtual void Close()
    {
        if (RequestClose != null)
        {
            RequestClose();
        }
    }
}
