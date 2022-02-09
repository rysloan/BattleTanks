Ryan Sloan & Kashish Singh

PS9 SEIZURE WARNING!
If you straddle the world border, the tank wrapping around to the other side can go between the two locations rapidly and
potentially create an effect that could cause a siezure? You can stop it by moving a lot in a direction.

---Game Basics---

Player Controls
-WASD keys for movement (up, left, down, right) respectively
-Left Click on mouse shoots projectiles
-Right Click on mouse shoots a beam that can instantly kill a player

Player UI
-Player name is displayed underneath the tank
-Score is displayed next to the player name and represents how many points/kills the player has
-Health Bar is displayed above the tank and decreases as health goes down

Quality/Additional Features
-Player Names, Scores, Projectiles, and Beams are all the same color as the player's tank
-HP bar changes color depending on what stage it's in (Green to Yellow to Red)
-Tank's beam starts out as a huge beam then shrinks down to nothing
-Tank shrinks until not visable on tank death

---Coding---

-MVC-
Model
-For our model we created a project called WorldModel that has seperate classes for all the different
model components for the game
	-Each of the model components has fields based off of the JSON information and anything else as needed
-Vector2D is a file that we are supposed to add that we use to adjust values and help with positioning and such
as well as using it to calculate angles

View
-The view has mainly the Form1.cs file that is used to deal with button inputs, recieve and send event updates
to the controller, and add the visuals and call for frame updats
	-DrawingPanel.cs is a file that contains all the drawing methods and onPaint for actually creating the images
and animations for the various frames that's drawn into the viewing window

Controller
-We have GameController that helps controller all the various functions like recieving and sending things
from the server as well as what data to save and deserializing JSON text
-NetworkController was a project where we stored our NetworkController and SocketState class files,
HOWEVER, we ran into an issue where our tank would stop moving after a minute or so in large scale matches
like with 25 AI players so instead we used the provided NetworkController.dll from FullChatSystem as a reference
-Controller Commands helps deal with the different controls for the game such as moving, direction, and where the tank
turret is aiming

-Resources-
-The resources project is used to just hold whatever files we need for our game. Including this README which is located in
the resource project
	-Images file is used to hold all the images that is used to draw the various components of the game
	-Libraries is used to hold any dll files we need. Currently the only one there is the Teacher provided 
	NetworkController.dll since ours was having that issue that stopped tanks from moving after a while

-Additonal Minor Notes-
-The Beam part has an added component called "frames" that is used to keep track of how
many frames the beam is live for. This is not JSON information but instad something we use to help with
animating
-All files try to have the "TankWars" namespace since it's all used for the same final product. Only thing that doesn't
is the NetworkController which has its own Namespace from PS7
-We don't have an animation on player disconnect

---------------------------------------------------------------------------------------------------------------------------------------------------
-----PS9 README Section-----

-Added Projects & Files-
-PS9 is all about creating a server that your client can run with. To do this we looked a lot at the PS9 help section and 
expanded things from there. New additions include:

	-A server project that holds the server class and ServerController class
		-Server.cs contains the main method that takes the settings.xml file and sends it to the Settings.cs class to be processed.
		 Then it sends the setting information to the ServerController and starts the server and then uses a Console.Read() statement
		 to keep the console open.
		 -ServerController.cs Does the networking side of things such as connecting to the client and recieving and sending data
		  also has an update method that appends the string(s) for the different model parts (tank, projectile, beam, etc) and 
		  eventually sends the data. It also takes the recieved user inputs from the client and process them so things like the 
		  tank can move and shoot

	-In the Resources Project a new file has been added called settings.xml which contains setttings information for the game
		IMPORTAINT NOTE: If the settings file is deleted OR the format of it is changed it causes the server to not run. 
		So be careful when adding and removing walls to make sure the format of it is the same as it was initially 
		(sometimes when copy-pasting, the format is changed where it's more organized into lines, to fix this you can
		 press ctrl+z and that can fix the formatting) Also NOTE: The format is based off of the given server.xml file given
		 by the teacher's TankWars files.

	-In the WorldModel project the Settings.cs class has been added that has a file reader that reads the file and sets certain 
	 setting parameters accordingly to be used by the server.

-The Server-
-The server's main function of processing data and collisions before sending them to the client has been done in different parts
	-Collisions: In the various model such as Tank, Beam, Projectile, etc. There are boolean methods that check if a collision has occured.
				 In the World.cs, it has methods that use the collision bool statements and adjust values from there so that the 
				 collisions actually do something such as stop a tank from moving if it hits the wall
	-Sending and Recieving: as mentioned before, this is done in the Server.cs class which uses appending strings and the information saved
	 in the WorldMoodel project classes to send the appropriate information to the client
	
-Additional Information-
	-World Wrap around is done when the tank hits the edge of the world. It is sent to the opposite side of the world.
	-To track frame we start a count and then for every fram passed based on the settings information for frame speed adds to the count
	 until it reaches whatever we need (like a Tank staying dead for 5 seconds is 300 frames for the default settings)
	-Random spawning for Tanks and Powerups is done by checking if there is a wall in the generated new location. If so a new location is
	 randomized again and checked. If not then the tank or powerup will spawn there. (NOTE: Tanks can spawn on top of one another). 
	 Initial starting location when connecting for the first time is also randomized

-Glitches-
	-If you have a world with no wall borders, you can go into a corner and leave the map when you're quickly shifting between the left and
	 right world boundrieds. If this is done then the tank is locked into place and stuck there meaning the user will have to restart the 
	 client or be stuck there permenently

-Additional Features-
	-This has to be directly changed in the code and not the settings file but, if you go to Settings.cs and change the tankHP value, it will
	 work in the game and the health bar will also be extended to match (but won't change color from green until you're down to just 2 health)











