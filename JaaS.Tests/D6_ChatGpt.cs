using Azure;
using Azure.AI.OpenAI;
using JaaS.Demos.Utility;
using JaaS.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Configuration;
using System.Speech.Synthesis;

namespace JaaS.Demos;

public class D6_ChatGpt
{
    private AppConfiguration _configuration;
    private OpenAIClient? _openAiClient;
    private ChatCompletionsOptions? _chatCompletionsOptions;


    [SetUp]
    public void Setup()
    {
        _configuration = new AppConfiguration();
        _openAiClient = new OpenAIClient(new Uri(_configuration.AzureOpenAiUrl), new AzureKeyCredential(_configuration.AzureOpenAiKey));
        _chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information. Your name is JaaS. You don't make things up and you reply with answers of 3 sentences " +
                "or less.")
            },
            DeploymentName = _configuration.AzureOpenAiDeployment,
            Temperature = (float)0.5,
            MaxTokens = 800,
            NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };
    }

    [Test]
    [TestCase("What is your name?", "My name is JaaS")]
    public async Task IntroduceYourself(string prompt, string expectedResponse)
    {
        // Arrange
        _chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, prompt));

        // Act
        Response<ChatCompletions> responseWithoutStream = _openAiClient.GetChatCompletionsAsync(_chatCompletionsOptions).Result;
        ChatCompletions response = responseWithoutStream.Value;
        var responseMessage = new ChatMessage(ChatRole.System, response.Choices.FirstOrDefault().Message.Content);
        _chatCompletionsOptions.Messages.Add(responseMessage);
        var responseText = response.Choices.FirstOrDefault().Message.Content;

        // Assert
        Assert.AreEqual(expectedResponse, responseText);
    }
}