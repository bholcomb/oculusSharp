solution "OculusSharp"
   location("../")
   configurations { "Debug", "Release" }
   platforms{"x32", "x64"}
   startproject "libOVR"
   
  configuration { "Debug" }
    defines { "DEBUG", "TRACE"}
    symbols "on"
    optimize "Off"
 
  configuration { "Release" }
    optimize "Speed"
	
  configuration {"x32"}
   targetdir "../bin/x32"
   debugdir "../bin/x32"

  configuration {"x64"}
   targetdir "../bin/x64"
   debugdir "../bin/x64"
   
project "libOVR"
	kind "SharedLib"
	language "C++"
	location "libOvr1.13.0"
	defines{"OVR_DLL_BUILD"}
	includedirs{"../libOVR1.13.0/Include"}
	files{"../libOvr1.13.0/Include/**.h", "../libOvr1.13.0/Src/**.c", "../libOvr1.13.0/Src/**.cpp", "../libOvr1.13.0/Src/**.h"}
 
 project "OculusSharp"
	kind "SharedLib"
	language "C#"
	location "OculusSharp"
	files{"../src/*.cs"}
	targetdir "../bin"
	links{"System", "OpenTK"}
 
 project "TestOculus"
   kind "ConsoleApp"
   language "C#"
   location "testOculus"
   files{"../testOculus/*.cs"}
   targetdir "../bin"
   links {"System", "OpenTK", "OculusSharp"}