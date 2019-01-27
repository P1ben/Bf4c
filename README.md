# Bf4c
A brainfuck interpreter with memory management tools.

It works the same way as classic brainfuck, but it has some extra commands:
```
  ":" Saves a cell value as an argument.
  "(" Reads the memory address that was given with ":", and saves to the cell the pointer is at.
  ")" Writes the value under the pointer to the memory address that was given with ":".
  "@" Checks if the keycode given by ":" is pressed, turns the value under the pointer to 0 or 1.
  "*" Adds the module baseaddress to the cell under the pointer.
  ";" Int32 input.
```  
On the start of the file, you must write the process and module you want to work with:
```
  process:module;
  ie.: csgo:client_panorama.dll;
```
