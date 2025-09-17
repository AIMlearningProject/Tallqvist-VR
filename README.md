# Tallqwist-VR
VR project for a client. The goal is to create a VR program with Unity that allows environments to be edited, saved, and loaded internally so that new employees can explore the construction site remotely.

The project includes different approaches for point cloud visualization for VR including renderable quads, voxels, and heightmaps.

## System Requirements
Unity 6 (6000.0.42f1)
All development and testing was done using Meta Quest2 VR headset connected to PC with Meta Quest Link.
Lighter point clouds (under 300K points) can also be viewed in a standalone application for Quest2.

## Supportend Formats
Point clouds: PLY binary little-endian format.
Ifc objects: glTF and glb formats.
Blueprints: png and jpg formats.

This project makes use of publicly available point cloud data and ifc models:
Point cloud data and ifc models are modified for use in Unity demo scenes.
**Licensed under Creative Commons Attribution 4.0 International:** https://creativecommons.org/licenses/by/4.0/ 
**Point cloud data source:** https://github.com/PDAL/data/tree/main/autzen
**IFC model source:** https://github.com/youshengCode/IfcSampleFiles 
