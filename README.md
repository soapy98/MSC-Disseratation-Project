# MSC-Disseratation-Project

The projects aim was to create VR tanks game with networking, raytracing and procedural generation.

The project was created in Unity with the use of the Oculus package as a Oculus Quest was used. Each feature stated was implemented to some extent with a simple game but interacting features. 

## Networking
The Unity package of MLAPI was used to enable multiplayer with it integrated into the game to support multiple users. 

## Procedural Generation
To satisfy this the maze used in the main level was procedurally generated alongside 2 spheres. For the map the use of Cellular automata to generate the intial map outline with the marching squares algorithm then used to polyganise and generate the maze as a 3D object. 
![image](https://user-images.githubusercontent.com/53182878/162799414-a2c9057d-ba8c-4819-827b-4fd6f079184d.png)

One of the spheres created was done through the manipulation of a cube to sphere through the use of a the tesselation stage of the GPU and application of a sphereical parametric equation applied onto the cube with the use a Phong shader.

![image](https://user-images.githubusercontent.com/53182878/162799550-8fd6ce23-8a97-49dd-b417-3f3d6bab4c8d.png)

The second sphere was procedurally generated through the use of a raymarhed algorithm in which a implicit raymarched sphere was generated and contained in a Sphere without a mesh. With a gradient estimation applied to enable Phong lighting.

![image](https://user-images.githubusercontent.com/53182878/162799723-27c770c9-522d-4ab7-9b4e-41cc945af8a9.png)


## Raytracing
A parallax shader was utilised which deployed a ray tracing algorithm to produce a bumpy effect. A further use was the creation of a raymarched sphere contained in a Sphere without a mesh.
![image](https://user-images.githubusercontent.com/53182878/162799498-629c9801-a2f1-403d-9e82-d279d31e5b61.png)
