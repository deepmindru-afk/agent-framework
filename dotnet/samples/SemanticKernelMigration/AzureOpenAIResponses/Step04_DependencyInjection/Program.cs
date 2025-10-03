﻿// Copyright (c) Microsoft. All rights reserved.

using System.ClientModel;
using System.ClientModel.Primitives;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Agents.OpenAI;
using OpenAI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = System.Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o";
var userInput = "Tell me a joke about a pirate.";

Console.WriteLine($"User Input: {userInput}");

await SKAgentAsync();
await AFAgentAsync();

async Task SKAgentAsync()
{
    Console.WriteLine("\n=== SK Agent ===\n");

    var serviceCollection = new ServiceCollection();
    serviceCollection.AddTransient<Microsoft.SemanticKernel.Agents.Agent>((sp)
        => new OpenAIResponseAgent(new OpenAIClient(
            new BearerTokenPolicy(new AzureCliCredential(), "https://cognitiveservices.azure.com/.default"),
            new OpenAIClientOptions() { Endpoint = new Uri($"{endpoint}/openai/v1") })
        .GetOpenAIResponseClient(deploymentName))
        {
            Name = "Joker",
            Instructions = "You are good at telling jokes."
        });

    await using ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
    var agent = serviceProvider.GetRequiredService<Microsoft.SemanticKernel.Agents.Agent>();

    var result = await agent.InvokeAsync(userInput).FirstAsync();
    Console.WriteLine(result.Message);
}

async Task AFAgentAsync()
{
    Console.WriteLine("\n=== AF Agent ===\n");

    var serviceCollection = new ServiceCollection();
    serviceCollection.AddTransient((sp) => new OpenAIClient(
        new BearerTokenPolicy(new AzureCliCredential(), "https://cognitiveservices.azure.com/.default"),
        new OpenAIClientOptions() { Endpoint = new Uri($"{endpoint}/openai/v1") })
        .GetOpenAIResponseClient(deploymentName)
        .CreateAIAgent(name: "Joker", instructions: "You are good at telling jokes."));

    await using ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
    var agent = serviceProvider.GetRequiredService<AIAgent>();

    var result = await agent.RunAsync(userInput);
    Console.WriteLine(result);
}
