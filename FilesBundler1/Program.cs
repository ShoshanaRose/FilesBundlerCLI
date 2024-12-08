
using System;
using System.CommandLine;


// יצירת האפשרות --language
var languageOption = new Option<string[]>("--language", "List of programming languages to include (e.g., 'csharp', 'python') or 'all' for all files.")
{
    IsRequired = true // הפיכת האפשרות לחובה
};

// יצירת האפשרות --output (נתיב לפלט)
var bundleOption = new Option<FileInfo>("--output", "File path and name for the bundled file") { IsRequired = true };// הפיכת האפשרות לחובה

var noteOption = new Option<bool>("--note", "Include the source file path as a comment in the bundle");
var sortOption = new Option<string>("--sort", "Sort files by name (default) or by extension")
{
    Arity = ArgumentArity.ZeroOrOne,
};
sortOption.SetDefaultValue("name"); // ערך ברירת מחדל הוא לפי שם הקובץ

var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "Remove empty lines from the code before adding it to the bundle");
var authorOption = new Option<string>("--author", "Name of the author to include in the bundle file as a comment");

// יצירת פקודת bundle
var bundleCommand = new Command("bundle", "Bundle code files to a single file")
{
    languageOption,
    bundleOption,
    noteOption,
    sortOption,
    removeEmptyLinesOption,
    authorOption
};//bundleCommand.AddOption();

// טיפול בפקודה
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

        // אם לא נמצאו קבצים תואמים, הצגת הודעת שגיאה
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
            // רישום שם היוצר אם סופק
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

// יצירת פקודת השורש
var rootCommand = new RootCommand("Root command for File Bundler CLI"){
    bundleCommand
};//rootCommand.AddCommand(bundleCommand);
// הרצת הפקודות
await rootCommand.InvokeAsync(args);
