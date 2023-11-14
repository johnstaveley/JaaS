﻿using System;
using System.Configuration;
using System.Linq;
using System.Speech.Recognition;
using System.Threading.Tasks;
using JaaS.Models;
using Microsoft.CognitiveServices.Speech;

namespace JaaS;

public class MainViewModel : ViewModelBase
{
    private SpeechRecognitionEngine? _speechRecognizerWindows;
    private AppConfiguration _configuration;
    private Microsoft.CognitiveServices.Speech.SpeechRecognizer? _speechRecognizerAzure;
    private System.Speech.Synthesis.SpeechSynthesizer? _speechSynthesizerWindows;


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
        if (configuration.SpeechRecognizerStrategy == SpeechStrategyKind.Windows)
        {
            _speechRecognizerAzure = null;
            _speechRecognizerWindows = new SpeechRecognitionEngine();
            InitialiseWindowsRecognitionEngine();
        }
        else
        {
            var azureSpeechConfig = SpeechConfig.FromSubscription(configuration.AzureSpeechSubscriptionKey, configuration.AzureSpeechRegion);
            _speechRecognizerAzure = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(azureSpeechConfig);
        }
        if (_configuration.SpeechSynthesiserStrategy == SpeechStrategyKind.Windows) {
            _speechSynthesizerWindows = new System.Speech.Synthesis.SpeechSynthesizer();
            InitialiseWindowsSpeechSynthesiserEngine();
        }
        _recognizedText = string.Empty;
    }

    public void Dispose()
    {
        if (_speechRecognizerWindows != null)
            _speechRecognizerWindows.Dispose();
        if (_speechRecognizerAzure!= null)
            _speechRecognizerAzure.Dispose();
        if (_speechSynthesizerWindows != null)
            _speechSynthesizerWindows.Dispose();

    }
    private void InitialiseWindowsSpeechSynthesiserEngine()
    {
        _speechSynthesizerWindows.SetOutputToDefaultAudioDevice();
        _speechSynthesizerWindows.Rate = -2;
        var installedVoices = _speechSynthesizerWindows.GetInstalledVoices().ToList();
        var chosenVoice = installedVoices.FirstOrDefault(x => x.VoiceInfo.Name == "Microsoft David Desktop");
        if (chosenVoice != null)
        {
            _speechSynthesizerWindows.SelectVoice(chosenVoice.VoiceInfo.Name);
        }
    }

    private void InitialiseWindowsRecognitionEngine()
    {
        _speechRecognizerWindows.SetInputToDefaultAudioDevice();
        GrammarBuilder builder = new GrammarBuilder();
        builder.Append("Hello");
        _speechRecognizerWindows.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("Jars");
        _speechRecognizerWindows.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("Close");
        _speechRecognizerWindows.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("sponsor");
        _speechRecognizerWindows.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("next event");
        _speechRecognizerWindows.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        builder = new GrammarBuilder();
        builder.Append("Open the pod bay doors");
        _speechRecognizerWindows.LoadGrammar(new System.Speech.Recognition.Grammar(builder));
        _speechRecognizerWindows.BabbleTimeout = TimeSpan.FromSeconds(3);
        _speechRecognizerWindows.InitialSilenceTimeout = TimeSpan.FromSeconds(3);
        _speechRecognizerWindows.RecognizeCompleted += RecognizeCompleted;

    }

    private void RecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
    {
        if (e.Result != null)
        {
            var speakResult = true;
            var responseText = GetResponse(e.Result.Text, out speakResult);
            if (speakResult && e.Result.Confidence > 0.7)
            {
                Speak(responseText);
            }
            ResponseText = responseText;
        }
        else
            ResponseText = "I have no idea what you just said.";
    }
    private string GetResponse(string input, out bool speakResult)
    {
        var responseText = "";
        speakResult = true;
        switch (input)
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
                responseText = input;
                speakResult = false;
                break;
        }
        return responseText;
    }
    private void Speak(string responseText)
    {
        if (_speechSynthesizerWindows != null)
        {
            _speechSynthesizerWindows.SpeakAsync(responseText);
        }
    }

    public void ActivateRecognition()
    {
        ResponseText = string.Empty;
        if (_speechSynthesizerWindows != null)
        {
            _speechRecognizerWindows.RecognizeAsync();
        }
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
