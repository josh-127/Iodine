if exists("b:current_syntax")
  finish
endif

filetype plugin indent on
" show existing tab with 4 spaces width
set tabstop=4
" when indenting with '>', use 4 spaces width
set shiftwidth=4
" On pressing tab, insert 4 spaces
set expandtab

syn match iodineComment "#.*$"
syn match iodineEscape	contained +\\["\\'0abfnrtvx]+

syn match iodineNumber '\d\+'  
syn match iodineNumber '[-+]\d\+' 

syn match iodineNumber '\d\+\.\d*' 
syn match iodineNumber '[-+]\d\+\.\d*'

syn region iodineString start='"' end='"' contains=iodineEscape
syn region iodineString start="'" end="'" contains=iodineEscape
syn keyword iodineKeyword if else for func class while do break lambda self use return true false null foreach from in as is isnot try except raise with super interface enum yield given when default match case var

syn keyword iodineFunctions print println input map filter

syn region iodineBlock start="{" end="}" fold transparent contains=ALL


let b:current_syntax = "iodine"

hi def link iodineComment     Comment
hi def link iodineKeyword     Statement
hi def link iodineFunctions   Function
hi def link iodineString      Constant
hi def link iodineNumber      Constant
