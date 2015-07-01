# Iodine programming language (INCOMPLETE)

### 1. Lexical units
The first phase of running an iodine program is lexical analysis which brakes the program up into tokens. A token is the smallest unit iodine source code.  Iodine tokens all follow into the following categories

#### Token classes
- [Identifier](#printf)
- [String Literal](#printr)
- [Int literal](#sprintf)
- [Float literal](#sprintf_t)
- [Keywords](#format)
- [Punctuators](#len)
- [Operators](#len)

#### 1.1 Identifier
In iodine an identifier is string of alphanumeric characters use to identify a structure in code
```bnf
<identifier> ::= <ident-char> { <ident-char> | <digit> }
<ident-char> ::= <letter> | "_"
<letter>     ::= "a" ... "z" | "A" ... "Z"
<digit>      ::= "0" ... "9"
```
#### 1.2 String Literal
A string is surrounded with quotation marks 
``` bnf
<string-literal> ::= "\"" {<ascii-char> | <escape-seq>} "\""
<ascii-char>     ::= "\x00" ... "\xFF"
<escape-seq>     ::= "\\" <escape>
<escape>         ::= "n" | "t" | "r" | "b"
```

#### 1.3 Int literal
An int literal consists of a numeric non decimal value
```bnf
<int-literal> ::= <digit> | {<digit>}
<digit>      ::= "0" ... "9"
```
#### 1.4 Float literal
A float literal consists of a numeric decimal value
```bnf
<float-literal> ::= <int-literal> "." <int-literal>
``` 

#### 1.5 Keywords
Keywords are reserved words that can not be used as identifiers. The following are iodine keywords
```
if else for func class use self foreach in true false null lambda try except break continue params super return from
```

#### 1.6 Punctuators
Punctuators are special characters that are used for separation, or grouping. The following are valid iodine punctuators

```
{ } [ ] ( ) , . ; :
```
#### 1.7 Operators
Operators are used in expressions and are either unary or binary. The following are operators in iodine
```
== != <= >= => = && || << >> += -= *= /= &= ^= |= <<= >>= + - / * % & ^ | 
```
### 2. Semantic Units

#### 2.1 Expression
An iodine expression is defined as
```bnf
<call> ::= <term> | 
<term> ::= <identifier> | <string-literal> | <int-literal> | <float-literal> | "(" <expression> ")"
```
