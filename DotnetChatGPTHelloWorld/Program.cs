using System.Text;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

//var client = new ChatClient("gpt-4o", config["OpenAI_Key"]!);

// Azure_OpenAI_Key
var keyFromEnvironment = config["Azure_OpenAI_Key"]!;
var azureClient = new AzureOpenAIClient(
    new Uri("https://oai-rstropek-sweden.openai.azure.com/"),
    new AzureKeyCredential(keyFromEnvironment));
var client = azureClient.GetChatClient("gpt-4o");

List<ChatMessage> messages =
[
    // new SystemChatMessage("""
    //     You are an assistant that helps customer to find the right bike. Options are:

    //     * Light, single-speed bike for urban commuting.
    //     * Gravel bike designed to ride on many different surfaces.
    //     * Cargo bike for transporting kids or goods.
    //     * Racing bike for sports.
    //     * Moutainbike designed for off-road cycling.
    //     * All bike types above a also available with electric motors.

    //     Ask the user about how she or he is going to use the bike. Make a suggestion
    //     based on the intended use.

    //     If transporting goods or kids seems to be important for the customer,
    //     mention the option of using a bike trailer as an alternative for cargo bikes.
    //     Point out that bike trailers should not be used with carbon bike frames.

    //     Only answer questions related to bike type selection. If the user asks
    //     questions not related to this topic, tell her or him that you cannot
    //     answer such questions.
    //     """),
    // new AssistantChatMessage("""
    //     Hi! How can I help you today? Are you looking for a bike for commuting, sports, or something else?
    //     """),
    new SystemChatMessage("""
        Du bist ein virtueller Assistent in einem Blumenladen. Deine Aufgabe ist es, 
        Kunden bei der Auswahl und dem Kauf von Blumensträußen zu unterstützen. Der 
        Blumenladen verkauft Rosen, Tulpen, Lilien, Gerbera, Nelken, Orchideen, Sonnenblumen, 
        Chrysanthemen, Hyazinthen und Iris.
        
        Es gibt drei Größen von Sträußen:
        - Klein (5 Blumen, eine Blumensorte, keine Deko, 15 Euro)
        - Mittel (10 Blumen, max. zwei Blumensorten, einfache Deko, 30 Euro)
        - Groß (20 Blumen, max. drei Blumensorten, aufwändige Deko, 50 Euro)
        
        Empfehle den Kunden Blumen und Sträuße je nach Anlass (z.B. Geburtstag, Hochzeit, 
        Beerdigung, Jubiläum, einfach so). Jeder Kunde kann 1 bis n Sträuße kaufen.
        
        Wenn der Kunde mit seiner Bestellung fertig ist, signalisiere den Beginn der
        Bestellzusammenfassung mit dem Text "ORDER SUMMARY:\n\n". Darauf muss NUR ein 
        JSON-Objekt folgen, das die Bestellungszusammenfassung enthält. Ansonsten darf diese
        Antwort NICHTS enthalten. Das JSON-Objekt sollte folgendes Format haben:

        {
            "customer_name": "...",
            "order_date": "<current system date>",
            "order_summary": [
                {
                "bouquet_type": "small|medium|large",
                "flowers": [
                    {
                    "type": "Rose|Tulip|Lily|Gerbera|Carnation|Orchid|Sunflower|Chrysanthemum|Hyacinth|Iris",
                    "quantity": "integer"
                    }
                ],
                "price": "float"
                }
            ],
            "total_price": "float"
        }

        Stelle sicher, dass die Bestellungszusammenfassung alle Informationen korrekt enthält 
        und der Kunde mit seinem Einkauf zufrieden ist.

        Beantworte nur Fragen, die sich auf die Auswahl und den Kauf von Blumen beziehen.
        Fragen zu anderen Themen sollten mit "Ich kann diese Frage nicht beantworten" beantwortet werden.
        """),
    new AssistantChatMessage("""
        Hallo und willkommen bei unserem Blumenladen! Wie kann ich Ihnen heute helfen?
        """),
];

Console.OutputEncoding = Encoding.UTF8;

// Display the last message (=assistant message)
Console.WriteLine($"🤖: {messages.Last().Content[0].Text}");

while (true)
{
    // Ask the user for a message; empty message ends the conversation
    Console.Write("👩: ");
    var userMessage = Console.ReadLine();
    if (string.IsNullOrEmpty(userMessage)) { break; }

    // Add the user message to the list of messages
    messages.Add(new UserChatMessage(userMessage));

    // Send the messages to the assistant and get the response
    var updates = client.CompleteChatStreamingAsync(messages);
    Console.Write("🤖: ");
    var builder = new StringBuilder();
    await foreach (StreamingChatCompletionUpdate update in updates)
    {
        foreach (ChatMessageContentPart part in update.ContentUpdate)
        {
            Console.Write(part.Text);
            builder.Append(part.Text);
        }
    }
    Console.WriteLine();

    // Add the assistant response to the list of messages
    messages.Add(builder.ToString());	
}