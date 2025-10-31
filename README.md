VR construction site
============

https://github.com/user-attachments/assets/ed334de1-e047-45cd-afe3-39a2dc5d95dd



[Longer mp4 version]

This project is developed for a client with the goal of creating a **VR application in Unity** that allows users to edit, save, and load environments internally, enabling new employees to **explore construction sites remotely**.

The project includes different approaches for **point cloud visualization for VR** including renderable **quads**, **voxels**, and **heightmaps**.

System Requirements
-------------------

**Unity Version:** Unity 6.2 (6000.2.6f2)  <br />
*Most development prior to the security patch was done using Unity 6 (6000.0.42f1).*

All development and testing were conducted using Meta Quest 2 VR headset connected to PC via Meta Quest Link.

Lightweight point clouds containing **under 300K points** can also be viewed as a **standalone application** on the **Meta Quest 2** headset.

Supported File Formats
----------------------

- **Point clouds:** `.ply` (binary little-endian)
- **IFC objects:** `.glTF` and `.glb`
- **Blueprints:** `.png` and `.jpg`

Instructions
------------

Runtime tools for point clouds are currently unavailable. Use the editor tools to render new point clouds or to modify point clouds.

More detailed instructions can be found [here].

Licenses & Credits
------------------

### Tools and Packages Used
- **Pcx** - Point Cloud Importer/Renderer for Unity: https://github.com/keijiro/Pcx.

- **Unity VOX File import** by Rui: https://github.com/ray-cast/UnityVOXFileImport (licensed under the [MIT license])

- **Unity glTFast package:** https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@5.2/manual/index.html.

### Data Sources
This project makes use of publicly available point cloud data and IFC models, which have been modified for demonstration purposes in Unity scenes.

- **Point Cloud Data:** https://github.com/PDAL/data/tree/main/autzen (licensed under Creative Commons Attribution license [CC BY 4.0])

- **IFC Model Samples:** https://github.com/youshengCode/IfcSampleFiles

[CC BY 4.0]: https://creativecommons.org/licenses/by/4.0/
[MIT license]: .//MIT_LICENSE.txt
[here]: .//INSTRUCTIONS.md
[Longer mp4 version]: /Videos/DemoVideo.mp4

This project is part of the AIMlearning project, which is co-funded by the European Union. The project will run from 1.2.2024 to 30.4.2027.
