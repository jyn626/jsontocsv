using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program {
    public static bool isRootAnArray(string json)
    {
        try
        {
            //  `using` acts as a lifetime-limiting control structure ensuring
            //  that the object's .Dispose() method is immediately triggered
            //  the moment the code leaves that block
            using (JsonDocument jsonDom = JsonDocument.Parse(json))
            {
                // get root of the json
                JsonElement root = jsonDom.RootElement;

                // verify if the root is an array
                if (root.ValueKind != JsonValueKind.Array) {
                    return false;
                }

                // check if all elements in the array are all objects
                foreach (JsonElement element in root.EnumerateArray()) 
                {
                    if (element.ValueKind != JsonValueKind.Object) {
                        return false;
                    }
                }

            }
            // the JsonDocument library is automatically CLOSED
            // and disposed right here, even if errors occur above.
            return true;

        }
        catch (JsonException e)
        {
            Console.WriteLine($"JSON invalid: {e.Message}");
            Console.WriteLine($"Path: {e.Path}");
            return false;
        }
    }
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

            // TODO: before serializing check for any unsuporrted JSON formats (nested objects/arrays, ...)
            // and send an error message.
            Console.WriteLine("----Serialized----");
            Console.WriteLine(serialized);

            Console.WriteLine("---isRootAnArray---");
            Console.WriteLine(isRootAnArray(fileContent));
        } catch(JsonException e) {
            // Captures JSON formatting, depth limits, or conversion issues
            Console.WriteLine($"JSON invalid: {e.Message}"); 
            Console.WriteLine($"Path: {e.Path}");
        }
     }

}