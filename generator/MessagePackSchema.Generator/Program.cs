using MessagePackSchema.Generator;
using System.Text;

while (true)
{
    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        try
        {
            // Decode base64
            input = Encoding.UTF8.GetString(Convert.FromBase64String(input));

            // Decode JSON
            var generateInput = GenerateInput.FromJson(input);

            using var output = Console.OpenStandardOutput();
            using var streamWriter = new StreamWriter(output);
            streamWriter.WriteLine("ok");
            break;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }
}