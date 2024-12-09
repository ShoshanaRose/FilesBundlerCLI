using System.CommandLine;

var languageOption = new Option<string[]>(
    aliases: new[] { "--language", "-l" },
    description: "List of programming languages to include (e.g., 'csharp', 'python') or 'all' for all files."){ IsRequired = true };

var bundleOption = new Option<FileInfo>(
    aliases: new[] { "--output", "-o" },
    description: "File path and name for the bundled file") { IsRequired = true };

var noteOption = new Option<bool>(
    aliases: new[] { "--note", "-n" },
    description: "Include the source file path as a comment in the bundle");

var sortOption = new Option<string>(
    aliases: new[] { "--sort", "-s" },
    description:"Sort files by name (default) or by extension"){ Arity = ArgumentArity.ZeroOrOne,};
    sortOption.SetDefaultValue("name"); // ערך ברירת מחדל הוא לפי שם הקובץ

var removeEmptyLinesOption = new Option<bool>(
    aliases: new[] { "--remove-empty-lines", "-r" },
    description: "Remove empty lines from the code before adding it to the bundle");

var authorOption = new Option<string>(
    aliases: new[] { "--author", "-a" },
    description: "Name of the author to include in the bundle file as a comment");


var bundleCommand = new Command("bundle", "Bundle code files to a single file")
{
    languageOption,
    bundleOption,
    noteOption,
    sortOption,
    removeEmptyLinesOption,
    authorOption
};

bundleCommand.SetHandler(async (string[] language, FileInfo output, bool note, string sort, bool removeEmptyLines, string author) =>
{
    try
    {
        // קביעת נתיב הקובץ: אם רק שם קובץ ניתן, שמור בתיקיה הנוכחית
        string outputFilePath = output.DirectoryName == null
            ? Path.Combine(Directory.GetCurrentDirectory(), output.Name)
            : output.FullName;

        // וידוא יצירת נתיב תקין או בדיקת קובץ קיים
        if (!Directory.Exists(Path.GetDirectoryName(outputFilePath)))
        {
            throw new DirectoryNotFoundException("The specified directory does not exist.");
        }

        // קריאת כל הקבצים בתיקיה הנוכחית
        var currentDirectory = Directory.GetCurrentDirectory();
        var allFiles = Directory.GetFiles(currentDirectory);

        // סינון הקבצים לפי שפות או כל הקבצים אם המילה 'all'
        // במקרה של שפות מרובות מופרדות בפסיקים
        var languageList = language.Contains("all", StringComparer.OrdinalIgnoreCase)
            ? new string[] { "all" }
            : language[0].Split(',').Select(lang => lang.Trim().ToLower()).ToArray();

        // המשך המיון של הקבצים
        var codeFiles = languageList.Contains("all", StringComparer.OrdinalIgnoreCase)
            ? allFiles
            : allFiles.Where(file =>
            {
                var extension = Path.GetExtension(file).TrimStart('.').ToLower();
                return languageList.Contains(extension, StringComparer.OrdinalIgnoreCase);
            }).ToArray();
        if (!codeFiles.Any())
        {
            Console.WriteLine("No matching code files found.");
            return;
        }

        // מיון הקבצים לפי האפשרות שנבחרה
        codeFiles = sort.ToLower() switch
        {
            "extension" => codeFiles.OrderBy(file => Path.GetExtension(file)).ToArray(),
            _ => codeFiles.OrderBy(file => Path.GetFileName(file)).ToArray()
        };

        // כתיבת קובץ הפלט
        using (var outputFile = new StreamWriter(outputFilePath))
        {
            if (!string.IsNullOrEmpty(author))
            {
                outputFile.WriteLine($"// Author: {author}");
            }

            // רישום כל קובץ בקובץ הפלט
            foreach (var file in codeFiles)
            {
                if (note)
                {
                    string relativePath = Path.GetRelativePath(currentDirectory, file);
                    outputFile.WriteLine($"// Source: {relativePath}");
                }

                outputFile.WriteLine($"// File: {Path.GetFileName(file)}");

                // קריאה לתוכן הקובץ
                string fileContent = File.ReadAllText(file);

                // הסרת שורות ריקות אם האפשרות נבחרה
                if (removeEmptyLines)
                {
                    fileContent = string.Join("\n", fileContent
                        .Split('\n')
                        .Where(line => !string.IsNullOrWhiteSpace(line)));
                }

                // כתיבת התוכן לקובץ המאוחד
                outputFile.WriteLine(fileContent);
                outputFile.WriteLine();
            }
        }
        Console.WriteLine($"Files bundled successfully into {output.FullName}");
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error: File path is invalid");
    }
    catch (IOException ioEx)
    {
        Console.WriteLine($"I/O Error: {ioEx.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    await Task.CompletedTask;
}, languageOption, bundleOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);


// יצירת פקודת create-rsp (ליצירת קובץ תגובה)
var createRspCommand = new Command("create-rsp", "Create a response file with pre-defined options for bundle command");//"יצירת קובץ תגובה עם אפשרויות קבועות לפקודת bundle"
                                                                                                                      
createRspCommand.SetHandler(async() =>
{
    try
    {
        // שאלות למשתמש
        Console.WriteLine("Please enter the languages ( 'csharp, python, cs, txt') or 'all' for all files:");
        string languageInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(languageInput) ||
            (!languageInput.Equals("all", StringComparison.OrdinalIgnoreCase) &&
            !languageInput.Split(',').All(lang => !string.IsNullOrWhiteSpace(lang.Trim()))))
        {
            Console.WriteLine("Invalid input for languages. Please enter a valid list of languages or 'all'.");
            return;
        }


        Console.WriteLine("Please enter the output file path (e.g., 'bundle.rsp'):");
        string outputPath = Console.ReadLine();

        // אם המשתמש לא סיפק נתיב, השתמש בנתיב ברירת מחדל
        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = "bundle.rsp";
        }

        Console.WriteLine("Do you want to include the source file path as a comment? (y/n):");
        bool noteAnswer = Console.ReadLine()?.ToLower() == "y";

        Console.WriteLine("How would you like to sort the files? (name/extension, default is 'name'):");
        string sortAnswer = Console.ReadLine();
        if (string.IsNullOrEmpty(sortAnswer)) sortAnswer = "name";  // ברירת מחדל היא לפי שם
        if (sortAnswer != "name" && sortAnswer != "extension")
        {
            Console.WriteLine("Invalid sort option. Please enter 'name' or 'extension'.");
            return;
        }

        Console.WriteLine("Do you want to remove empty lines from the code? (y/n):");
        bool removeEmptyLinesAnswer = Console.ReadLine()?.ToLower() == "y";

        Console.WriteLine("Enter the author's name (optional):");
        string authorName = Console.ReadLine();

        // עכשיו ייצור את קובץ ה-RSP עם הערכים שהוזנו
        using (var rspFile = new StreamWriter(outputPath))
        {
            rspFile.WriteLine($"--language {languageInput}");
            rspFile.WriteLine($"--output {outputPath}");
            if (noteAnswer)
                rspFile.WriteLine("--note");
            if (!string.IsNullOrEmpty(sortAnswer))
                rspFile.WriteLine($"--sort {sortAnswer}");
            if (removeEmptyLinesAnswer)
                rspFile.WriteLine("--remove-empty-lines");
            if (!string.IsNullOrEmpty(authorName))
                rspFile.WriteLine($"--author {authorName}");
        }

        Console.WriteLine($"Response file created successfully at {outputPath}. You can now run `bundle` with this file.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
});

// יצירת פקודת השורש
var rootCommand = new RootCommand("Root command for File Bundler CLI"){
bundleCommand,
createRspCommand
};
// הרצת הפקודות
await rootCommand.InvokeAsync(args);