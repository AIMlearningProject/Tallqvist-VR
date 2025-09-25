Instructions for Using the Tools in the Project
============

This file provides guidance on how to use the tools in the project and highlights important details to keep in mind.

- Editor Tools:
Files must be placed in the "A User Folder" located inside the project’s Assets folder. Can be dragged and dropped straight to the editor.
`Tallqvist-VR/Assets/A User Folder`

- Runtime Tools:
Files must be located in a local user folder specific to the tool being used.
`Users/User/AppData/LocalLow/DefaultCompany/Tallqvist Tyomaa VR/PlyFiles`

Point Clouds as Quads
---------------------

The **Pcx** tool is used to import `.ply` point cloud files into the project. The Pcx shaders have been modified for VR rendering.

- Prepare the imported point cloud by changing the **Container Type** from *Mesh* to *Compute Buffer* before use. 
- After that the **Point Cloud Data** inside the `.ply` file can be dragged to the **PointCloudQuads** Inspector (**Point Cloud** field inside the script).
- In the script, under **Point Cloud**, adjust the **Quad Size** property to change the size of the quads.
- For denser point clouds, use smaller quads. A typical size range is **0.1 to 3.0.**

**Prepare the point cloud**

![Image of the Cubizer in use](/Images/PointQuadInstruction1.PNG)

**Set the point cloud to PointCloudQuads**

![Image of the Cubizer in use](/Images/PointQuadInstruction2.PNG)

When trying to view point cloud only in the editor with Pcx, the coordinate system needs to be noted. In Unity the coordinate system handles X-axis in opposite way than most point cloud data. In Unity the **X-scale** should be set to **-1** to reverse the axis. 
After adjusting the scale, update the rotation. 
- **X-scale: -1**
- **X-axis: -90°**
- **Z-axis: 180°**

Pre-edit Point Cloud
--------------------

Point cloud data can be edited in [CloudCompare]. For example, point cloud files in `.las` format can be exported to Unity as `.ply` files.

Large point clouds can be **subsampled** to reduce their size using the `Subsample` tool. The number of points can also be reduced by cutting the point cloud into smaller sections to be used.

**Important:** The **Global Shift/Scale** values should be set to `0`. If the global shift/scale is significantly different from zero, the point cloud will not appear in the Unity scene.
Use the `Edit global shift and scale` tool to adjust the **X**, **Y** and **Z** shift/scale values to `0`.

Point Clouds as Voxels
----------------------

Using **UnityVOXFileImport** to import voxel data into the project and view it in the scene with the **Cubizer** tool. 
- **Note:** The voxel file version must be 150 for Cubizer to read the file correctly.

### Loading a Voxel into the Scene
![Image of the Cubizer in use](/Images/CubizerInstruction.PNG)

**Tools --> Cubizer --> Show VOXFileLoader inspector**

Heightmaps
----------

Runtime conversion a from point cloud to `heightmap.png` cannot be built as an Android executable. This feature works only in the Editor and on Windows. 

Pre-made grayscale images, however, will work at runtime as long as they are placed in the local user folder.

IFC Models in Unity
-------------------

IFC files can be viewed in Unity after being converted to `.glb` or `.gltf` format. The conversion can be done using the [BIM] Blender extension.

Once Converted:
- Place the `.glb` file in the **A User Folder** inside the project’s **Assets** folder.
- The model can then be added to the scene without any additional steps.

Saving and Loading
------------------

The **Save** feature saves data to a `.json` file.

Currently, the save data includes:
- User position
- Prefab position
- The loaded heightmap

[BIM]: https://extensions.blender.org/add-ons/bonsai/?utm_source=blender-4.4.3
[CloudCompare]: https://cloudcompare.org/index.html 
