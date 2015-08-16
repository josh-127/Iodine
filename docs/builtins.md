# Iodine Builtin Functions

##### func ```print``` (object)
Prints the string representation of an object
##### func ```input``` ([prompt])
Reads from the standard input stream, optionally displaying prompt
##### func ```eval``` (source)
Evaluates a string of iodine source code
##### func ```filter``` (iterable, function)
Iterates through an iterable object, passing each iteration to function. If function returns true, then the element is added to a list that is returned to the caller.
##### func ```map``` (iterable, function)
Iterates through an iterable object, performing function on each iteration. The outputs from function is added to a new list that is returned to the caller.
##### func ```range``` (n)
##### func ```range``` (start, end)
Returns an iterable object with n iterations 
##### func ```open``` (file, mode)
Opens up a file, returning a new stream object.
# Iodine Builtin Classes
##### class ```Int``` (object)
Class represents a 64 bit signed integer. 
##### class ```Char``` (object)
Class represents a UTF-16 char
##### class ```Bool``` (object)
Class represents a boolean
##### class ```Str``` (object)
Class represents a string
##### class ```HashMap``` (object)
Class represents a HashMap (Dictionary)
##### class ```List``` (object)
Class represents a variable length list
##### class ```Tuple``` (object)
Class represents a variable tuple
##### class ```Event``` ()
Class represents an event
##### class ```Stream``` ()
Class represents a stream
