using System;
using System.Configuration;
using System.Speech.Recognition;
using JaaS.Models;
using Microsoft.CognitiveServices.Speech;

namespace JaaS;

public class MainViewModel : ViewModelBase
{
    private SpeechRecognitionEngine _basicRecognizer;
    private AppConfiguration _configuration;
    private Microsoft.CognitiveServices.Speech.SpeechRecognizer _speechRecognizerAzure;
    private System.Speech.Synthesis.SpeechSynthesizer _basicSynthesizer;
    

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

    public MainViewModel(AppConfiguration configuration)
    {
        _configuration = configuration;
        _basicRecognizer = new SpeechRecognitionEngine();
        _basicSynthesizer = new System.Speech.Synthesis.SpeechSynthesizer();
        var azureSpeechConfig = SpeechConfig.FromSubscription(configuration.SpeechSubscriptionKey, configuration.SpeechRegion);
//        _speechRecognizerAzure = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(azureSpeechConfig);
        _recognizedText = string.Empty;
        InitialiseRecognitionEngine();
        InitialiseSpeechSynthesiserEngine();
    }

    public void Dispose()
    {
        if (_basicRecognizer != null)
            _basicRecognizer.Dispose();
    }
    private void InitialiseSpeechSynthesiserEngine()
    {
        _basicSynthesizer.SetOutputToDefaultAudioDevice();
    }

    private void InitialiseRecognitionEngine()
    {
        _basicRecognizer.SetInputToDefaultAudioDevice();
        GrammarBuilder builder = new GrammarBuilder();
        builder.Append("Hello");
        _basicRecognizer.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("Jars");
        _basicRecognizer.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("Close");
        _basicRecognizer.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("sponsor");
        _basicRecognizer.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("next event");
        _basicRecognizer.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("Open the pod bay doors");
        _basicRecognizer.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        _basicRecognizer.BabbleTimeout = TimeSpan.FromSeconds(3);
        _basicRecognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(3);
        _basicRecognizer.RecognizeCompleted += RecognizeCompleted;

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
            if (speakResult && e.Result.Confidence > 0.7) _basicSynthesizer.SpeakAsync(responseText);
            ResponseText = responseText;
        }
        else
            ResponseText = "I have no idea what you just said.";
    }

    public void ActivateRecognition()
    {
        ResponseText = string.Empty;
        _basicRecognizer.RecognizeAsync();
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
