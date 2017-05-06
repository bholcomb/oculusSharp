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
    
  configuration { "**.png" }
      buildaction "Embed"
	
  configuration {"x32"}
   defines{"BUILD32"}
   targetdir "../bin/x32"
   debugdir "../bin/x32"

  configuration {"x64"}
   defines{"BUILD64"}
   targetdir "../bin/x64"
   debugdir "../bin/x64"
   
project "libOVR"
	kind "SharedLib"
	language "C++"
	location "libOvr1.14.0"
	defines{"OVR_DLL_BUILD"}
	includedirs{"../libOVR1.14.0/Include"}
	files{"../libOvr1.14.0/Include/**.h", "../libOvr1.14.0/Src/**.c", "../libOvr1.14.0/Src/**.cpp", "../libOvr1.14.0/Src/**.h"}
 
 project "OculusSharp"
	kind "SharedLib"
	language "C#"
	location "OculusSharp"
	files{"../src/*.cs"}
	targetdir "../bin"
	links{"System", "OpenTK"}
   clr "Unsafe"
 
 project "TestOculus"
   kind "ConsoleApp"
   language "C#"
   location "testOculus"
   files{"../testOculus/*.cs", "../testOculus/*.png"}
   targetdir "../bin"
   links {"System", "System.Drawing", "OpenTK", "OculusSharp"}
