using Newtonsoft.Json.Linq;

namespace JsonExplorer
{
    /// <summary>
    /// JSON Explorer - A simple console application to load, search, sort,
    /// and export JSON data to XML format.
    /// Supports any JSON structure including nested and recursive objects.
    /// </summary>
    class Program
    {
        #region Fields

        /// <summary>
        /// Current working dataset (may be filtered or sorted).
        /// Each dictionary represents a flattened JSON object.
        /// </summary>
        private static List<Dictionary<string, object>> _data;

        /// <summary>
        /// Backup of the original dataset, used to restore data when filters are reset.
        /// This list is never modified after the initial JSON loading.
        /// </summary>
        private static List<Dictionary<string, object>> _originalData;

        #endregion

        #region Main Entry Point

        /// <summary>
        /// Application entry point.
        /// Displays the welcome banner, loads the JSON data, then runs the main menu loop.
        /// </summary>
        /// <param name="args">Command-line arguments (not used).</param>
        static void Main(string[] args)
        {
            // Display the welcome banner
            DisplayWelcomeBanner();

            // Load JSON data (from file or example)
            if (!LoadInitialData())
            {
                // Loading failed, exit the application
                return;
            }

            // Run the main menu loop until the user chooses to quit
            RunMainMenu();
        }

        /// <summary>
        /// Displays the application's welcome banner with cyan color.
        /// </summary>
        static void DisplayWelcomeBanner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════╗");
            Console.WriteLine("║       JSON EXPLORER       ║");
            Console.WriteLine("╚═══════════════════════════╝");
            Console.ResetColor();
        }

        /// <summary>
        /// Prompts the user for a JSON file path and loads the data.
        /// If no path is provided, loads the built-in example.
        /// On success, both _data and _originalData are populated.
        /// </summary>
        /// <returns>True if data was loaded successfully, false otherwise.</returns>
        static bool LoadInitialData()
        {
            Console.Write("\nJSON file path (or [ENTER] for example): ");
            var path = Console.ReadLine();

            try
            {
                // Use example JSON if no path was provided, otherwise read from file
                string json = string.IsNullOrWhiteSpace(path)
                    ? GetExampleJson()
                    : File.ReadAllText(path);

                _data = LoadJson(json);

                // Keep a copy of the original data so we can reset filters later
                _originalData = new List<Dictionary<string, object>>(_data);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{_data.Count} objects loaded successfully!");
                Console.ResetColor();
                Thread.Sleep(1000);

                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPress [ENTER] to quit...");
                Console.ReadLine();
                return false;
            }
        }

        /// <summary>
        /// Main menu loop. Displays the menu and dispatches to the corresponding action
        /// until the user selects "Quit".
        /// </summary>
        static void RunMainMenu()
        {
            while (true)
            {
                DisplayMainMenu();

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\nYour choice: ");
                Console.ResetColor();

                // Dispatch to the appropriate action based on user input
                switch (Console.ReadLine())
                {
                    case "1": Display(); break;
                    case "2": Search(); break;
                    case "3": Sort(); break;
                    case "4": ExportXml(); break;
                    case "5": ResetFilters(); break;
                    case "0":
                        Console.Clear();
                        return;
                    default:
                        ShowInvalidChoiceMessage();
                        break;
                }
            }
        }

        /// <summary>
        /// Renders the main menu with the current object count and the list of available actions.
        /// </summary>
        static void DisplayMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════╗");
            Console.WriteLine("║       MAIN MENU       ║");
            Console.WriteLine("╚═══════════════════════╝");
            Console.ResetColor();

            // Show how many objects are currently in memory (after any filters)
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n{_data.Count} object(s) in memory\n");
            Console.ResetColor();

            Console.WriteLine("1. Display data");
            Console.WriteLine("2. Search");
            Console.WriteLine("3. Sort");
            Console.WriteLine("4. Export to XML");
            Console.WriteLine("5. Reset filters");
            Console.WriteLine("0. Quit");
        }

        /// <summary>
        /// Displays a brief "invalid choice" message in red, then pauses briefly
        /// so the user has time to read it before the menu redraws.
        /// </summary>
        static void ShowInvalidChoiceMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nInvalid choice!");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        #endregion

        #region JSON Loading

        /// <summary>
        /// Parses a JSON string and converts it into a list of flattened dictionaries.
        /// Handles three root structures:
        /// - JSON array at root: each element becomes one entry.
        /// - JSON object containing arrays: each array element becomes one entry.
        /// - JSON object without arrays: the whole object becomes a single entry.
        /// </summary>
        /// <param name="json">The raw JSON string to parse.</param>
        /// <returns>A list of dictionaries, where each dictionary is a flattened object.</returns>
        static List<Dictionary<string, object>> LoadJson(string json)
        {
            var result = new List<Dictionary<string, object>>();
            var token = JToken.Parse(json);

            // Case 1: root is an array — flatten each element
            if (token is JArray array)
            {
                foreach (var item in array)
                    result.Add(Flatten(item));
            }
            // Case 2: root is an object — look for arrays inside
            else if (token is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    if (prop.Value is JArray arr)
                    {
                        // Flatten each element of the inner array
                        foreach (var item in arr)
                            result.Add(Flatten(item));
                    }
                }

                // Case 3: no inner array found — treat the whole object as a single entry
                if (result.Count == 0)
                    result.Add(Flatten(token));
            }

            return result;
        }

        /// <summary>
        /// Recursively flattens a JSON token into a flat dictionary using dot notation
        /// for nested objects and bracket notation for array indices.
        /// Examples:
        /// - { "user": { "name": "Alice" } } becomes { "user.name": "Alice" }
        /// - { "tags": ["a", "b"] } becomes { "tags[0]": "a", "tags[1]": "b" }
        /// </summary>
        /// <param name="token">The JSON token to flatten.</param>
        /// <param name="prefix">The current key prefix for recursion (empty at the root).</param>
        /// <returns>A flat dictionary mapping dotted keys to scalar values.</returns>
        static Dictionary<string, object> Flatten(JToken token, string prefix = "")
        {
            var dict = new Dictionary<string, object>();

            // Handle JSON objects: iterate through properties and build dotted keys
            if (token is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    // Build the full key path (e.g. "address.city")
                    var key = string.IsNullOrEmpty(prefix)
                        ? prop.Name
                        : $"{prefix}.{prop.Name}";

                    if (prop.Value is JObject || prop.Value is JArray)
                    {
                        // Recurse into nested objects/arrays and merge results
                        foreach (var kvp in Flatten(prop.Value, key))
                            dict[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        // Scalar value: store directly as a string
                        dict[key] = prop.Value.ToString();
                    }
                }
            }
            // Handle JSON arrays: use bracketed indices in keys
            else if (token is JArray array)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    var key = $"{prefix}[{i}]";

                    if (array[i] is JObject || array[i] is JArray)
                    {
                        // Recurse into nested objects/arrays
                        foreach (var kvp in Flatten(array[i], key))
                            dict[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        // Scalar value: store directly
                        dict[key] = array[i].ToString();
                    }
                }
            }

            return dict;
        }

        #endregion

        #region Menu Actions

        /// <summary>
        /// Displays all objects in the current dataset, showing each property as "key: value".
        /// Waits for user input (ENTER) before returning to the menu.
        /// </summary>
        static void Display()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════╗");
            Console.WriteLine("║       DATA DISPLAY       ║");
            Console.WriteLine("╚══════════════════════════╝");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n{_data.Count} object(s)\n");
            Console.ResetColor();

            // Iterate through each object and print its properties
            for (int i = 0; i < _data.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Object #{i + 1}");
                Console.ResetColor();

                // Print each key-value pair indented for readability
                foreach (var kvp in _data[i])
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }

                Console.WriteLine();
            }

            // Wait for user before returning to menu
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("\nPress [ENTER] to continue...");
            Console.ResetColor();
            Console.ReadLine();
        }

        /// <summary>
        /// Prompts the user for a search term and filters the current dataset
        /// to keep only objects containing that term in any of their values.
        /// Search is case-insensitive. The result replaces the current dataset
        /// (use ResetFilters to restore the original data).
        /// </summary>
        static void Search()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════╗");
            Console.WriteLine("║       SEARCH       ║");
            Console.WriteLine("╚════════════════════╝");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nSearch: ");
            Console.ResetColor();

            // Normalize input for case-insensitive comparison
            var search = Console.ReadLine().ToLower();

            // Keep only objects where at least one value contains the search term
            var results = _data.Where(obj =>
                obj.Values.Any(v => v.ToString().ToLower().Contains(search))
            ).ToList();

            _data = results;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{results.Count} result(s) found");
            Console.ResetColor();

            Thread.Sleep(1500);
        }

        /// <summary>
        /// Lets the user pick a field, then sorts the current dataset alphabetically
        /// (ascending) by that field's string value. Objects missing the field
        /// are sorted as if their value were an empty string.
        /// </summary>
        static void Sort()
        {
            // Guard clause: nothing to sort
            if (_data.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNo data to sort!");
                Console.ResetColor();
                Thread.Sleep(1500);
                return;
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════╗");
            Console.WriteLine("║       SORT       ║");
            Console.WriteLine("╚══════════════════╝");
            Console.ResetColor();

            // Use the first object's keys as the available sort fields
            var fields = _data[0].Keys.ToList();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nAvailable fields:\n");
            Console.ResetColor();

            // Display the numbered list of fields
            for (int i = 0; i < fields.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  {i + 1}. ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(fields[i]);
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nField number: ");
            Console.ResetColor();

            // Validate the selected index and apply the sort
            if (int.TryParse(Console.ReadLine(), out int idx) && idx > 0 && idx <= fields.Count)
            {
                var field = fields[idx - 1];

                // Sort by the chosen field; missing values are treated as empty strings
                _data = _data
                    .OrderBy(obj => obj.ContainsKey(field) ? obj[field].ToString() : "")
                    .ToList();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nData sorted by '{field}'");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid number!");
                Console.ResetColor();
            }

            Thread.Sleep(1500);
        }

        /// <summary>
        /// Exports the current dataset to an XML file.
        /// The XML structure is:
        /// <code>
        /// &lt;Data&gt;
        ///   &lt;Object&gt;
        ///     &lt;field_name&gt;value&lt;/field_name&gt;
        ///     ...
        ///   &lt;/Object&gt;
        /// &lt;/Data&gt;
        /// </code>
        /// Field names are sanitized to be valid XML element names.
        /// </summary>
        static void ExportXml()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════╗");
            Console.WriteLine("║       XML EXPORT       ║");
            Console.WriteLine("╚════════════════════════╝");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nFile name (default: export.xml): ");
            Console.ResetColor();

            // Use default name if user just hits ENTER
            var path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path))
                path = "export.xml";

            try
            {
                // Configure indented XML output for readability
                var settings = new System.Xml.XmlWriterSettings { Indent = true };

                using (var writer = System.Xml.XmlWriter.Create(path, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Data");

                    // Write each object as an <Object> element
                    foreach (var obj in _data)
                    {
                        writer.WriteStartElement("Object");

                        // Write each property as a child element
                        foreach (var kvp in obj)
                        {
                            // Sanitize key to ensure it's a valid XML element name
                            writer.WriteStartElement(SanitizeName(kvp.Key));
                            writer.WriteString(kvp.Value.ToString());
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement(); // </Object>
                    }

                    writer.WriteEndElement();  // </Data>
                    writer.WriteEndDocument();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{_data.Count} object(s) exported to '{path}'");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                // Catch any IO/permission/XML error and report it gracefully
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nExport error: {ex.Message}");
                Console.ResetColor();
            }

            Thread.Sleep(2000);
        }

        /// <summary>
        /// Restores the working dataset to its original state, undoing any search filters
        /// or sort operations applied during the session. The original data is preserved
        /// in _originalData and never modified after the initial load.
        /// </summary>
        static void ResetFilters()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════╗");
            Console.WriteLine("║       FILTER RESET       ║");
            Console.WriteLine("╚══════════════════════════╝");
            Console.ResetColor();

            // Restore from the untouched backup
            _data = new List<Dictionary<string, object>>(_originalData);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFilters reset! {_data.Count} object(s) restored");
            Console.ResetColor();

            Thread.Sleep(1500);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Sanitizes a string to make it a valid XML element name.
        /// Non-alphanumeric characters are replaced with underscores, and a leading
        /// underscore is added if the name starts with a digit (XML element names
        /// cannot start with a digit).
        /// </summary>
        /// <param name="name">The raw name (typically a JSON key) to sanitize.</param>
        /// <returns>A valid XML element name.</returns>
        static string SanitizeName(string name)
        {
            // Replace any non-alphanumeric character with an underscore
            var result = "";
            foreach (var c in name)
                result += char.IsLetterOrDigit(c) ? c : '_';

            // XML element names cannot start with a digit — prefix with underscore if needed
            return char.IsDigit(result[0]) ? "_" + result : result;
        }

        /// <summary>
        /// Returns the built-in example JSON used when no file path is provided.
        /// Contains a small "users" array to demonstrate the application's features.
        /// </summary>
        /// <returns>A JSON string with sample user data.</returns>
        static string GetExampleJson() => @"{
  ""users"": [
    {
      ""id"": 1,
      ""name"": ""Alice"",
      ""city"": ""Paris""
    },
    {
      ""id"": 2,
      ""name"": ""Bob"",
      ""city"": ""Lyon""
    }
  ]
}";

        #endregion
    }
}
