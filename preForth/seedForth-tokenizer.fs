\ seedForth tokenizer (byte-tokenized source code)

\ load on on top of gforth   uho  2018-04-13

\ -----------------------------

WARNINGS OFF

VARIABLE OUT

: PROGRAM ( <name> -- )
   BL WORD COUNT R/W CREATE-FILE THROW OUT ! ;

: SUBMIT ( c -- )
   PAD C!  PAD 1 OUT @ WRITE-FILE THROW ;

: END ( -- )
   .S CR 0 SUBMIT OUT @ CLOSE-FILE THROW BYE ;

Variable #FUNS

: FUNCTIONS ( u -- )   #FUNS ! ;


: #FUN: ( <name> n -- )
    CREATE dup , 1+ FUNCTIONS DOES> @ SUBMIT ;
    
: FUN: ( <name> -- )
    #FUNS @ #FUN: ;

$02 #FUN: key
$0A #FUN: -
$29 #FUN: couple

: byte# ( c -- )
    ( seedForth ) key    
    SUBMIT ;

: # ( x -- )      \ x is placed in the token file. Handle also negative and large numbers
     dup 0<    IF  0 byte#   negate recurse  ( seedForth ) -  EXIT THEN
     dup $FF > IF  dup 8 rshift  recurse  $FF and  byte#  ( seedForth ) couple EXIT THEN
     byte# ;    

$22 #FUN: compiler

: [ ( -- )  0 SUBMIT ;
: ] ( -- )  compiler ;

\ Literal numbers

$0C #FUN: lit
$1E #FUN: ,
$1F #FUN: c,

: #, ( x -- ) lit [ # , ] ;    \ x is placed in memory as a cell-sized quantity (32/64 bit), as defined by comma

\ Strings

$32 #FUN: $lit

: ", ( c-addr u -- )
    dup # ( seedForth) c,  BEGIN dup WHILE >r dup char+ swap c@ # ( seedForth) c,  r> 1- REPEAT 2drop ;

: ," ( ccc" -- )   [char] " parse ", ;

: $, ( c-addr u -- )  $lit [ ", ] ;

: s" ( ccc" -- )   [char] " parse $, ;  \ only in compile mode


  $00 #FUN: bye       $01 #FUN: emit        ( $02 #FUN: key )      $03 #FUN: dup
  $04 #FUN: swap      $05 #FUN: drop          $06 #FUN: 0<         $07 #FUN: ?exit
  $08 #FUN: >r        $09 #FUN: r>         (  $0A #FUN: - )        $0B #FUN: unnest
( $0C #FUN: lit )      $0D #FUN: @            $0E #FUN: c@         $0F #FUN: !
  $10 #FUN: c!        $11 #FUN: execute       $12 #FUN: branch     $13 #FUN: ?branch
  $14 #FUN: negate    $15 #FUN: +             $16 #FUN: 0=         $17 #FUN: ?dup
  $18 #FUN: cells     $19 #FUN: +!            $1A #FUN: h@         $1B #FUN: h,
  $1C #FUN: here      $1D #FUN: allot       ( $1E #FUN: , )      ( $1F #FUN: c, )
  $20 #FUN: fun       $21 #FUN: interpreter ( $22 #FUN: compiler ) $23 #FUN: create
  $24 #FUN: does>     $25 #FUN: cold          $26 #FUN: depth      $27 #FUN: compile,
  $28 #FUN: new     ( $29 #FUN: couple  )     $2A #FUN: and        $2B #FUN: or
  $2C #FUN: catch     $2D #FUN: throw         $2E #FUN: sp@        $2F #FUN: sp!
  $30 #FUN: rp@       $31 #FUN: rp!         ( $32 #FUN: $lit )

$33 FUNCTIONS

\ Definitions
  
: ': ( <name> -- ) FUN: fun ;
: ;' ( -- ) unnest [ ;


\ Control structure macros

: AHEAD ( -- addr ) branch [ here  0 # , ] ;
: IF ( -- addr )   ?branch [ here  0 # , ] ;
: THEN ( addr -- ) [ here swap ! ] ;
: ELSE ( addr1 -- addr2 )  branch  [ here 0 # ,  swap ] THEN ;

: BEGIN ( -- addr )  [ here ] ;
: AGAIN ( addr -- )   branch [ , ] ;
: UNTIL ( addr -- )  ?branch [ , ] ;
: WHILE ( addr1 -- addr2 addr1 )  IF [ swap ] ;
: REPEAT ( addr -- ) AGAIN THEN ;

