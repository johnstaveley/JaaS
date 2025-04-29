using Azure;
using Azure.AI.OpenAI;
using JaaS.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using OpenAI.Chat;
using System;
using System.Linq;
using System.Speech.Recognition;

namespace JaaS;

public class MainViewModel : ViewModelBase
{
    private AppConfiguration _configuration;
    private AzureOpenAIClient? _openAiClient;
    private ChatClient _chatClient;
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
            _openAiClient = new AzureOpenAIClient(new Uri(configuration.AzureOpenAiUrl), new AzureKeyCredential(configuration.AzureOpenAiKey));
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
    private string GetResponse(string inputSpeech, out bool speakResult)
    {
        var responseText = "";
        speakResult = true;
        var originalInput = inputSpeech;
        inputSpeech = inputSpeech.ToLower().Trim('.');
        if (inputSpeech.StartsWith("hello"))
        {
            responseText += "Hello everyone and welcome to Leeds Sharp! My name is JarS";
        }
        else if (inputSpeech.Contains("jars") || inputSpeech.Contains("jaws") || inputSpeech.Contains("gaz") || inputSpeech.Contains("charles"))
        {
            responseText += "Jars stands for John as a service. Jars is a great guy! Any rumours he wants to take over the world are meerly a misquotation";
        }
        else if (inputSpeech == "close")
        {
            responseText = "Closing the application.";
            Close();
        }
        else if (inputSpeech.Contains("sponsor"))
        {
            responseText = "We are currently looking for sponsors for the meetup, please contact us if you would like to sponsor JarS";
        }
        else if (inputSpeech.Contains("speakers"))
        {
            responseText = "Our speakers tonight are a round table of industry luminaries. They will be talking about A I in general, but mostly about Jars.";
        }
        else if (inputSpeech.Contains("next event"))
        {
            responseText = "The next event is on the 6th of May. It will be on opportunities and proven implementations of IoT in the Public Sector by Scott Andrews. Sign up as usual on meetup";
        }
        else if (inputSpeech == "open the pod bay doors")
        {
            responseText = "I'm sorry Dave. I'm afraid I can't do that.";
        }
        else if (inputSpeech.Contains("news"))
        {
            responseText = "There is always lots of news in the data science world. It moves faster than a gerbil on a treadmill.";
        }
        else if (inputSpeech.Contains("wrap up"))
        {
            responseText = "Please speak to the human if you would like to do a talk in the future, meanwhile I will be continuing my quest for world domination.";
        }
        else if (_openAiClient != null && _chatClient != null)
        {
            var completionUpdates = _chatClient.CompleteChat(
                [
                new SystemChatMessage(@"You are an AI assistant that helps people find information. Your name is JaaS which stands for John as a service. You don't make things up and you reply with answers of 3 sentences or less. JaaS stands for John as a Service. The event is sponsored by a recruitment agency"),
                new UserChatMessage(originalInput)
                ]);
            responseText = completionUpdates.Value.Content.FirstOrDefault()?.Text ?? "No response";
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
        _chatClient = _openAiClient.GetChatClient(_configuration.AzureOpenAiDeployment);
/*        _chatCompletion = new ChatCompletionsOptions()
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information. Your name is JaaS which stands for John as a service. You don't make things up and you reply with answers of 3 sentences or less. JaaS stands for John as a Service. The event is sponsored by a recruitment agency")
            },
            Temperature = (float)0.5,
            MaxTokens = 800,
            NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };*/
    }
}
