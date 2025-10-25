# A .NET implementation of a DAWG and GADDAG dictionary. 
## Ultra compact on disk and in memory.
## Ultra fast tree navigation. 

The core implementations includes : 
- Dawg and GadDag Compiler
- Dawg and GadDag Word Search : 
    - Word search
    - Prefix search
    - Suffix search 
    - Wildcard search (seemless multiple ? or *)
    - Anagram search 
    - Anagram & smaller search.
- A scrabble engine that can create games 
    - includes a boosted version which analyses solutions to get a level of difficulty.

The current library uses extensively interfaces as models and services, most of them are not implemented in this library. To build an application using this library, you need first to implement the missing objects which will ne most likely dependant of your data layers. 

This code is totally written by my own, and it is free for strictly personal use.
There is no free support on any piece of code. 
Any commercial usage is prohibited without any mutual agreement 

To experiment a full functional application you can try out Skrabby with a small contribution of 5€ by month :
https://www.facebook.com/profile.php?id=61582287357805

For any information and credits : crolow.fastdico at outlook.com
And if you want to support me, donations are welcome. 


[![](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=GXN5ACMFKDSF6)
