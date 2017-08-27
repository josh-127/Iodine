## class ```BigInt``` ()
___
An arbitrary size integer
## class ```Dict``` ([values])
___
A dictionary containing a list of unique keys and an associated value
### func ```clear``` ()
Clears the dictionary, removing all items.
### func ```get``` (key)
Returns the value specified by [key], raising a KeyNotFound exception if the given key does not exist.
### func ```remove``` (key)
Removes a specified entry from the dictionary, raising a KeyNotFound exception if the given key does not exist.
### func ```contains``` (key)
Tests to see if the dictionary contains a key, returning true if it does.
### func ```set``` (key, value)
Sets a key to a specified value, if the key does not exist, it will be created.
## class ```File``` ()
___
An object supporting read or write operations (Typically a file)
### func ```readln``` ()
Reads a single line from the underlying stream.
### func ```writeln``` (obj)
Writes an object to the stream, appending a new line character to the end of the file.
### func ```write``` (obj)
Writes an object to the underlying stream.
### func ```readall``` ()
Reads all text.
### func ```read``` (n)
Reads [n] bytes from the underlying stream.
### func ```close``` ()
Closes the stream.
### func ```flush``` ()
Flushes the underlying stream.
## class ```List``` ()
___
A mutable sequence of objects
### func ```reduce``` (callable, default)
Reduces all members of the specified iterable by applying the specified callable to each item left to right. The callable passed to reduce receives two arguments, the first one being the result of the last call to it and the second one being the current item from the iterable.
### func ```each``` (func)
Iterates through each element in the collection.
### func ```last``` (value)
Returns the last item in this collection.
### func ```map``` (callable)
Iterates over the specified iterable, passing the result of each iteration to the specified callable. The result of the specified callable is added to a list that is returned to the caller.
### func ```prepend``` (item)
Prepends an item to the beginning of the list.
### func ```find``` (item)
Returns the index of the first occurance of the supplied argument, returning -1  if the supplied argument cannot be found.
### func ```filter``` (callable)
Iterates over the specified iterable, passing the result of each iteration to the specified callable. If the callable returns true, the result is appended to a list that is returned to the caller.
### func ```rfind``` (item)
Returns the index of the last occurance of the supplied argument, returning -1  if the supplied argument cannot be found.
### func ```contains``` (item)
Returns true if the supplied argument can be fund within the list.
### func ```remove``` (item)
Removes an item from the list, raising a KeyNotFound exception if the list does not contain [item].
### func ```append``` (*args)
Appends each argument to the end of the list
### func ```removeat``` (index)
Removes an item at a specified index.
### func ```index``` (item)
Returns the index of the first occurance of the supplied argument, raising a KeyNotFound exception  if the supplied argument cannot be found.
### func ```first``` (value)
Returns the first item in this collection.
### func ```clear``` ()
Clears the list, removing all items from it.
### func ```rindex``` (item)
Returns the index of the last occurance of the supplied argument, raising a KeyNotFound exception  if the supplied argument cannot be found.
### func ```appendrange``` (iterable)
Iterates through the supplied arguments, adding each item to the end of the list.
### func ```discard``` (item)
Removes an item from the list, returning true if success, otherwise, false.
## class ```Str``` ()
___
An immutable string of UTF-16 characters
### func ```isalpha``` ()
Returns true if all characters in this string are letters.
### func ```replace``` (str1, str2)
Returns a new string where call occurances of [str1] have been replaced with [str2].
### func ```index``` (substring)
Returns the index of the first occurance of a string within this string. Raises KeyNotFound exception if the specified substring does not exist.
### func ```isalnum``` ()
Returns true if all characters in this string are letters or digits.
### func ```issymbol``` ()
Returns true if all characters in this string are symbols.
### func ```join``` (*args)
Joins all arguments together, returning a string where this string has been placed between all supplied arguments
### func ```startswith``` (value)
Returns true if the string starts with the specified value.
### func ```rindex``` (substring)
Returns the index of the last occurance of a string within this string. Raises KeyNotFound exception if the specified substring does not exist.
### func ```trim``` ()
Returns a string where all leading whitespace characters have been removed.
### func ```map``` (callable)
Iterates over the specified iterable, passing the result of each iteration to the specified callable. The result of the specified callable is added to a list that is returned to the caller.
### func ```endswith``` (value)
Returns true if the string ends with the specified value.
### func ```each``` (func)
Iterates through each element in the collection.
### func ```substr``` (start, [end])
Returns a substring contained within this string.@returns The substring between start and end
### func ```upper``` ()
Returns the uppercase representation of this string
### func ```rjust``` (n, [c])
Returns a string that has been justified by [n] characters to left.
### func ```contains``` (value)
Returns true if the string contains the specified value. 
### func ```lower``` ()
Returns the lowercase representation of this string
### func ```filter``` (callable)
Iterates over the specified iterable, passing the result of each iteration to the specified callable. If the callable returns true, the result is appended to a list that is returned to the caller.
### func ```rfind``` (substring)
Returns the index of the last occurance of a string within this string. Returns -1 if the specified substring does not exist.
### func ```isdigit``` ()
Returns true if all characters in this string are digits.
### func ```last``` (value)
Returns the last item in this collection.
### func ```iswhitespace``` ()
Returns true if all characters in this string are white space characters.
### func ```split``` (seperator)
Returns a list containing every substring between [seperator].
### func ```find``` (substring)
Returns the index of the first occurance of a string within this string. Returns -1 if the specified substring does not exist.
### func ```reduce``` (callable, default)
Reduces all members of the specified iterable by applying the specified callable to each item left to right. The callable passed to reduce receives two arguments, the first one being the result of the last call to it and the second one being the current item from the iterable.
### func ```ljust``` (n, [c])
Returns a string that has been justified by [n] characters to right.
### func ```first``` (value)
Returns the first item in this collection.
## class ```StringBuffer``` ()
___
A mutable string of UTF-16 characters
### func ```prepend``` (item)
Prepends text to the beginning of the string buffer.
### func ```clear``` ()
Clears the string buffer.
### func ```append``` (*args)
Appends each argument to the end of the string buffer.
## class ```Tuple``` ()
___
An immutable collection of objects
### func ```filter``` (callable)
Iterates over the specified iterable, passing the result of each iteration to the specified callable. If the callable returns true, the result is appended to a list that is returned to the caller.
### func ```first``` (value)
Returns the first item in this collection.
### func ```map``` (callable)
Iterates over the specified iterable, passing the result of each iteration to the specified callable. The result of the specified callable is added to a list that is returned to the caller.
### func ```last``` (value)
Returns the last item in this collection.
### func ```reduce``` (callable, default)
Reduces all members of the specified iterable by applying the specified callable to each item left to right. The callable passed to reduce receives two arguments, the first one being the result of the last call to it and the second one being the current item from the iterable.
### func ```each``` (func)
Iterates through each element in the collection.
## func ```chr``` (num)
Returns the character representation of a specified integer.
## func ```compile``` (source)
Compiles a string of iodine code, returning a callable object.
## func ```enumerate``` (iterable)
Maps an iterable object to a list, with each element in the list being a tuple containing an index and the object associated with that index in the supplied iterable object.
## func ```eval``` (source)
Evaluates a string of Iodine source code.
## func ```filter``` (iterable, callable)
Iterates over the specified iterable, passing the result of each iteration to the specified callable. If the callable returns true, the result is appended to a list that is returned to the caller.
## func ```globals``` ()
Returns a dictionary of all global variables.
## func ```hex``` (obj)
Returns hexadecimal representation of a specified object,supports both Bytes and Str objects.
## func ```id``` (obj)
Returns a unique identifier for the supplied argument. 
## func ```input``` (prompt)
Reads from the standard input stream. Optionally displays the specified prompt.
## func ```invoke``` (callable, dict)
Invokes the specified callable under a new Iodine context.Optionally uses the specified dict as the instance's global symbol table.
## func ```len``` (countable)
Returns the length of the specified object. If the object does not implement __len__, an AttributeNotFoundException is raised.
## func ```loadmodule``` (name)
Loads an iodine module.
## func ```locals``` ()
Returns a dictionary of all local variables.
## func ```map``` (iterable, callable)
Iterates over the specified iterable, passing the result of each iteration to the specified callable. The result of the specified callable is added to a list that is returned to the caller.
## func ```open``` (file, mode)
Opens up a file using the specified mode, returning a new stream object.<br><strong>Supported modes</strong><br><li> r - Read<li> w - Write<li> a - Append<li> b - Binary 
## func ```ord``` (char)
Returns the numeric representation of a character.
## func ```print``` (*object)
Prints a string to the standard output streamand appends a newline character.
## func ```property``` (getter, setter)
Returns a new Property object.
## func ```range``` (start, end, step)
Returns an iterable sequence containing [n] items, starting with 0 and incrementing by 1, until [n] is reached.
## func ```reduce``` (iterable, callable, default)
Reduces all members of the specified iterable by applying the specified callable to each item left to right. The callable passed to reduce receives two arguments, the first one being the result of the last call to it and the second one being the current item from the iterable.
## func ```reload``` (module)
Reloads an iodine module.
## func ```repr``` (object)
Returns a string representation of the specified object, which is obtained by calling its __repr__ function. If the object does not implement the __repr__ function, its default string representation is returned.
## func ```require``` ()
Internal function used by the 'use' statement, do not call this directly.
## func ```sort``` (iterable, [key])
Returns an sorted tuple created from an iterable sequence. An optional function can be provided that can be used to sort the iterable sequence.
## func ```sum``` (iterable, default)
Reduces the iterable by adding each item together, starting with [default].
## func ```type``` (object)
Returns the type definition of the specified object.
## func ```typecast``` (type, object)
Performs a sanity check, verifying that the specified [object] is an instance of [type]. If the test fails, a TypeCastException is raised.
## func ```zip``` (iterables)
Iterates over each iterable in [iterables], appending every item to a tuple, that is then appended to a list which is returned to the caller.
