using System.Data;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Nodes;

class Program {
    
    public static string ReadJsonFile(string path) 
    {
        // read the entire file content of the JSON as a string but not an actual JSON file.
        return File.ReadAllText(path);
    }

    public static JsonDocument ParseJsonIntoDocu(string jsonString) {
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

    public static List<string> getHeaders(JsonDocument json) {
        // ! TODO: handle exceptions -> ArgumentOutOfRangeException, InvalidOperationException, others...
        // parse json into a JsonObject
        //JsonObject parsedJson = JsonNode.Parse(stringJson).AsObject();
        List<string> headers = new List<string> { };

        // 1. Read JSON file
        // 2. Parse JSON into a readable object
        // 3. Get headers

            var root = json.RootElement;

            // if the json file is just a single object {}
            // then do:
            if (root.ValueKind == JsonValueKind.Object) 
            {
            // JsonNode.Parse -> converts a JSON string directly into a JsonObject, which allows to manipulate the data as a key-value pair collection.
            //JsonObject parsedJson = JsonDocument.Parse().AsObject();
            JsonObject? jsonObj = JsonObject.Create(root);

                // iterate through to the json object and add its Key (headers)
                
                foreach (var property in jsonObj) {
                    headers.Add(property.Key);
                }
                return headers;
            }

            // suppose stringJson is an [] (array) of JSONs we get the first object's keys/headers
            var firstObject = root[0]; // the first object from the array (root element)

            // EnumerateObject() -> converts a JSON object into a searchable, loopable collection of key - value properties.
            foreach (JsonProperty property in firstObject.EnumerateObject()) {
                headers.Add(property.Name);
            }

        return headers;

    }

    public static List<List<string>>  createRows(List<string> headers, string stringJson) {
        List<List<string>> rows = new();
        using(var jsonDoc = JsonDocument.Parse(stringJson)) {
            if (!isRootAnArray(stringJson)) {
                var jsonObj = jsonDoc.RootElement;
                List<string> row = new();

                foreach (string header in headers) {
                    string property = jsonObj.GetProperty(header).ToString();
                    row.Add(property);
                }
                rows.Add(row);
                return rows;
                }


            // json type -> array
            foreach (var jsonObj in jsonDoc.RootElement.EnumerateArray()) {
                // TODO: handle special characters in CSV cells (commas, qoutes, line breaks)
                if (jsonObj.ValueKind == JsonValueKind.Object)
                    {       
                        List<string> row = new();
   
                        foreach (string header in headers) {
                            string value= jsonObj.GetProperty(header).ToString();
                            // handle empty value
                            if (String.IsNullOrEmpty(value)) {
                                row.Add("");
                            } else {
                                row.Add(value);                        
                            }

                        }

                    rows.Add(row);
                }                
            }
        }
        return rows;
    }

    public static void DisplayCsv(List<string> headers, List<List<string>> rows) {
        foreach(string header in headers) {
            Console.Write(header + ", ");
        }

        Console.WriteLine();

        foreach (var row in rows)
        {
            foreach (string prop in row)
            {
                Console.Write(prop + ", ");
            }
            Console.WriteLine();
        }

    }

    public static string ConvertToCsvString(List<string> headers, List<List<string>> rows) {
        // take the headers and rows and turn it into one big string that looks like a CSV file.
        // Headers: ["name", "age", "city"]
        //Rows:
        //  [
        //    ["john", "20", "tokyo"],
        //    ["alice", "25", "seoul"]
        //  ]

        // headers
        string headersString = "";
        foreach (string header in headers) {
            headersString += header +",";
        }

        headersString += "\n";

        string rowsString = "";
        // rows
        foreach(var row in rows) {
            foreach(string value in row) {
                // if the value contains a comma (New York, USA) then wrapp it in qoutes ("New York, USA")
                if (value.Contains(",") || value.Contains("\"")) {
                    rowsString += $"\"{value}\"" + ",";
                } else {
                    rowsString += value + ",";
                }

            }
            rowsString += "\n";
        }

        string csvString = String.Concat(headersString, rowsString);
        Console.WriteLine(csvString);

        return ""; 
    }

    public static void Main(string[] args) {
        Console.Write("Current Directory: ");
        Console.Write(Directory.GetCurrentDirectory());
        Console.WriteLine();
      
  
        try
        {
            // parsing JSON,
            // i misunderstood these methods so im commenting it out.
            //  1. JsonNode.Parse converts a JSON string into a mutable, readable DOM object.
            //  2. JsonSerializer.Serialize converts a C# object into a JSON string

            //var serialized = JsonSerializer.Serialize(fileContent);
            //var parsedJson = JsonNode.Parse(fileContent).AsObject();

            // print out contents
            //Console.WriteLine(fileContent
            
            // if directory is jsontocsv/jsontocsv then input/sample.json filepath would work cuz its inside the directory,
            // but if the directory comes from /bin/Debug/... then it means were running from inside the bin/ and we'll need 
            // to go up three directories to reach the input/ file : ..\..\..\
            string path = @"..\..\..\input\sample.json";

            string file = ReadJsonFile(path);

            if (String.IsNullOrWhiteSpace(file)) {
                Console.WriteLine("File is empty.");
                return;
            }

            using JsonDocument doc = ParseJsonIntoDocu(file);
            var headers = getHeaders(doc);
            var rows = createRows(headers, file);

            ConvertToCsvString(headers, rows);

            // TODO: before parsing check for any unsuporrted JSON
            // formats (nested objects/arrays, ...) and send an error message.
            //Console.WriteLine("---- Parsed JSON ----");
            //Console.WriteLine(parsedJson);


            // Create a list of column names(headers)

            //Console.WriteLine("---isRootAnArray---");
            //Console.WriteLine(isRootAnArray(fileContent));
        } catch(JsonException e) {
            // Captures JSON formatting, depth limits, or conversion issues
            Console.WriteLine($"JSON invalid: {e.Message}"); 
            Console.WriteLine($"Path: {e.Path}");
        } catch(FileNotFoundException e) {
            Console.WriteLine($"File not found error: {e.Message}");
        }
    }

}