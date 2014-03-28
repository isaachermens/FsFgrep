// accepts 2 character arrays/pointers
// compares the first n characters, n = cmpLen
// returns 0 if characters match, >0 if str1 > str2, <0 if str1 < str2
int strncmp(const char* str1, const char* str2, unsigned int cmpLen)
{
	// Should perhaps protect against null pointers? 
	// the stdlib strcmp does NOT protect against it
	// and I can't think of a clear return value
	// so I will follow the example of the stdlib and ignore it.
	int index;
	char str1Char, str2Char;
	for(index = 0; index < cmpLen; index++){
		str1Char = str1[index];
		str2Char = str2[index];
		if(str1Char == 0 && str2Char == 0){
			break;
		}
		if(str1Char != str2Char){
			return str1Char - str2Char;
		}
	}
	return 0;
}

int main(int a, char **av)
{
// I ran many more tests than this :)
	strncmp("fred", "bob", 5);
	return 0;
}
