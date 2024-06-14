## Only show the minimap when driving

Can be implemented using events, create two scripts.

#### Script 1 (Show minimap)
The first script will show the minimap when the player enters a vehicle.

1. Add the action `Show Minimap` to the script. Can be found in the `Actions -> Misc` or 
enter "Show Minimap" in the search bar at the top in `Actions`.
2. When the action has been added to the script, enable the event `Entered Vehicle` in 
`Settings -> Events`. Now the first script is done.

#### Script 2 (Hide minimap)
For the second script, you will hide the minimap when the player exits the vehicle.

1. Add the action `Hide Minimap` to the script.
2. When the action has been added to the script, enable the event `Left Vehicle` in 
`Settings -> Events`.

Now enter a vehicle for the scripts to start working.
