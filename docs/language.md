# Iodine programming language (INCOMPLETE)

### 1. Lexical units
The first phase of running an iodine program is lexical analysis which brakes the program up into tokens. A token is the smallest unit iodine source code.  Iodine tokens all follow into the following categories

#### Token classes
- [Identifier](#Identifier)
- [String Literal](#StringLit)
- [Int literal](#Intlit)
- [Float literal](#FloatLit)
- [Keywords](#Keywords)
- [Punctuators](#Punctuators)
- [Operators](#Operators)

#### 1.1 Identifier <a id="Identifier"></a>
In iodine an identifier is string of alphanumeric characters used to identify a structure in code
```bnf
<identifier> ::= <ident-char> { <ident-char> | <digit> }
<ident-char> ::= <letter> | "_"
<letter>     ::= "a" ... "z" | "A" ... "Z"
<digit>      ::= "0" ... "9"
```
#### 1.2 String Literal <a id="Stringlit"></a>
A string is surrounded with quotation marks 
``` bnf
<string-literal> ::= "\"" {<ascii-char> | <escape-seq>} "\""
<ascii-char>     ::= "\x00" ... "\xFF"
<escape-seq>     ::= "\\" <escape>
<escape>         ::= "n" | "t" | "r" | "b"
```

#### 1.3 Int literal <a id="IntLit"></a>
An int literal consists of a numeric non decimal value
```bnf
<int-literal> ::= <digit> | {<digit>}
<digit>      ::= "0" ... "9"
```
#### 1.4 Float literal <a id="FloatLit"></a>
A float literal consists of a numeric decimal value
```bnf
<float-literal> ::= <int-literal> "." <int-literal>
``` 

#### 1.5 Keywords <a id="Keywords"></a>
Keywords are reserved words that can not be used as identifiers. The following are iodine keywords
```
if else for func class use self foreach in true false null lambda try except break continue params super return from
```

#### 1.6 Punctuators <a id="Punctuators"></a>
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

#### 2.1 Expressions
An iodine expression is a sequence of operators, operands and constants.
##### 2.1.1 Operator Precedence 
| Precedence   |      Operator      |  Associativity |
|----------|:-------------|------:|
| 0 |()<br>[]<br>.        | Left to right|
| 1 |!<br>-<br>~          | Right to left|
| 2 |!<br>-<br>~          | Right to left|
| 3 |*<br>/<br>%          | Left to right|
| 5 |+<br>-               | Left to right|
| 6 |<<<br>>>             | Left to right|
| 7 |<<br>><br><=<br>>=<br>is | Left to right|
| 8 |==<br>!=             | Left to right|
| 9 |&                    | Left to right|
| 10 |^                   | Left to right|
| 11 |&#124;              | Left to right|
| 12 |&&                  | Left to right|
| 13 |&#124;&#124;        |Left to right|
| 14 |=<br>+=<br>-=<br>*=<br>/=<br>%=<br><<=<br>>>=<br>&#124;=<br>&=<br>^=   |Left to right|

#### 2.2 Statements

#### 2.2.1 Class definitions
```bnf
 class_def ::= "class" <ident> "{" { <statement> } "}"
```
#### 2.2.2 Function definitions
```bnf
 func_def ::= "func" <ident> ( "()" | "(" <param-list> ")" ) "{" { <statement> } "}"
 <param-list> ::= <ident> | <ident> "," <param-list>
```
