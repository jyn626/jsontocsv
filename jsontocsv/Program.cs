using System.Text.Json;

class Program {
    public static void Main(string[] args) {
        Console.Write("Current Directory: ");
        Console.Write(Directory.GetCurrentDirectory());
        Console.WriteLine();
        // if directory is jsontocsv/jsontocsv then input/sample.json filepath would work cuz its inside the directory,
        // but if the directory comes from /bin/Debug/... then it means were running from inside the bin/ and we'll need 
        // to go up three directories to reach the input/ file : ..\..\..\
        string filePath = @"..\..\..\input\sample.json";

        // read the entire file content of the JSON as a txt or string but not an actual JSON file.
        var fileContent = File.ReadAllText(filePath);

        if (fileContent == null || fileContent == "")
        {
            Console.WriteLine("JSON file is empty.");
            return;
        }

        try
        {
            // parse JSON
            var serialized = JsonSerializer.Serialize(fileContent);
        
            // print out contents
            Console.WriteLine(fileContent);
            Console.WriteLine("----Serialized----");
            Console.WriteLine(serialized);
        } catch(JsonException e) {
            // Captures JSON formatting, depth limits, or conversion issues
            Console.WriteLine($"JSON invalid: {e.Message}"); 
            Console.WriteLine($"Path: {e.Path}");
        }
     }

}