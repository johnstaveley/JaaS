using System;
using System.Speech.Recognition;

namespace JaaS;

public class MainViewModel : ViewModelBase
{
    private SpeechRecognitionEngine _recognizer;

    private string? _recognizedText;
    public string? RecognizedText
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
        _recognizedText = string.Empty;
        InitializeRecognitionEngine();
    }

    public void Dispose()
    {
        if (_recognizer != null)
            _recognizer.Dispose();
    }

    private void InitializeRecognitionEngine()
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
            switch (e.Result.Text)
            {
                case "Hello":
                    RecognizedText = "Hello, World!";
                    break;
                case "Jars":
                    RecognizedText = "Jars is a great guy!";
                    break;
                case "Close":
                    RecognizedText = "Closing the application.";
                    Close();
                    break;
                case "sponsor":
                    RecognizedText = "This episode is sponsored by JetBrains.";
                    break;
                case "next event":
                    RecognizedText = "The next event is on the 20th of April.";
                    break;
                case "Open the pod bay doors":
                    RecognizedText = "I'm sorry, Dave. I'm afraid I can't do that.";
                    break;
                default:
                    RecognizedText = e.Result.Text;
                    break;
            }
        }
        else
            RecognizedText = "I have no idea what you just said.";
    }

    public void ActivateRecognition()
    {
        RecognizedText = string.Empty;
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
