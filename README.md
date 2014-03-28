fsfgrep
=======

A partial implementation of the fgrep utility for Windows. 

This project is implemented in F#.Net.

To distinguish it from the fgrep utility provided by grep for Window (http://gnuwin32.sourceforge.net/packages/grep.htm),
the executable file is named fsfgrep.exe.

Support for optional flags is not comprehensive, but a number have been implemented. See source for details.

Tests are implemented through a batch file and corresponding small file system contained in the Fgrep/Tests directory.
Output produced by running fsfgrep is compared to that produced by running standard fgrep with the same input.
