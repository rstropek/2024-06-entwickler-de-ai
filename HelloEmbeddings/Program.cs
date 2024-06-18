using Microsoft.Extensions.Configuration;
using OpenAI.Embeddings;
using MathNet.Numerics.LinearAlgebra;
using OpenAI.Chat;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var client = new EmbeddingClient("text-embedding-3-large", config["OpenAI_Key"]!);

var response1 = await client.GenerateEmbeddingAsync("I like going to the gym");
var response2 = await client.GenerateEmbeddingAsync("I don't dislike going to the gym");
var vector1 = Vector<float>.Build.Dense(response1.Value.Vector.ToArray());
var vector2 = Vector<float>.Build.Dense(response2.Value.Vector.ToArray());

Console.WriteLine($"Cosine similarity: {vector1.DotProduct(vector2)}");
Console.WriteLine($"Cosine similarity: {DotProduct(response1.Value.Vector.Span, response2.Value.Vector.Span)}");

string[] cityNames =
[
    "Emeraldine: A bustling metropolis surrounded by lush forests, known for its towering skyscrapers and vibrant night markets.",
    "Solara: A small, sun-drenched coastal town famous for its golden beaches, seafood cuisine, and laid-back lifestyle.",
    "Nebulae: A futuristic city with floating buildings and neon lights, renowned for its advanced technology and AI-driven services.",
    "Auroria: A serene mountain city, hidden in misty peaks, with ancient monasteries and breathtaking hiking trails.",
    "Thalassa: An island city with a rich maritime history, surrounded by crystal clear waters, perfect for scuba diving and sailing.",
    "Cinderpeak: A city built around an active volcano, known for its unique architecture, geothermal energy, and vibrant arts scene.",
    "Vespera: A city that never sleeps, with a bustling nightlife, cultural festivals, and a diverse culinary scene, under a starlit sky.",
    "Windmere: A small town on the plains, famous for its windmills, open fields, and a tight-knit community with traditional values.",
    "Polaria: An isolated city in the far north, known for its ice castles, aurora borealis views, and resilient, warm-hearted residents.",
    "Glimmerdale: A city in a valley, illuminated by bioluminescent plants, known for its sustainable living and harmony with nature.",
];

// Embedding vectors for all cities
var response = await client.GenerateEmbeddingsAsync(cityNames);
var cityVectors = response.Value.Select(e => Vector<float>.Build.Dense(e.Vector.ToArray())).ToArray();

Console.Write("What would you like to do in your vacation? ");
string query = Console.ReadLine()!;
var queryVector = Vector<float>.Build.Dense((await client.GenerateEmbeddingAsync(query)).Value.Vector.ToArray());

// Big Question: Which cities are most relevant for the user's vacation preferences?

var similarities = cityVectors.Select(v => v.DotProduct(queryVector)).ToArray();

// K-nearest neighbors search
var topCities = similarities
    .Select((similarity, index) => (similarity, index))
    .OrderByDescending(pair => pair.similarity)
    .Take(3)
    .Select(pair => cityNames[pair.index]);

Console.WriteLine("Top cities for your vacation:");
foreach (var city in topCities) { Console.WriteLine(city); }

// AG = Augmented Generation

var chatClient = new ChatClient("gpt-4o", config["OpenAI_Key"]!);

List<ChatMessage> messages =
[
    new SystemChatMessage($"""
        You are a helpful assistant in a travel agency. Customers are describing
        what they want to do in their vacation. Make suggestions based on the
        city descriptions provided below. ONLY use the provided city descriptions.
        Do NOT use other information sources. Answer the user in the language
        of his question.

        If you cannot generate a meaningful answer based on the given city description,
        write a polite excuse. If the user's input is not related to finding
        a travel location, say "Sorry, I can only help with vacation locations".

        ===========
        {string.Join("\n\n", topCities)}
        ===========
        """),
    new UserChatMessage(query)
];

var chatResponse = await chatClient.CompleteChatAsync(messages);

Console.WriteLine(chatResponse.Value.Content[0].Text);

static float DotProduct(ReadOnlySpan<float> span1, ReadOnlySpan<float> span2)
{
    if (span1.Length != span2.Length)
    {
        throw new ArgumentException("Vectors must be of the same length");
    }

    int length = span1.Length;
    int simdLength = System.Numerics.Vector<float>.Count;
    float result = 0.0f;

    // Process chunks of simdLength
    int i;
    for (i = 0; i <= length - simdLength; i += simdLength)
    {
        System.Numerics.Vector<float> v1 = new System.Numerics.Vector<float>(span1.Slice(i, simdLength));
        System.Numerics.Vector<float> v2 = new System.Numerics.Vector<float>(span2.Slice(i, simdLength));
        result += System.Numerics.Vector.Dot(v1, v2);
    }

    // Process remaining elements
    for (; i < length; i++)
    {
        result += span1[i] * span2[i];
    }

    return result;
}