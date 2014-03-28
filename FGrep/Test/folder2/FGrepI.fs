module FGrepI
    open System
    open System.IO
    open CommandLineParser

    // Extend the string class with a case-insensitive variant of the string.Contains(string) method
    type private System.String with
       member s1.icontains(s2: string) =
        s1.ToLowerInvariant().Contains(s2.ToLowerInvariant())

    // If the line number option is set in $option
    //    return the string: "$lineNumber:"
    // Else return an empty string
    let private createLineNumber options lineNumber = 
        match options with
        | {CommandLineOptions.lineNumber = IncludeNumber} -> lineNumber.ToString() + ":"
        | _ -> ""

    let private tryFileName options (fileName : string) = 
        match options with
        | {CommandLineOptions.file = ShowFile} -> fileName.Replace(@"\", "/") + ":" // Note: Changing Slash seperators to align with grep for Windows tool being used for testing
        | _ -> ""
    
    // Create a string consisting of $fileName:$lineNumber
    // The line number is only included if the line number option is set in $options    
    let private createPrefix options lineNumber fileName = 
        (tryFileName options fileName) + (createLineNumber options lineNumber)
    
    // Determine whether $line contains $pattern
    // If the ignore case option is set in $options, a case insensitive comparison is used
    // Otherwise, a case senstive comparison is used         
    let private compareStrings options (line : string) pattern =
        match options with 
        | {CommandLineOptions.ignoreCase = IgnoreCase} -> line.icontains(pattern)
        | {CommandLineOptions.ignoreCase = RequireCase} -> line.Contains(pattern)
    
    // If the -c (count) option is not specified in $options, print the provided line
    // Otherwise do nothing
    // return (true, 1) regardless
    let private tryPrintLine options display = 
        match options with 
        | {CommandLineOptions.count = ShowLines} -> (printfn "%s" display); (true, 1)
        | {CommandLineOptions.count = ShowCount} -> (true, 1)

    // If the -c option is set in $options:
    //   If a file path was provided, prefix the count with the file path
    //   Else simply print the count
    // Else do nothing
    let private tryPrintCount options fileName count = 
        let prefix = match fileName with
                     | null -> ""
                     | x when x = "" -> ""
                     | _ -> fileName + ":"
        match options with 
        | {CommandLineOptions.count = ShowLines} -> ()
        | {CommandLineOptions.count = ShowCount} -> (printfn "%s%s" prefix (count.ToString()))

    // Check to see whether $testLine is not null and contains $pattern
    // Returns false if the line is null, true otherwise
    // if $testLine contains $pattern, the line will be printed with an appropriate prefix, as specified by $options
    let private testLine options pattern line prefix = 
        match line with
        | null -> (false, 0)
        | (inputString : string) -> (if (compareStrings options inputString pattern) then (tryPrintLine options (prefix + line)) else (true, 0)) // todo handle count flag
   
    // Read line-by-line from stdinput until an end of file is detected
    // Each line will be checked for $pattern based on the flags in $options
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
    
    // Determine whether the path points to an existing file or directory
    // No distinction is made between files and directories
    // Invalid paths will result in a return value of true, according to the documentation for the .Exists() methods
    let private exists path =
        File.Exists(path) || Directory.Exists(path)

    // Determine whether a path points to a file directory
    let private isDirectory path = 
        (File.GetAttributes(path) &&& FileAttributes.Directory) = FileAttributes.Directory
    
    // Builds a list of all files present in $pathss
    // If the recursive option is set, directories will be scanned for all contained files
    // Else, any directories contained in $paths will be excluded from the result 
    let private getFiles options paths =
        let pathList = List.ofArray paths

        // Recursively retrieve paths to all files within the provided directory
        let rec getFiles (dirInfo : DirectoryInfo) (basePath : string) =
            let files = dirInfo.GetFiles() |> List.ofArray |> List.map (fun info -> basePath + "\\" + info.Name)
            let dirs = dirInfo.GetDirectories() |> List.ofArray
            let dirFiles = dirs |> List.collect (fun el -> getFiles el (basePath + "\\" + el.ToString()))
            List.append dirFiles files

        let files = pathList |> List.filter (fun path -> (exists path) && not (isDirectory path))
        let dirs = pathList |> List.filter (fun path -> (exists path) && (isDirectory path)) |> List.map (fun dirPath -> new DirectoryInfo(dirPath))

        match options with 
        | {CommandLineOptions.recurse = Recurse} -> List.append files (dirs |> List.collect (fun el -> getFiles el (el.ToString())))
        | {CommandLineOptions.recurse = NoRecurse} -> files
    
    // Read a file line-by-line until an end of file is detected
    // Each line is checked for $pattern based on the flags specified in $options                                       
    let private testFile options pattern (filePath : string) =  
        let mutable continueLooping = true
        use streamRead = new StreamReader(filePath)
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
    
    // Uses the specified options and pattern to search the files (and optionally directories)
    //   Specified in the filePaths parameter
    let private grepOverFiles options pattern filePaths = 
        let files = getFiles options filePaths
        let options = if files.Length > 1 then {options with file = ShowFile} else options
        ignore (files |> List.map (fun path -> testFile options pattern path))

    let private formatPrompt = "Usage: fgrep [OPTION]... PATTERN [FILE]..."
    let private helpPrompt = formatPrompt + "\r\n\
                              Files are scanned and matches are printed as they are found. (For extra credit, Mr. Professor)\r\n\
                              Supported command line options:\r\n\
                              Compact flag form (e.g., -cinr) is not currently supported
                               -c, --count: Display the number of matches found in each file instead of displaying matching lines\r\n\
                               -i, --ignore-case: Perform case insensitive comparisons using PATTER\r\n\
                               -n, --line-number: Display the line number of each matching line in output. No effect if used with -c\r\n\
                               -r, --recursive, -R: Read all files under each directory, recursively\r\n\
                               --help: view help information\r\n"
        
    // FGrep takes a list of command line arguments
    //  containing one or more optional flags, a literal pattern to match, and one or more files
    // Any optional flags are parsed by the CommandLineParser module
    // If a pattern is not provided, the function prints a formatting prompt and returns
    // If a pattern is provided, but files are not, stdin will be read and searched
    // If a pattern is provided as well as files, the files (and optionally directories) will be read and searched
    let FGrep args = 
        // determine which options were set
        let flags = args |> Array.filter isSupportedFlag
        let options = parseCommandLine flags
        let nonFlags = args |> Array.filter (fun elem -> not (isSupportedFlag elem))
        match options with
        | {CommandLineOptions.help = ShowHelp} -> printfn "%s" helpPrompt; ()
        | {CommandLineOptions.help = NoHelp} ->
            match nonFlags.Length with
            | x when x < 1 -> (printfn "%s" formatPrompt); () // print prompt and return unit
            | 1 -> grepOverInput options nonFlags.[0] 
            | x when x > 1 -> grepOverFiles options nonFlags.[0] (nonFlags.[1..nonFlags.Length-1])
            | _ -> (printfn "%s" formatPrompt); () 