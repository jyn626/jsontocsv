using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

class Program
{

    public static List<string> Headers { get; set; } = new List<string>();
    public static List<List<string>> Rows { get; set; } = new List<List<string>>();

    public static string ReadJsonFile(string path)
    {
        // read the entire file content of the JSON as a string but not an actual JSON file.
        return File.ReadAllText(path);
    }

    public static JsonDocument ParseJsonIntoDocu(string jsonString)
    {
        return JsonDocument.Parse(jsonString);
    }

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
                if (root.ValueKind != JsonValueKind.Array)
                {
                    return false;
                }

                // check if all elements in the array are all objects
                foreach (JsonElement element in root.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object)
                    {
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

    public static void getHeaders(JsonDocument json)
    {
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
            var firstObject = root[0]; // the first object from the array (root element)

            // EnumerateObject() -> converts a JSON object into a searchable, loopable collection of key - value properties.
            foreach (JsonProperty property in firstObject.EnumerateObject())
            {
                Headers.Add(property.Name);
            }

        }

    }

    public static void createRows(string stringJson)
    {
        using (var jsonDoc = JsonDocument.Parse(stringJson))
        {
            if (!isRootAnArray(stringJson))
            {
                var jsonObj = jsonDoc.RootElement;
                List<string> row = new();

                foreach (string header in Headers)
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
    }

    public static void DisplayCsv()
    {
        foreach (string header in Headers)
        {
            Console.Write(header + ", ");
        }

        Console.WriteLine();

        foreach (var row in Rows)
        {
            foreach (string prop in row)
            {
                Console.Write(prop + ", ");
            }
            Console.WriteLine();
        }

    }

    public static string ConvertToCsvString()
    {
        // take the Headers and Rows and turn it into one big string that looks like a CSV file.
        // Headers: ["name", "age", "city"]
        //Rows:
        //  [
        //    ["john", "20", "tokyo"],
        //    ["alice", "25", "seoul"]
        //  ]

        // Headers
        var csvData = new StringBuilder();

        // join the Headers with a comma -> name,age,city
        string _Headers = String.Join(',', Headers);

        // append first the Headers cuz we need it at the top
        csvData.AppendLine(_Headers);

        // Rows
        foreach (var row in Rows)
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
        Console.WriteLine(finalStringCsv);
        return finalStringCsv;
    }

    public static void SaveCsvFile()
    {
        // write the csv string to output/result.csv,
        // if name already exists then name_2 -> name_3 -> ...
        string fileName = "result.csv";
        string folder = @".\output";
        string path = Path.Combine(folder, fileName);
        try
        {
            // check if the directory exists, if not then create directory 
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }


            string csvString = ConvertToCsvString();

            // ! TODO: change the filename if the it already exists.  

            // File.WriteAllText is a built-in static method in C# (System.IO) used to create a new file, 
            // write a string to it, and automatically close the file. 
            // if the target file already exists, the method overwrites its contents
            File.WriteAllText(path, csvString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


    }

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

            string stringJson = ReadJsonFile(path);

            if (String.IsNullOrWhiteSpace(stringJson))
            {
                Console.WriteLine("File is empty.");
                return;
            }

            using JsonDocument doc = ParseJsonIntoDocu(stringJson);

            // add contents to `Headers` -> ["heade1", "header2", ...]
            getHeaders(doc);
            // add contents to `Rows` -> [ ["row1", ...], ["row2", ...] ]
            createRows(stringJson);
            // saves csv results to /output/result.csv 
            SaveCsvFile();

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