@ECHO off
ECHO Starting tests

ECHO Testing single simple file
fsfgrep test 1.txt > result1.txt
fgrep test 1.txt > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing single file with forced filename display
fsfgrep -H test 1.txt > result1.txt
fgrep -H test 1.txt > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing multiple simple files
fsfgrep test 1.txt 2.txt > result1.txt
fgrep test 1.txt 2.txt > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing line numbers
fsfgrep -n assemblyIdentity 1.txt 2.txt app.txt > result1.txt
fgrep -n assemblyIdentity 1.txt 2.txt app.txt > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing count
fsfgrep --count assembly 1.txt 2.txt app.txt > result1.txt
fgrep --count assembly 1.txt 2.txt app.txt > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing insensitivity
fsfgrep -i assembly 1.txt 2.txt app.txt git.txt > result1.txt
fgrep -i assembly 1.txt 2.txt app.txt git.txt > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing recursiveness
fsfgrep -r a 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r a 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing recursion and line numbers
fsfgrep -r -n b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r -n b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing recursion and counts
fsfgrep -r -c c 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r -c c 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing recursion and ignore case
fsfgrep -r -i b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r -i b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing counts and ignore case
fsfgrep -c -n b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt 2> error1.txt
fgrep -c -n b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt 2> error2.txt
diff result1.txt result2.txt
diff error1.txt error2.txt
del result1.txt result2.txt
del error1.txt error2.txt

ECHO Testing recursion, ignore case, and line numbers
fsfgrep -r -n -i b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r -n -i b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing recursion, ignore case, line numbers, and counts
fsfgrep -r -n -i -c b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r -n -i -c b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing recursion, ignore case, line numbers, and filename hiding
fsfgrep -r -n -i -c -h b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r -n -i -c -h b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing recursion, ignore case, line numbers, and printing only matches
fsfgrep -r -n -i -c -o b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r -n -i -c -o b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing recursion, ignore case, line numbers, and inverse matching
fsfgrep -r -n -i -c -v b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r -n -i -c -v b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Testing recursion, ignore case, line numbers, and inverse matching long flag
fsfgrep -r -n -i -c -v b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result1.txt
fgrep -r -n -i -c -v b 1.txt 2.txt app.txt git.txt folder1 folder2 rootCopy > result2.txt
diff result1.txt result2.txt
del result1.txt result2.txt

ECHO Tests finished
PAUSE