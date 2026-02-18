if (Args.Count == 0)
{
    Console.WriteLine("Usage: dotnet script EncodeToBase64.csx <inputFilePath>");
    return;
}

var inputPath = Args[0];

if (!File.Exists(inputPath))
{
    Console.WriteLine($"File not found: {inputPath}");
    return;
}

var outputPath = Path.ChangeExtension(inputPath, ".txt");

var bytes = File.ReadAllBytes(inputPath);
var base64 = Convert.ToBase64String(bytes);
File.WriteAllText(outputPath, base64);

Console.WriteLine($"Base64 written to {outputPath}");
