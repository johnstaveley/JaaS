using Azure;
using Azure.AI.OpenAI;
using JaaS.Models;
using OpenAI;
using OpenAI.Chat;

namespace JaaS.Demos;

public class D6_ChatGpt
{
    private AppConfiguration _configuration;
    private AzureOpenAIClient? _openAiClient;
    private ChatClient _chatClient;


    [SetUp]
    public void Setup()
    {
        _configuration = new AppConfiguration();
        _openAiClient = new AzureOpenAIClient(new Uri(_configuration.AzureOpenAiUrl), new AzureKeyCredential(_configuration.AzureOpenAiKey));
        _chatClient = _openAiClient.GetChatClient(_configuration.AzureOpenAiDeployment);
    }

    [Test]
    [TestCase("What is your name?", "my name is jaas")]
    [TestCase("What is the latest version of .net?", ".net 7")]
    public async Task IntroduceYourself(string prompt, string expectedResponse)
    {
        // Arrange
        Assert.That(_openAiClient, Is.Not.Null);
        Assert.That(_chatClient, Is.Not.Null);

        // Act
        var response = await _chatClient.CompleteChatAsync(
            [
            new SystemChatMessage(@"You are an AI assistant that helps people find information. Your name is JaaS which stands for John as a service. You don't `
                    make things up and you reply with answers of 3 sentences or less."),
            new UserChatMessage(prompt)
            ]
            );
        var responseText = response.Value.Content.First().Text.ToLower();

        // Assert
        Assert.That(responseText.Contains(expectedResponse), Is.True);
    }
}