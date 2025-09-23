Tallqwist-VR
============

VR project for a client. The goal is to create a VR program with Unity that allows environments to be edited, saved, and loaded internally so that new employees can explore the construction site remotely.

The project includes different approaches for **point cloud visualization for VR** including renderable quads, voxels, and heightmaps.

System Requirements
-------------------

Unity 6 (6000.0.42f1)

All development and testing was done using Meta Quest 2 VR headset connected to PC with Meta Quest Link.

Lighter point clouds containing under 300K points can also be viewed in a standalone application for Quest 2.

Supported File Formats
----------------------

- Point clouds: PLY binary little-endian format.
- Ifc objects: glTF and glb formats.
- Blueprints: png and jpg formats.

Instructions
------------

Runtime tools for point clouds are currently unavailable. Use the editor tools to render new point clouds or to modify point clouds.

More instructions [here].

Licenses & Credits
------------------

### Tools and packages used in the project
Pcx - Point Cloud Importer/Renderer for Unity: https://github.com/keijiro/Pcx.

Unity VOX File import by Rui: https://github.com/ray-cast/UnityVOXFileImport licensed under the ([MIT license]).

Unity glTFast package: https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@5.2/manual/index.html.

### This project makes use of publicly available point cloud data and ifc models
Point cloud data and ifc models are modified for use in Unity demo scenes.

Point cloud data source: https://github.com/PDAL/data/tree/main/autzen licensed under Creative Commons Attribution license ([CC BY 4.0]).

IFC model source: https://github.com/youshengCode/IfcSampleFiles.

[CC BY 4.0]: https://creativecommons.org/licenses/by/4.0/
[MIT license]: .//MIT_LICENSE.txt
[here]: .//INSTRUCTIONS.md