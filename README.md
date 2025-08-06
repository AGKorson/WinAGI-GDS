
# WinAGI-GDS

    WinAGI Game Development System
             ©2005 - 2025
           by Andrew Korson
    
    ==============================                                   
        Version 3.0.0alpha18.1
    ==============================


### About WinAGI:  

WinAGI is a full featured game development system, that includes editors which provide full control over of all resources in an AGI game. It includes several integrated tools to assist in game design and development, allowing you to create new AGI games more efficiently and with less effort. 

---
  
  
### Known Issues:  

 - When installing, don't use a sub-directory of Program Files. There are weird file access permission issues that may prevent the app from running.

---


### Progress Points:  

    X  1. Game and Form basic structure
    X  2. Game and resource functions
    X  3. Resource tree and preview window
    X  4. Game menu functions
    X  5. Resource menu functions
    X  6. Tools menu functions
    X  7. Logic Editor
    X  8. Status Bar
    X  9. Character Map
    X 10. Snippets Editor
    X 11. Global Defines Editor
    X 12. Reserved Defines Editor
    X 13. Object Editor
    X 14. Word Editor
    X 15. Palette Editor
    X 16. Picture Editor
    X 17. View Editor
    X 18. Sound Editor
    O 19. Layout Editor
      20. Menu Editor
      21. Text Screen Editor
      22. Help Integration
      23. BETA TESTING

---

  
### History:
**alpha18.1** Layout editor framework set. Layouts successfully load and display. Integration between logic editor and layout editor works as expected to keep the layout and logics up to date. Layout editing functions still need to be completed. Continuing to fix bugs in all other editors as they are found.

**alpha18.0** Sound editor is complete. Added support for drawing and editing staff notes directly from the staff display. Fixed some bugs in the sound editor and other editors.


**alpha17.1** Sound editor framework is set. Basic sound editing functions are working. Added support for importing MIDI, IT and MOD files directly, without needing intermediary conversion. Still need to add support for drawing and editing staff notes directly from the staff display.

**alpha17.0** View Editor is complete. 

**alpha16.1** View Editor framework is set. Drawing and scaling features complete. Editing support for non-graphical functions (adding/removing loops/cels, changing cel size, mirroring/unmirroring, etc) complete. 
Still need to add drawing function and image manipulation support.
Lots of refactoring of other editors and game objects.

**alpha16.0** Picture Editor is complete, including all editing and testing features. Refactored a bunch of code to streamline editors. Lots of bug fixes in previous editors and functions.

**alpha15.3** Drawing/edit features of picture editor are complete. Added support for scaling pictures non-integer values.
Reworked statusbar support in all editors to deal with WinForm's unworkable merging behavior.
Fixed some bugs in AGIResource object.

**alpha15.2** added picture background support to the editor.

**alpha15.1** picture editor framework is set, basic image drawing/scaling functions working. Still lot of work to do to get the editor working. 
The export as gif functions now work correctly and performance significantly improved (after fixing the LZW compression functions). 
Lots of other minor bug fixes across many other files.

**alpha15.0** Palette editor is complete.

**alpha14.0** WORDS.TOK editor search functions done, WORDS.TOK editor is now complete

**alpha13.3** all words.tok editing functions done, working on search/replace functions


**alpha13.2** basic word editor layout complete.  object editor now updates status bar correctly


**alpha13.1** fixed bug in wordlist that wasn't reading extended characters correctly; added statusbar updates to object editor; working on word editor form layout


**alpha13.0** OBJECT file editor is complete. Fixed errors in Find form, fixed right-click menu in globals and objects editors, added keyhandler to msgboxex class.


**alpha12.1** Working on OBJECT file editor. Basic editing functions working, still need to add find/replace functions.


**alpha12.0:** Reserved Defines editor is complete. 


**alpha11.3:** GlobalList and ReservedList objects now work similarly to other game objects- with load/save/add/clear methods; cleaned up loadgame/newgame functions and logic editors and compiler to use the new objects.


**alpha11.2:** Global Defines editor is complete. Removed the 'High' setting for logic compiler errors. A few other minor bug fixes.


**alpha11.1:** Converting WinAGIv2 game files now add includes when game is first opened. Fixed some bugs in the logic compiler, the ReplaceAll function and resID mod function.


**alpha11.0:** Snippets editor is complete. Beginning work on defines editor. Major change in handling of defines. What were previously treated as 'built-in' (for resource IDs), 'reserved' (variables, flags, constants reserved by AGI) and 'global' defines now use real include files that must be added to every file using #include directive. (In WinAGI VB no #include directives were needed, the defines were automatically added by the compiler.) With this change, all source files are self-sufficient and will compile in any AGI compiler (e.g. AGI Studio). WinAGI automatically updates the three include files, and will add/remove the #include directives to/from the source files automatically as well. Users can opt to disable the auto-include feature if they wish to maintain all defines manually.  


**alpha9.0:** Character Map dialog to allow insertion of extended characters is complete and integrated into logic editor. When upgrading version 2 wag files, logic source files are backed up before being converted.  
  
  
**alpha8.0:** First public release. All game-level functions (opening, importing, creating new from blank or template, managing setting and properties, rebuilding/compiling, etc) and basic resource management (adding/removing, renumbering, exporting, importing, previewing) are functional. The only working editor is the Logics editor.  
  
  
---

### Licensing

    Copyright (C) 2005 - 2025 Andrew Korson

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

