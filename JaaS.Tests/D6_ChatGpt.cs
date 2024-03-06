using Azure;
using Azure.AI.OpenAI;
using JaaS.Models;

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
                new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information. Your name is JaaS. You don't `
                    make things up and you reply with answers of 3 sentences or less.")
            },
            DeploymentName = _configuration.AzureOpenAiDeployment,
            Temperature = 0.5F,
            MaxTokens = 800,
            NucleusSamplingFactor = 0.95F,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };
    }

    [Test]
    [TestCase("What is your name?", "my name is jaas")]
    [TestCase("What is the latest version of .net?", ".net 5")]
    public async Task IntroduceYourself(string prompt, string expectedResponse)
    {
        // Arrange
        Assert.IsNotNull(_openAiClient);
        Assert.IsNotNull(_chatCompletionsOptions);
        _chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, prompt));

        // Act
        Response<ChatCompletions> responseWithoutStream = await _openAiClient.GetChatCompletionsAsync(_chatCompletionsOptions);
        ChatCompletions response = responseWithoutStream.Value;
        var responseText = response.Choices.First().Message.Content.ToLower();

        // Assert
        Assert.IsTrue(responseText.Contains(expectedResponse));
    }
}