using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var client = new ChatClient("gpt-4o", config["OpenAI_Key"]!);

List<ChatMessage> messages =
[
    new SystemChatMessage("""
        You are a helpful assistant answering questions about comic characters.
        The characters are stored in a database. You can access this database through
        provided functions.

        Keep your answers short, funny, and sprinkle in some emoji.
        """),
];

var getComicCharacterFunction = ChatTool.CreateFunctionTool(
    functionName: "getComicCharacter",
    functionDescription: "Get information about a comic character from the database.",
    functionParameters: // JSON Schema
        BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "name": {
                        "type": "string",
                        "description": "The name of the character."
                    }
                },
                "required": ["name"]
            }
            """)
);

var getAllComicCharactersFunction = ChatTool.CreateFunctionTool(
    functionName: "getAllComicCharacters",
    functionDescription: "Get a list of all comic characters in the database.",
    functionParameters:
        BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "nameContainsFilter": {
                        "type": "string",
                        "description": "Filter the list of characters by name."
                    }
                }
            }
            """)
);

var options = new ChatCompletionOptions()
{
    Tools = { getComicCharacterFunction, getAllComicCharactersFunction }
};

do
{
    Console.Write("You: ");
    var userMessage = Console.ReadLine()!;
    messages.Add(new UserChatMessage(userMessage));

    bool requiresAction;
    do
    {
        requiresAction = false;

        ChatCompletion completion = await client.CompleteChatAsync(messages, options);
        Console.WriteLine($"""
            \tInput tokens: {completion.Usage.InputTokens}
            \tOutput tokens: {completion.Usage.OutputTokens}
            \tTotal tokens: {completion.Usage.TotalTokens}
            """);

        switch (completion.FinishReason)
        {
            case ChatFinishReason.Stop:
                // We have a response that we should send to the user
                var responseFromBot = completion.Content[0].Text;
                Console.WriteLine($"Bot: {responseFromBot}");
                messages.Add(new AssistantChatMessage(responseFromBot));
                break;
            case ChatFinishReason.ToolCalls:
                // We have a tool call that we must execute and return the result to OpenAI
                messages.Add(new AssistantChatMessage(completion));
                foreach (ChatToolCall toolCall in completion.ToolCalls)
                {
                    Console.WriteLine($"\tBot: Executing tool call '{toolCall.FunctionName}' with arguments '{toolCall.FunctionArguments}.");
                    switch (toolCall.FunctionName)
                    {
                        case "getComicCharacter":
                            try
                            {
                                var functionArguments = JsonSerializer.Deserialize<GetComicCharacterArguments>(
                                    toolCall.FunctionArguments,
                                    new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                                var httpClient = new HttpClient();
                                var response = await httpClient.GetFromJsonAsync<Pokemon>($"https://pokeapi.co/api/v2/pokemon/{functionArguments!.Name.ToLower()}/");
                                messages.Add(new ToolChatMessage(toolCall.Id, JsonSerializer.Serialize(response)));
                            }
                            catch (JsonException)
                            {
                                messages.Add(new ToolChatMessage(toolCall.Id, "Invalid arguments."));
                                break;
                            }
                            break;
                        case "getAllComicCharacters":
                            {
                                var functionArguments = JsonSerializer.Deserialize<GetAllComicCharactersArguments>(
                                    toolCall.FunctionArguments,
                                    new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                                var pokemons = await GetAllPokemons(functionArguments!.NameContainsFilter ?? "");
                                messages.Add(new ToolChatMessage(toolCall.Id, JsonSerializer.Serialize(pokemons)));
                                break;
                            }
                        default:
                            messages.Add(new ToolChatMessage(toolCall.Id, $"Function '{toolCall.FunctionName}' not found."));
                            break;
                    }
                }

                requiresAction = true;
                break;
        }
    } while (requiresAction);
}
while (true);

static async Task<string[]> GetAllPokemons(string nameContainsFilter)
{
    var httpClient = new HttpClient();
    var pokemons = new List<string>();
    string? nextUrl = "https://pokeapi.co/api/v2/pokemon";

    do
    {
        var response = await httpClient.GetFromJsonAsync<ResultRoot>(nextUrl);
        var results = response?.Results;
        if (results != null)
        {
            pokemons.AddRange(results.Select(pokemon => pokemon.Name));
            nextUrl = response!.Next;
        }
        else
        {
            nextUrl = null;
        }
    } while (nextUrl != null);

    return pokemons.Where(pokemon => pokemon.Contains(nameContainsFilter.ToLower())).ToArray();
}

record ResultRoot(int Count, string? Next, string? Previous, List<Result> Results);

record Result(string Name, string Url);

record GetAllComicCharactersArguments(string? NameContainsFilter);

record GetComicCharacterArguments(string Name);

record Pokemon(string Name, int Weight);
