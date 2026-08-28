# WinAGI-GDS

    WinAGI Game Development System
             ©2005 - 2026
           by Andrew Korson
    
    ==============================                                   
        Version 3.0.0beta.63
    ==============================


### About WinAGI:  

WinAGI is a full featured game development system, that includes editors which provide full control over of all resources in an AGI game. It includes several integrated tools to assist in game design and development, allowing you to create new AGI games more efficiently and with less effort. 

WinAGI GDS v3 represents a completely rewritten/refreshed codebase (based on the original VB6 version) with an updated project structure and a clean repository history. All capabilities of the last VB release (v2.3.7) are fully supported, and many new enhancements and additions are included.

Game projects created in v2 can be opened in this version, and the WAG file and source files will be automatically updated. Note that game projects in v3 are not backward compatible and cannot be opened in v2.

The Help file included with this release contains detailed information on all the features and tools available.

---

### Notes:

This is a beta release. Feedback, bug reports, and feature requests are welcome.

---

  
### Known Issues:  
---
  
### History:
Beta.63:
- removed unneeded debug statements; no functional changes

Beta.62:
- fixed logic editor syntax highlighting to handle tokens with numbers consistently

Beta.61:
- more minor code cleanup and refactoring; no functional changes

Beta.60:
- fixed fan compiler to correctly identify defines that override a resource ID

Beta.59:
- fixed define name checker to correctly identify names in form of 'v.##' as OK, and not as an argument marker

Beta.58:
- adds additional error information to failed resource load exceptions

Beta.57:
- right-clicking the infogrid now selects the row under cursor in all situations

Beta.56:
- cumulative update that includes Beta.40 through Beta.56
- in picture editor, selecting a command now forces tool to change to Edit in all cases
- fixed bug in reserved name editor that crashed when trying to edit when no game is loaded
- changed format of menu editor name to be more consistent
- updated new game from template behavior to not allow changing syntax or code page
- importing game from Sierra source files now correctly adds all required include files, and adjusts gameid by looking for set.game.id commands in the imported source code
- changed import method to disallow if the imported directory already has a WAG file
- fixed refactor in Beta.49 to correctly read loader files to find a target gameid
- refactored the guessed GameID code in game import function the bug that was truncating the guessed GameID when importing to Sierra syntax
- fixed bug in loadloops that didn't check for parent view before mirror loop detection
- fixed logic decoder to update the grid when decompile warnings are detected
- fixed decompiler to correctly insert group 0, 1, 9999 placeholder words, also added decompiler warning if group 0 is found in a said command
- fixed game import and new game functions to correctly use the 'use layout editor' option, and cleaned up the import parameter form
- fixed bug in layout form that causes error when minimizing the main form when the layout form is maximized
- Fixed bug in game-import so it now correctly identifies existing WAG files in the import directory
- Compile logic option on resource menu is now correctly enabled when a changed logic is currently selected in the resource list
- fixed bug in Dismiss Warning functions to display the correct context menu items and to properly handle warnings that are in include files

Beta.39:
- cumulative update that includes Beta.26 though Beta.39
- fixed command counts for several versions that were not correct
- fixed bug in the error msg for missing include files, which was not showing up on first click
- when importing a game using Sierra syntax, all include files are now correctly added to the game's include file list
- errors and warnings in Include files are now correctly updated in the infogrid when compiling logics
- when decoding sierra logics, if block nesting limit is 10 not 9
- refactored preview view cycling start/stop functions
- changed key handling in pic editor command list to refresh screen every time the selection changes instead of only when key is released
- fixed bug introduced in Beta.5 that made pic edit display mode fail to show priority screen when Both was selected
- fixed form closing handlers so out-of-game resources will trigger an 'ask-to-save' query
- fixed bug that was not properly renaming wag file when importing a game in Sierra syntax
- refactored Include file handlers; fixes errors when adding/removing to/from sierra syntax; improves sorting, adds relative path to properties window
- added a new dithering tool to the picture editor
- refactored pen updates to more intuitively know when to add a new pen change command and when to edit an existing or prior command; fixed dither to only add the start coordinates if a dither line is just a single pixel

Beta.25:
- cumulative update that includes Beta.5 through Beta.25
- a lot of code cleanup, commenting, and refactoring
- add check to local defines to menu editor ExtractMenu function
- fix bug in plot pen style update function that corrupted picture data in some cases
- fixed export picture as gif to work correctly for preview pictures
- updated logic editor save function to update text editor by line instead of replacing the entire editor text
- updated property form to enforce gameid size limits (6 for v2, 5 for v3)
- restored property grid functionality (it was broken by Beta.5 release)
- removed unnecessary check for indirection in fan compiler
- changed picture resource properties StepDraw and DrawPos to read/write regardless of ingame state
- refactored LZW decompression for v3 resources; new routine improves performance
- refactored gif export functions, which significantly improved speed
- Refactored midi and pcm functions to improve performance
- cleaned up custom exception numbering, no functional changes
- fixed view description property to handle errors gracefully
- logic decoder was using wrong version of #include when decoding in Fan syntax
- custom Tools function was incorrectly parsing the '%PROGDIR%' tag, resulting in invalid filename pointers
- fixed NewGame method to correctly save the new game's OBJECT file
- word group 9999 ('rol') is not supported in version 2.089; added new check/warning in compilers, updated Help files
- error data for invalid cel data had loop and cel values backwards
- when error encountered in logic resource (such as an invalid DIR pointer, or missing VOL number), resetting the logic to default was also marking it as compiled; it should continue to be non-compiled until the DIR/VOL issue is fixed by actually compiling
- SaveSettings had minor bug when saving ignored warnings; uncompiled logics need CompiledCRC set to 0xffffffff, not 0xffff
- IncludeFile settings were not properly updated when creating or importing a new WAG file
- add support for version 2.230, fix bug in GetVersion function that always returned the default version value instead of actual version found; some help file corrections/clarifications
- Finished adding v2.230 support; updated view editor to automatically convert v2.230 mirror loops to normal loops; also cleaned up resource warning handlers, fixing some bugs in numbering; updated help file to align with changes

Beta.4:
- fixes bug in newgame when using v3 template;
- fixes bug when toggling IncludeReservedText property
- includelist now properly updated when include options are changed
- fixed syntax highlighting to correctly highlight symbols such as '@=' and '=@'
Beta.3:
- Style and formatting updates; no change to code
Beta.2:
- Fixed bugs caused by incorrectly constructing filenames (affected NewGame, OpenGame, and several other methods)
Beta.1:
- Fixed compiler setting so app builds as x86 instead of ARM so it now runs on all Windows systems

5/30/2026: Initial v3.0.0 BETA release

---

### Licensing

    Copyright (C) 2005 - 2026 Andrew Korson

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 

