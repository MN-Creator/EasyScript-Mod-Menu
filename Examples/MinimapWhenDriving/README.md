## Only show the minimap when driving

Can be implemented using events or conditions, this example uses events.

#### Script 1 (Show minimap)
The first script will show the minimap when the player enters a vehicle.

Add the action `Show Minimap` to the script. Can be found in `Actions -> Misc` or 
enter "Show Minimap" at the top in `Actions`.

![Show-Minimap4-White](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/ff2236f4-7a2a-4eb1-9ab3-3d43b724ec55)

Enable the event `Entered Vehicle` in 
`Settings -> Events`.

![Events-EnteredVehicle (Custom)](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/636e309a-0103-41e9-beb3-1c31cf87d82d)



#### Script 2 (Hide minimap)
For the second script, you will hide the minimap when the player exits the vehicle.

Add the action `Hide Minimap` to the script.

![Hide-Minimap](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/1b897dcd-2ca4-4d7a-a8f1-c1dfab11b1ea)

Enable the event `Left Vehicle` in `Settings -> Events`.

![Events-LeftVehicle (Custom)](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/fa784051-3336-498a-90d7-f0aaa1213bfe)


Now enter a vehicle for the scripts to start working.
