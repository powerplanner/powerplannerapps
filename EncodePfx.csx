var inputPath = @"C:\Users\andre\Downloads\PowerPlannerUWP_Sideloading_2026.pfx";
var outputPath = Path.ChangeExtension(inputPath, ".txt");

var bytes = File.ReadAllBytes(inputPath);
var base64 = Convert.ToBase64String(bytes);
File.WriteAllText(outputPath, base64);

Console.WriteLine($"Base64 written to {outputPath}");
