To get this working, copy app.config from the main project and remame as testhost.dll.config

Your config file should look like this:

<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="AzureNeuralVoiceEndpointId" value="CHANGEME"/>
    <add key="AzureNeuralVoiceName" value="CHANGEME"/>
    <add key="AzureOpenAiDeployment" value="CHANGEME"/>
    <add key="AzureOpenAiKey" value="CHANGEME"/>
    <add key="AzureOpenAiUrl" value="CHANGEME"/>
    <add key="AzureSpeechSubscriptionKey" value="CHANGEME"/>
    <add key="AzureSpeechRegion" value="CHANGEME"/>
    <add key="SpeechRecognizerStrategy" value="Azure"/>
    <add key="SpeechSynthesiserStrategy" value="Azure"/>
  </appSettings>
</configuration>