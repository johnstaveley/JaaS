using JaaS.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Linq;
using System.Speech.Recognition;
using Azure.AI.OpenAI;
using Azure;

namespace JaaS;

public class MainViewModel : ViewModelBase
{
    private AppConfiguration _configuration;
    private OpenAIClient? _openAiClient;
    private ChatCompletionsOptions? _chatCompletionsOptions;
    private Microsoft.CognitiveServices.Speech.SpeechRecognizer? _speechRecognizerAzure;
    private SpeechRecognitionEngine? _speechRecognizerWindows;
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
        if (configuration.UseOpenAi)
        {
            _openAiClient = new OpenAIClient(new Uri(configuration.AzureOpenAiUrl), new AzureKeyCredential(configuration.AzureOpenAiKey));
            InitialiseChatGpt();
        }
        if (configuration.SpeechRecognizerStrategy == SpeechStrategyKind.Windows)
        {
            _speechRecognizerAzure = null;
            _speechRecognizerWindows = new SpeechRecognitionEngine();
            InitialiseWindowsRecognitionEngine();
        }
        else
        {
            var azureSpeechConfig = SpeechConfig.FromSubscription(configuration.AzureSpeechSubscriptionKey, configuration.AzureSpeechRegion);
            _speechRecognizerAzure = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(azureSpeechConfig, GetAudioConfig());
            _speechRecognizerWindows = null;
        }
        if (_configuration.SpeechSynthesiserStrategy == SpeechStrategyKind.Windows)
        {
            _speechSynthesizerWindows = new System.Speech.Synthesis.SpeechSynthesizer();
            InitialiseWindowsSpeechSynthesiserEngine();
        }
        _recognizedText = string.Empty;
    }

    public void Dispose()
    {
        if (_speechRecognizerWindows != null)
            _speechRecognizerWindows.Dispose();
        if (_speechRecognizerAzure != null)
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
    private AudioConfig GetAudioConfig()
    {
        var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        return audioConfig;
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
        _speechRecognizerWindows.RecognizeCompleted += WindowsRecognizeCompleted;

    }

    private void WindowsRecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
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
        switch (input.ToLower().Trim('.'))
        {
            case "hello":
                responseText = "Hello, World!";
                break;
            case "jars":
            case "jaws":
                responseText = "Jars is a great guy!";
                break;
            case "close":
                responseText = "Closing the application.";
                Close();
                break;
            case "sponsor":
                responseText = "This meetup is sponsored by Fruition IT, Bruntwood and JetBrains.";
                break;
            case "next event":
                responseText = "The next event is on the 25th of Jaunary by Michael Gray talking about what is the role of a principal engineer.";
                break;
            case "open the pod bay doors":
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
        if (_speechSynthesizerWindows != null && _configuration.SpeechSynthesiserStrategy == SpeechStrategyKind.Windows)
        {
            _speechSynthesizerWindows.SpeakAsync(responseText);
        }
    }

    public void ActivateRecognition()
    {
        ResponseText = string.Empty;
        if (_speechRecognizerWindows != null && _configuration.SpeechRecognizerStrategy == SpeechStrategyKind.Windows)
        {
            _speechRecognizerWindows.RecognizeAsync();
        }
        if (_speechRecognizerAzure != null && _configuration.SpeechRecognizerStrategy == SpeechStrategyKind.Azure)
        {
            var result = _speechRecognizerAzure.RecognizeOnceAsync().Result;
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                var responseText = GetResponse(result.Text.Trim('.'), out bool speakResult);
                if (speakResult)
                {
                    Speak(responseText);
                }
                ResponseText = responseText;
            }
            else
                ResponseText = "I have no idea what you just said.";
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
    private void InitialiseChatGpt()
    {
        _chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information. Your name is JaaS. You don't make things up and you reply with answers of 3 sentences or less.")
            },
            Temperature = (float)0.5,
            MaxTokens = 800,
            NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };
    }
}
