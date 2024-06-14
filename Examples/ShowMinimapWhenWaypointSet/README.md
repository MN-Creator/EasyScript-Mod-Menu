## Only show minimap when a waypoint is set while driving

Create two scripts, the first will show the minimap when the player is driving and
a waypoint is set. The second script will hide the minimap.

### Script 1 (Show minimap)

1. Add the action `Show Minimap` to the script, found in `Actions -> Misc`.
1. Set the conditions `In Any Vehicle` and `Waypoint Set` to `Yes`.
1. Enable the background runner in `Settings -> Run in Background`, set the	 
interval time to 1 second or use less if you want it to be more responsive. 
A lower time will be more demanding.


### Script 2 (Hide minimap)

1. Add the action `Hide Minimap` to the script.
1. Set the condition `In Any Vehicle` to `No`.
1. Enable the background runner in `Settings -> Run in Background`.