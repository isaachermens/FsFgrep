// Fgrep docs http://unixhelp.ed.ac.uk/CGI/man-cgi?fgrep
// http://fsharpforfunandprofit.com/posts/pattern-matching-command-line/
// Utility module used to parse command line flags for the fgrep utility
// Supported flags include "-c" ;"--count"; "-i"; "--ignore-case"; "-n"; "--line-number"; "-r"; "-R"; "--recursive"; "--help"
module CommandLineParser
    // Flag types
    type CountOption = ShowCount | ShowLines
    type CaseOption = IgnoreCase | RequireCase
    type NumberOption = IncludeNumber | ExcludeNumber
    type RecurseOption = Recurse | NoRecurse
    type HelpOption = ShowHelp | NoHelp
    type IncludeFileOption = HideFile | ShowFile | NotSpecified
    type OnlyMatchingOption = EntireLine | OnlyMatch
    type InvertMatchOption = DoNotInvert | Invert

    // set up a type to represent the options
    type CommandLineOptions = {
        count: CountOption;
        ignoreCase: CaseOption;
        lineNumber: NumberOption; 
        recurse: RecurseOption;
        help: HelpOption;
        file : IncludeFileOption;
        onlyMatch : OnlyMatchingOption;
        invert : InvertMatchOption
        }
    
    // Used to identify flags
    let private supportedFlags = 
        ["-c" ;"--count"; "-i"; "--ignore-case"; "-n"; "--line-number"; "-r"; "-R"; "--recursive"; "-H"; "--with-filename"; "-h"; "--no-filename"; "-o"; "--only-matching"; "--help"
         "-v"; "--invert-match"]

    // is a flag supported?
    let isSupportedFlag flag = 
        List.exists ((=)flag) supportedFlags 
    
    /// Convert an array of command line flags into a CommandLineOptions struct
    /// Unsupported flags are handled gracefully and result in a printed error message (will not break the function)
    let parseCommandLine argv = 
        let args = List.ofArray argv // convert array to list
        // Could convert from recursion to fold, but who has the time...
        // Recursive, accumulator function
        let rec parseCommandLineRec args optionsSoFar = 
            match args with 
            // empty list means we're done.
            | [] -> 
                optionsSoFar  

            // match count flag
            | "-c"::rest | "--count"::rest -> 
                let newOptionsSoFar = { optionsSoFar with count = ShowCount}
                parseCommandLineRec rest newOptionsSoFar 

            // match ignore case flag
            | "-i"::rest | "--ignore-case"::rest -> 
                let newOptionsSoFar = { optionsSoFar with ignoreCase = IgnoreCase}
                parseCommandLineRec rest newOptionsSoFar 

            // match line numbers flag
            | "-n"::rest | "--line-number"::rest -> 
                let newOptionsSoFar = { optionsSoFar with lineNumber = IncludeNumber }
                parseCommandLineRec rest newOptionsSoFar
            // match recursive flag
            | "-r"::rest | "--recursive"::rest | "-R"::rest -> 
                let newOptionsSoFar = { optionsSoFar with recurse = Recurse }
                parseCommandLineRec rest newOptionsSoFar
            | "-H"::rest | "--with-filename"::rest -> 
                let newOptionsSoFar = { optionsSoFar with file = ShowFile }
                parseCommandLineRec rest newOptionsSoFar
            | "-h"::rest | "--no-filename"::rest -> 
                let newOptionsSoFar = { optionsSoFar with file = HideFile }
                parseCommandLineRec rest newOptionsSoFar
            | "-o"::rest | "--only-matching"::rest -> 
                let newOptionsSoFar = { optionsSoFar with onlyMatch = OnlyMatch }
                parseCommandLineRec rest newOptionsSoFar
            | "-v"::rest | "--invert-match"::rest ->
                let newOptionsSoFar = {optionsSoFar with invert = Invert}
                parseCommandLineRec rest newOptionsSoFar
            | "--help"::rest -> 
                let newOptionsSoFar = { optionsSoFar with help = ShowHelp }
                parseCommandLineRec rest newOptionsSoFar 
            // handle unrecognized options; continue recurring
            | x::xs -> 
                eprintfn "Option '%s' is unrecognized" x
                parseCommandLineRec xs optionsSoFar 
        // initiallize default options
        let defaultOptions = {
            count = ShowLines;
            ignoreCase = RequireCase;
            lineNumber = ExcludeNumber;
            recurse = NoRecurse;
            help = NoHelp;
            file = NotSpecified;
            onlyMatch = EntireLine;
            invert = DoNotInvert
        }   
        // use recursive function to parse flags
        parseCommandLineRec args defaultOptions

