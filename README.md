# MSC-Disseratation-Project
The projects aim was to create VR tanks game with networking, raytracing and procedural generation. The project was created in Unity with the use of the Oculus package as a Oculus Quest was used. Each feature stated was implemented to some extent with a simple game but interacting features.
Networking: The Unity package of MLAPI was used to enable multiplayer with it integrated into the game to support multiple users.
Procedural Generation:To satisfy this the maze used in the main level was procedurally generated alongside 2 spheres. For the map the use of Cellular automata to generate the intial map outline with the marching squares algorithm then used to polyganise and generate the maze as a 3D object.
One of the spheres created was done through the manipulation of a cube to sphere through the use of a the tesselation stage of the GPU and application of a sphereical coordinates applied onto the cube.
The second sphere was procedurally generated through the use of a raymarhed algorithm in which a raymarched sphere was generated and contained in a Sphere without a mesh.
Raytracing: A parallax shader was utilised which deployed a ray tracing algorithm to produce a bumpy effect. A further use was the creation of a raymarched sphere contained in a Sphere without a mesh.
