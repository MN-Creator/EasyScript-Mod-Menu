## Only show minimap when a waypoint is set while driving

Create two scripts, the first will show the minimap when the player is driving and
a waypoint is set. The second script will hide the minimap.

### Script 1 (Show minimap)

Add the action `Show Minimap` to the script, found in `Actions -> Misc`.

![Show-Minimap4-White](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/4145ca96-7fc5-40c3-a6c9-0957b2c00b9b)

Set the conditions `In Any Vehicle` and `Waypoint Set` to `Yes`.

![Conditions-Waypoint](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/60a2f643-0f1a-45e1-950b-ad8bbbbd9486)

Enable the background runner in `Settings -> Run in Background`, set the	 
interval time to 1 second or use a lower time if you want it to be more responsive. 
The background runner will run the script every second if the conditions are true.
A lower time will be more demanding.

![RunInBackground](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/6a770c55-4920-4800-931b-52015f1cd947)

### Script 2 (Hide minimap)

Add the action `Hide Minimap` to the script.

![Hide-Minimap](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/dcd294d3-8c48-41ba-9f3f-55150886a12a)

Set the condition `In Any Vehicle` to `No`.

![Conditions-NotInVehicle](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/03971173-e9d0-4b8d-b29e-2c75b06f446b)


Enable the background runner in `Settings -> Run in Background`.

![RunInBackground](https://github.com/MN-Creator/EasyScript-Mod-Menu/assets/68109830/6a770c55-4920-4800-931b-52015f1cd947)
