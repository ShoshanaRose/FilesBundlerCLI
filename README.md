# File Bundler CLI

A simple tool for bundling code files from multiple programming languages into a single file.

## Requirements
- .NET 6 or higher

## Installation
1. Clone the repository.
2. Build the project with `dotnet build`.
3. Run the tool with `dotnet run -- [options]`.

## Options (for detailed options, see [More Info](#more-info))
- `--language`: Specify programming languages to include (e.g., `csharp`, `python`, or `all`).
- `--output`: Define output file path and name.
- `--sort`: Sort files by name or extension.
- `--remove-empty-lines`: Remove empty lines from the code.
- `--author`: Add author’s name as a comment.

Functionality Overview
1. Creating Command-Line Options:
The --language option allows the user to select which files to include.
The --output option defines the output file name.
Other options for sorting, adding comments, removing empty lines, and specifying the author.
2. File Reading and Writing:
The tool scans all files in the current directory and filters them based on the selected programming languages.
Files are written to the output file in the selected order (by name or extension).
3. Error Handling:
The tool checks if the output file path is valid.
It handles errors such as missing files, read/write issues, or invalid paths.
