using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

public static class FileReader
{
    public static string Read(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("File not found: " + e);
            return "";
        }
    }

    public static bool isEmpty(string content)
    {
        if (String.IsNullOrWhiteSpace(content))
        {
            return true;
        }
        return false;
    }
}

public static class TableBuilder
{
    public static List<string> Headers = new List<string>();
    public static List<List<string>> Rows = new List<List<string>>();

    public static List<string> getHeaders(JsonDocument json)
    {
        Headers.Clear();
        // ! TODO: handle exceptions -> ArgumentOutOfRangeException, InvalidOperationException, others...
        // parse json into a JsonObject
        //JsonObject parsedJson = JsonNode.Parse(stringJson).AsObject();

        // 1. Read JSON file
        // 2. Parse JSON into a readable object
        // 3. Get Headers

        var root = json.RootElement;

        // if the json file is just a single object {}
        // then do:
        if (root.ValueKind == JsonValueKind.Object)
        {
            // JsonNode.Parse -> converts a JSON string directly into a JsonObject, which allows to manipulate the data as a key-value pair collection.
            //JsonObject parsedJson = JsonDocument.Parse().AsObject();
            JsonObject jsonObj = JsonObject.Create(root);

            // iterate through to the json object and add its Key (Headers)

            foreach (var property in jsonObj)
            {
                Headers.Add(property.Key);
            }
        }
        else
        {
            // suppose stringJson is an [] (array) of JSONs we get the first object's keys/Headers
            var firstObject = root.EnumerateArray().First(); // the first object from the array (root element)

            // EnumerateObject() -> converts a JSON object into a searchable, loopable collection of key - value properties.
            foreach (JsonProperty property in firstObject.EnumerateObject())
            {
                Headers.Add(property.Name);
            }

        }

        return Headers;

    }
    public static List<List<string>> createRows(string json)
    {
        Rows.Clear();
        using (var jsonDoc = JsonDocument.Parse(json))
        {
            if (jsonDoc.RootElement.ValueKind != JsonValueKind.Array)
            {
                var jsonObj = jsonDoc.RootElement;
                List<string> row = new();

                foreach (string header in TableBuilder.Headers)
                {
                    string property = jsonObj.GetProperty(header).ToString();
                    row.Add(property);
                }
                Rows.Add(row);
            }
            else
            {
                // json type -> array
                foreach (var jsonObj in jsonDoc.RootElement.EnumerateArray())
                {
                    // TODO: handle special characters in CSV cells (commas, qoutes, line breaks)
                    if (jsonObj.ValueKind == JsonValueKind.Object)
                    {
                        List<string> row = new();

                        foreach (string header in Headers)
                        {
                            string value = jsonObj.GetProperty(header).ToString();
                            // handle empty value
                            if (String.IsNullOrEmpty(value))
                            {
                                row.Add("");
                            }
                            else
                            {
                                row.Add(value);
                            }

                        }

                        Rows.Add(row);
                    }
                }
            }
        }

        return Rows;
    }
}

public static class CsvFormatter
{
    public static void DisplayCsv()
    {
        foreach (string header in TableBuilder.Headers)
        {
            Console.Write(header + ", ");
        }

        Console.WriteLine();

        foreach (var row in TableBuilder.Rows)
        {
            foreach (string prop in row)
            {
                Console.Write(prop + ", ");
            }
            Console.WriteLine();
        }

    }

    public static string Format()
    {
        // take the Headers and Rows and turn it into one big string that looks like a CSV file.
        // Headers: ["name", "age", "city"]
        // Rows:
        //  [
        //    ["john", "20", "tokyo"],
        //    ["alice", "25", "seoul"]
        //  ]

        // Headers
        var csvData = new StringBuilder();

        // join the Headers with a comma -> name,age,city
        string _Headers = String.Join(',', TableBuilder.Headers);

        // append first the Headers cuz we need it at the top
        csvData.AppendLine(_Headers);

        // Rows
        foreach (var row in TableBuilder.Rows)
        {
            List<string> temp = new(); // store the values inside an array so we can join it later by a comma

            foreach (string value in row)
            {
                if (value.Contains(",") || value.Contains("\""))
                {
                    // if the value contains a comma (New York, USA) then wrapp it in qoutes ("New York, USA")
                    // OR if it had qoutes inside it ("Matt "Daredevil" Murdock")
                    temp.Add($"\"{value}\"");
                }
                else
                {
                    // if the value is just a plain text with no especial chars then just add it directly.
                    temp.Add(value);
                }

            }

            string data = String.Join(",", temp); // joins the row with a comma
            csvData.AppendLine(data); // append the joined data to our stringbuilder

        }

        // have to call .ToString() on a StringBuilder 
        // because it is not a string type, it is a 
        // custom internal buffer optimized for memory manipulation
        string finalStringCsv = csvData.ToString();
        // Console.WriteLine(finalStringCsv);

        return finalStringCsv;
    }
}

public static class CsvWriter
{
    public static void WriteToDisk()
    {
        // write the csv string to output/result.csv,
        // if name already exists then name_2 -> name_3 -> ...
        int fileCount = 0;
        string fileName = $"result{fileCount}.csv";
        string folder = @".\output";
        try
        {
            // check if the directory exists, if not then create directory 
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // getting all files in the folder, and select only their name
            var existingFiles = Directory.EnumerateFiles(folder)
                                        // LINQ stuffs i should learn about 
                                        // removes path: C:/folder/ouput/
                                        .Select(Path.GetFileName);

            // while the filename exists already keep generating a new one.
            while (existingFiles.Contains(fileName))
            {
                fileCount++;
                fileName = $"result{fileCount}.csv";
            }

            // form the complete path
            string path = Path.Combine(folder, fileName);
            // combines our headers and rows into a string 
            string csvString = CsvFormatter.Format();

            // File.WriteAllText is a built-in static method in C# (System.IO) used to create a new file, 
            // write a string to it, and automatically close the file. 
            // if the target file already exists, the method overwrites its contents
            File.WriteAllText(path, csvString, Encoding.UTF8);

            // print the file path to find it quickly
            Console.WriteLine(path);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


    }
}

public static class JsonParser
{
    public static JsonDocument ParseJsonIntoDocu(string jsonString)
    {
        try
        {
            return JsonDocument.Parse(jsonString);
        }
        catch (JsonException e)
        {
            Console.WriteLine("JSON is invalid: " + e);
            return null;
        }
    }
}

class Program
{
    public static void Main(string[] args)
    {
        Console.Write("Current Directory: ");
        Console.Write(Directory.GetCurrentDirectory());
        Console.WriteLine();

        try
        {
            // parsing JSON,
            // i misunderstood these methods so im commenting it out.
            // 1. JsonNode.Parse converts a JSON string into a mutable, readable DOM object.
            // 2. JsonSerializer.Serialize converts a C# object into a JSON string

            //var serialized = JsonSerializer.Serialize(fileContent);
            //var parsedJson = JsonNode.Parse(fileContent).AsObject();

            // print out contents
            // Console.WriteLine(fileContent

            // if directory is jsontocsv/jsontocsv then input/sample.json filepath would work cuz its inside the directory,
            // but if the directory comes from /bin/Debug/... then it means were running from inside the bin/ and we'll need 
            // to go up three directories to reach the input/ file : ..\..\..\
            string path = @".\input\sample.json";

            // string stringJson = ReadJsonFile(path);
            string stringJson = FileReader.Read(path);

            if (FileReader.isEmpty(stringJson))
            {
                Console.WriteLine("File is empty.");
                return;
            }

            // using JsonDocument doc = ParseJsonIntoDocu(stringJson);
            using JsonDocument doc = JsonParser.ParseJsonIntoDocu(stringJson);

            // add contents to `Headers` -> ["heade1", "header2", ...]
            var headers = TableBuilder.getHeaders(doc);
            // add contents to `Rows` -> [ ["row1", ...], ["row2", ...] ]
            var rows = TableBuilder.createRows(stringJson);
            // saves csv results to /output/result.csv 
            CsvWriter.WriteToDisk();

            // TODO: before parsing check for any unsuporrted JSON
            // formats (nested objects/arrays, ...) and send an error message.
            //Console.WriteLine("---- Parsed JSON ----");
            //Console.WriteLine(parsedJson);


            // Create a list of column names(Headers)

            //Console.WriteLine("---isRootAnArray---");
            //Console.WriteLine(isRootAnArray(fileContent));
        }
        catch (JsonException e)
        {
            // Captures JSON formatting, depth limits, or conversion issues
            Console.WriteLine($"JSON invalid: {e.Message}");
            Console.WriteLine($"Path: {e.Path}");
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine($"File not found error: {e.Message}");
        }
    }

}