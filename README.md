# oculusSharp #
Lightweight C# bindings to the OculusSDK 1.16.0.  Based heavily on the original work of OculusWrap (https://oculuswrap.codeplex.com/). 

## To build ##
Open a command prompt in the build directory
Run premake5.exe vs2015 (or your current version of visual studio)
Open solution  and build

## Test Oculus ##
A bare bones application that renders a cube in VR and blits the mirror texture to the screen.  ESC to exit.  Spacebar will recenter the tracking origin.   Shows usage of OpenTK/OpenGL/Oculus SDK without a lot of helper classes/abstractions so you can see how the API is used.

## Other Notes ##
* Oculus API coverage isn't fully tested or complete.  I'm sure there's bugs.  
* OculusSharp Depends on OpenTK for Vector, Quaternion and Matrix classes. 
