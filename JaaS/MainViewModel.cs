using JaaS.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Linq;
using System.Speech.Recognition;
using Azure.AI.OpenAI;
using Azure;
using System.Configuration;

namespace JaaS;

public class MainViewModel : ViewModelBase
{
    private AppConfiguration _configuration;
    private OpenAIClient? _openAiClient;
    private ChatCompletionsOptions? _chatCompletionsOptions;
    private Microsoft.CognitiveServices.Speech.SpeechRecognizer? _speechRecognizerAzure;
    private SpeechRecognitionEngine? _speechRecognizerWindows;
    private System.Speech.Synthesis.SpeechSynthesizer? _speechSynthesizerWindows;
    private SpeechSynthesizer? _speechSynthesizerAzure;

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
        SpeechConfig azureSpeechConfig = null;
        if (configuration.SpeechRecognizerStrategy == SpeechStrategyKind.Azure ||
            configuration.SpeechSynthesiserStrategy == SpeechStrategyKind.Azure)
        {
            azureSpeechConfig = SpeechConfig.FromSubscription(configuration.AzureSpeechSubscriptionKey, configuration.AzureSpeechRegion);
        }
        if (configuration.SpeechRecognizerStrategy == SpeechStrategyKind.Windows)
        {
            _speechRecognizerAzure = null;
            _speechRecognizerWindows = new SpeechRecognitionEngine();
            InitialiseWindowsRecognitionEngine();
        }
        else
        {
            _speechRecognizerAzure = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(azureSpeechConfig, AudioConfig.FromDefaultMicrophoneInput());
            _speechRecognizerWindows = null;
        }
        if (_configuration.SpeechSynthesiserStrategy == SpeechStrategyKind.Windows)
        {
            _speechSynthesizerWindows = new System.Speech.Synthesis.SpeechSynthesizer();
            InitialiseWindowsSpeechSynthesiserEngine();
            _speechSynthesizerAzure = null;
        }
        else
        {
            InitialiseAzureSpeechSynthesis(azureSpeechConfig);
            _speechSynthesizerWindows = null;
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
        if (_speechSynthesizerAzure != null)
            _speechSynthesizerAzure.Dispose();
    }
    private void InitialiseWindowsSpeechSynthesiserEngine()
    {
        _speechSynthesizerWindows.SetOutputToDefaultAudioDevice();
        _speechSynthesizerWindows.Rate = 0;
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
        _speechRecognizerWindows.RecognizeCompleted += WindowsRecognizeCompleted;

    }

    private void WindowsRecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
    {
        if (e.Result != null)
        {
            var responseText = GetResponse(e.Result.Text, out bool speakResult);
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
        var originalInput = input;
        input = input.ToLower().Trim('.');
        if (input.StartsWith("hello"))
        {
            responseText += "Hello, World!";
        }
        else if (input.Contains("jars") || input.Contains("jaws") || input.Contains("gaz") || input.Contains("charles"))
        {
            responseText += "Jars is a great guy!";
        }
        else if (input == "close")
        {
            responseText = "Closing the application.";
            Close();
        }
        else if (input.Contains("sponsor"))
        {
            responseText = "This meetup is sponsored by Fruition IT, Bruntwood and JetBrains.";
        }
        else if (input.Contains("next event"))
        {
            responseText = "The next event is on the 25th of January by Michael Gray. He is talking about what is the role of a principal engineer.";
        }
        else if (input == "open the pod bay doors")
        {
            responseText = "I'm sorry Dave. I'm afraid I can't do that.";
        }
        else if (_openAiClient != null && _chatCompletionsOptions != null)
        {
            _chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, originalInput));
            Response<ChatCompletions> responseWithoutStream = _openAiClient.GetChatCompletionsAsync(_chatCompletionsOptions).Result;
            ChatCompletions response = responseWithoutStream.Value;
            var responseMessage = new ChatMessage(ChatRole.System, response.Choices.FirstOrDefault().Message.Content);
            _chatCompletionsOptions.Messages.Add(responseMessage);
            responseText = response.Choices.FirstOrDefault().Message.Content;
        }
        else
        {
            responseText = originalInput;
            speakResult = false;
        }
        return responseText;
    }
    private void Speak(string responseText)
    {
        if (_speechSynthesizerWindows != null && _configuration.SpeechSynthesiserStrategy == SpeechStrategyKind.Windows)
        {
            _speechSynthesizerWindows.SpeakAsync(responseText);
        }
        if (_speechSynthesizerAzure != null && _configuration.SpeechSynthesiserStrategy == SpeechStrategyKind.Azure)
        {
            _speechSynthesizerAzure.SpeakTextAsync(responseText);
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
    private void InitialiseAzureSpeechSynthesis(SpeechConfig azureSpeechConfig)
    {
        azureSpeechConfig.SpeechRecognitionLanguage = "en-GB";
        azureSpeechConfig.SpeechSynthesisLanguage = "en-GB";
        // Microsoft Server Speech Text to Speech Voice (en-GB, [Voice]Neural) Where voice is one of:  Thomas, Oliver, Ethan, Noah, Elliot, Alfie, Ryan
        azureSpeechConfig.SpeechSynthesisVoiceName = "Microsoft Server Speech Text to Speech Voice (en-GB, OliverNeural)"; 
        _speechSynthesizerAzure = new SpeechSynthesizer(azureSpeechConfig);
        using var voices = _speechSynthesizerAzure.GetVoicesAsync(azureSpeechConfig.SpeechSynthesisLanguage).Result;
        var englishMaleVoices = voices.Voices.Where(a => a.Gender == SynthesisVoiceGender.Male && a.Locale == "en-GB").ToList();
        var voiceIndex = new Random().Next(0, englishMaleVoices.Count);
        var voice = englishMaleVoices[voiceIndex];
        var oliver = englishMaleVoices.FirstOrDefault(a => a.Name == "Microsoft Server Speech Text to Speech Voice (en-GB, OliverNeural)");
        if (oliver != null)
        {
            voice = oliver;
        }
        azureSpeechConfig.SpeechSynthesisVoiceName = voice.Name;
        _speechSynthesizerAzure = new SpeechSynthesizer(azureSpeechConfig);

    }
    private void InitialiseChatGpt()
    {
        _chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information. Your name is JaaS. You don't make things up and you reply with answers of 3 sentences or less.")
            },
            DeploymentName = _configuration.AzureOpenAiDeployment,
            Temperature = (float)0.5,
            MaxTokens = 800,
            NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };
    }
}
