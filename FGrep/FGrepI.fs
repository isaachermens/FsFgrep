// Fgrep docs http://unixhelp.ed.ac.uk/CGI/man-cgi?fgrep
// Implementation file for the FGrep utility
module FGrepI
    open System
    open System.IO
    open CommandLineParser

    // Extend the string class 
    type private System.String with
       //with a case-insensitive variant of the string.Contains(string) method
       member s1.icontains(s2: string) =
        s1.ToLowerInvariant().Contains(s2.ToLowerInvariant())
       //with an IndexOf method that considers the IgnoreCase command line option
       member s1.optionalIndexOf(s2: string, options:CommandLineOptions) =
        let comparisonType = match options with 
                             | {CommandLineOptions.ignoreCase = IgnoreCase} -> StringComparison.InvariantCultureIgnoreCase
                             | {CommandLineOptions.ignoreCase = RequireCase} -> StringComparison.InvariantCulture
        s1.IndexOf(s2, comparisonType)


    /// If the line number option is set in $option
    ///    return the string: "$lineNumber:"
    /// Else return an empty string
    let private createLineNumber options lineNumber = 
        match options with
        | {CommandLineOptions.lineNumber = IncludeNumber} -> sprintf "%s:" (lineNumber.ToString())
        | _ -> ""

    /// If we need to print the filename, adjust slashes and append a semi-colon.
    /// Otherwise return an empty string
    let private tryFileName options (fileName : string) = 
        match options with
        | {CommandLineOptions.file = ShowFile} -> sprintf "%s:" <| fileName.Replace(@"\", "/") // Note: Changing Slash seperators to align with grep for Windows tool being used for testing
        | _ -> ""
    
    /// Create a string consisting of $fileName:$lineNumber
    /// The line number is only included if the line number option is set in $options    
    let private createPrefix options lineNumber fileName = 
        sprintf "%s%s" (tryFileName options fileName) (createLineNumber options lineNumber)
    
    /// Determines whether $line contains $pattern.
    /// If the ignore case option is set in $options, a case insensitive comparison is used.
    /// Otherwise, a case senstive comparison is used.
    /// If the -v option is set in $options, the result of the comparison if negated.
    let private compareStrings options (line : string) pattern =
        let invertIfNecessary x = match options with 
                                  | {CommandLineOptions.invert = Invert} -> not x
                                  | {CommandLineOptions.invert = DoNotInvert} -> x
        match options with 
        | {CommandLineOptions.ignoreCase = IgnoreCase} -> line.icontains(pattern)
        | {CommandLineOptions.ignoreCase = RequireCase} -> line.Contains(pattern)
        |> invertIfNecessary

    /// If the -c (count) option is not specified in $options
    ///   print either the provided line or just the matching portion of the line (depending on how the onlyMatch field of $options is set).
    /// Otherwise do nothing.
    /// returns (true, 1) regardless
    let private tryPrintLine options display = 
        match options with 
        | {CommandLineOptions.count = ShowLines} -> (printfn "%s" display); (true, 1)
        | {CommandLineOptions.count = ShowCount} -> (true, 1)

    /// If the -c option is set in $options:
    ///   If a file path was provided, prefix the count with the file path/
    ///   Else simply print the count/
    /// Else do nothing/
    let private tryPrintCount options fileName count = 
        let prefix = match fileName with
                     | null -> ""
                     | x when x = "" -> ""
                     | _ -> tryFileName options fileName
        match options with 
        | {CommandLineOptions.count = ShowLines} -> ()
        | {CommandLineOptions.count = ShowCount} -> printfn "%s%s" prefix <| count.ToString()

    /// If the -o option is set in $options:
    ///   Return the substring of $line that matches $pattern, considering any flags set in $options.
    /// Else returns $line.
    /// Behavior is undefined if $line does not contain $pattern under the current options.
    let private getDesiredLineContent options (line : string) (pattern : string) = 
        match options with
        | {CommandLineOptions.onlyMatch = EntireLine} -> line
        | {CommandLineOptions.onlyMatch = OnlyMatch} -> let startIndex = line.optionalIndexOf(pattern, options)
                                                        line.[startIndex..(startIndex + pattern.Length - 1)]

    /// Check to see whether $testLine is not null and contains $pattern.
    /// Returns false if the line is null, true otherwise.
    /// if $testLine contains $pattern, the line will be printed with an appropriate prefix, as specified by $options.
    let private testLine options pattern line prefix = 
        match line with
        | null -> (false, 0)
        | (inputString : string) -> (if (compareStrings options inputString pattern)
                                     then tryPrintLine options <| sprintf "%s%s" prefix (getDesiredLineContent options line pattern)
                                     else (true, 0))
   
    /// Read line-by-line from stdinput until an end of file is detected.
    /// Each line will be checked for $pattern based on the flags in $options.
    let private grepOverInput options pattern = 
        let mutable continueLooping = true
        let mutable line = System.Console.ReadLine()
        let mutable lineNumber = 1
        let mutable count = 0
        while continueLooping do 
            let (shouldContinue, increment) = testLine options pattern line (createLineNumber options lineNumber)
            continueLooping <- shouldContinue
            count <- count + increment
            line <- System.Console.ReadLine()
            lineNumber <- lineNumber + 1
        tryPrintCount options "" count
    
    /// Determine whether the path points to an existing file or directory.
    /// No distinction is made between files and directories.
    /// Invalid paths will result in a return value of true, according to the documentation for the .Exists() methods.
    let private exists path =
        File.Exists(path) || Directory.Exists(path)

    // Determine whether a path points to a file directory
    let private isDirectory path = 
        (File.GetAttributes(path) &&& FileAttributes.Directory) = FileAttributes.Directory
    
    /// Builds a list of all files present in $paths.
    /// If the recursive option is set, directories will be scanned for all contained files.
    /// Else, any directories contained in $paths will be excluded from the result 
    let private getFiles options paths =
        let pathList = List.ofArray paths

        // Recursively retrieve paths to all files within the provided directory
        let rec getFiles (dirInfo : DirectoryInfo) (basePath : string) =
            let files = dirInfo.GetFiles() 
                        |> List.ofArray 
                        |> List.map (fun info -> sprintf "%s\\%s" basePath info.Name)
            let dirs = dirInfo.GetDirectories() |> List.ofArray
            let dirFiles = dirs |> List.collect (fun el -> getFiles el <| sprintf "%s\\%s" basePath (el.ToString()))
            dirFiles @ files

        let files = pathList |> List.filter (fun path -> (exists path) && not <| isDirectory path)
        let dirs = pathList |> List.filter (fun path -> (exists path) && (isDirectory path)) 
                            |> List.map (fun dirPath -> new DirectoryInfo(dirPath))

        match options with 
        | {CommandLineOptions.recurse = Recurse} -> files @ (dirs |> List.collect (fun el -> getFiles el (el.ToString())) |> List.sort)
        | {CommandLineOptions.recurse = NoRecurse} -> pathList
    
    /// Read a file line-by-line until an end of file is detected.
    /// Each line is checked for $pattern based on the flags specified in $options                                       
    let private testFile options pattern (filePath : string) =  
        match filePath with 
        | x when (exists filePath) = false -> eprintfn "fgrep: %s: No such file or directory" filePath
        | x when (isDirectory filePath) -> eprintfn "fgrep: %s: Is a directory" filePath
        | _ ->  use streamRead = new StreamReader(filePath)
                let mutable continueLooping = true
                let mutable line = streamRead.ReadLine()
                let mutable lineNumber = 1
                let mutable count = 0
                while continueLooping do
                    let (shouldContinue, increment) = (testLine options pattern line (createPrefix options lineNumber filePath))
                    continueLooping <-  shouldContinue
                    count <- count + increment
                    line <- streamRead.ReadLine()
                    lineNumber <- lineNumber + 1
                tryPrintCount options filePath count

    let private shouldShowFiles options num = 
        match options with
        | {CommandLineOptions.file = NotSpecified} -> if num > 1 then {options with file = ShowFile} else {options with file = HideFile}
        | _ -> options
        
    
    /// Uses the specified options and pattern to search the files (and optionally directories)
    ///   specified in the filePaths parameter
    let private grepOverFiles options pattern filePaths = 
        let files = getFiles options filePaths
        // If the user did not explicitly request file names to be hidden or shown, base it on the number of files
        let options = shouldShowFiles options files.Length
        ignore (files |> List.map (fun path -> testFile options pattern path))

    let private formatPrompt = "Usage: fgrep [OPTION]... PATTERN [FILE]..."
    let private helpPrompt = sprintf "%s%s" formatPrompt "\r\n\
                              Files are scanned and matches are printed as they are found. (For extra credit, Mr. Professor)\r\n\
                              File paths are printed in the Unix/Linux fashion rather than the Windows fasion. 
                              Supported command line options:\r\n\
                              Compact flag form (e.g., -cinr) is not currently supported
                               -c, --count: Display the number of matches found in each file instead of displaying matching lines\r\n\
                               -i, --ignore-case: Perform case insensitive comparisons using PATTER\r\n\
                               -n, --line-number: Display the line number of each matching line in output. No effect if used with -c\r\n\
                               -r, --recursive, -R: Read all files under each directory, recursively\r\n\
                               -H, --with-filename: Print the filename for each match. Even if only one file is provided\r\n\
                               -h, --no-filename:  Suppress	the  prefixing	of  filenames  on output when multiple files are searched.\r\n\
                               -o, --only-matching: Show only the part of a matching line that matches PATTERN.\r\n\
                               -v, --invert-match: Invert the sense of matching, to select non-matching lines.
                               --help: view help information\r\n"
        
    /// FGrep takes a list of command line arguments
    ///  containing one or more optional flags, a literal pattern to match, and one or more files
    /// Any optional flags are parsed by the CommandLineParser module
    /// If a pattern is not provided, the function prints a formatting prompt and returns
    /// If a pattern is provided, but files are not, stdin will be read and searched
    /// If a pattern is provided as well as files, the files (and optionally directories) will be read and searched
    let FGrep args = 
        // determine which options were set
        let (flags, nonFlags) = args |> Array.partition isSupportedFlag
        let options = parseCommandLine flags
        match options with
        | {CommandLineOptions.help = ShowHelp} -> printfn "%s" helpPrompt; ()
        | {CommandLineOptions.help = NoHelp} ->
            match nonFlags.Length with
            | x when x < 1 -> (printfn "%s" formatPrompt); () // print prompt and return unit
            | 1 -> grepOverInput options nonFlags.[0] 
            | x when x > 1 -> grepOverFiles options nonFlags.[0] (nonFlags.[1..nonFlags.Length-1])
            | _ -> (printfn "%s" formatPrompt); () 