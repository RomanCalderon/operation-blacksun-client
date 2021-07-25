As you can see in the main folder we have multiple _URP & _HDRP .unitypackages.

URP_OLD package was setup and tested on Unity 2019.3.0 using URP 7.1.1

URP_7 package was setup and tested on Unity 2019.4.10 using URP 7.5.1

URP_10 package was setup and tested on Unity 2020.3.5 using URP 10.4.0

URP_11 package was setup and tested on Unity 2021.1.6 using URP 11.0.0

HDRP package was setup and tested on Unity 2019.3.0 using HDRP 7.1.1

So when you load the package into an SRP everything will be pink and broken. What you need to do is double click on the SRP type and version closest to what you are using. So if you are using HDRP 10.3.2 you need to double click the HDRP_10 package and unpack it in your project. It should overwrite everything it needs and the demo scenes and assets should be working like they should. 

For the demo water shaders in URP you need to enable Depth Texture & Opaque Texture in the Universal Pipline Renderer you are using in your project.

This is very new to me and there does not seem to be any ideal solution for converting environments to an SRP. So there will show errors because of broken shaders and what it thinks are missing prefabs. Also some prefabs might break and need some manual resetting of materials.

Will have a video up on the Asset Store Page and Thread to go over this process. If you have any questions though please email me at baldinoboy@gmail.com

