<p align="center">
  <img src="https://img.shields.io/badge/REVIT%20API-2020--22-blue?style=for-the-badge">
  <img src="https://img.shields.io/badge/PLATFORM-WINDOWS-blue?style=for-the-badge">
  <img src="https://img.shields.io/badge/.NET-4.8-blue?style=for-the-badge">
  <img src="https://img.shields.io/badge/LICENSE-AGPL%20v3-blue?style=for-the-badge">
</p>

<p> <br> </p>

# <img src="https://files.agn-group.com/index.php/s/rz2ffk9k9PzpNNN/preview"> 
agn Niederberghaus &amp; Partner GmbH - agn|apps - software@agn.de <br> 
<br>
ifc2room makes it possible to read architectural rooms (in the form of levels, room geometries, <br>
room numbers and room names) from an Ifc file and transfers them to a desired Autodesk® Revit® file. <br>
The primary target group for ifc2room are mechanical and electrical engineers who use Revit themselves <br>
but are provided with the architectural models as Ifc files in openBIM projects. <br>
In order to generate the required MEP rooms in Revit, <br>
the HVAC engineers need a valid data set in the form of Revit architecture rooms.
This is exactly what ifc2room provides on the basis of Ifc files.


## Installation
Simply double-click the downloaded installer from agnBIM/ifc2room/installer to install the app/plugin.
You may need to restart the Revit to activate the app.
When you start Revit for the first time, you will be prompted to load the tool once or permanently.
Please confirm your selection.
To uninstall this App, exit the Revit if you are currently running it, simply rerun the installer, and select
the "Uninstall" button. OR, click Control Panel > Programs > Programs and Features (Windows
7/8.1/10) and uninstall as you would any other application from your system.

## Usage Instructions
1.) Open an empty Revit file (version 2020, 2021, 2022). <br>
Caution: The app deletes all levels (and the associated views) from the underlying Revit file and <br>
then creates new levels based on the Ifc file. <br>
2.) Start the program. <br>
3.) Selection of the Ifc file to be read (Double click in text field). <br>
4.) Selection of the Revit view type, which shall be the basis for the rooms. <br>
5.) Press "Start".

## Known Limitations
· Only one architectural model per Ifc file (no merged Ifc files) <br>
· Currently only tested for Ifc 2x3 <br>
· Revit spaces should not be exported as IfcSpace, otherwise they will overlay the rooms. <br>
· Due to Revit's own geometric minimum tolerances, inaccuracies within the Ifc file may cause <br>
problems with the IfcSpaces to be converted. This would have corresponding consequences for
the Revit Rooms to be generated. To compensate for this Revit-side deficit, Ifc2Room identifies
these spaces, calculates the error and documents the respective spaces in the log file. At the end
of the program run, Ifc2Room lists the converted, the unconverted and the over-calculated rooms
for checking purposes.

## Author
<img src="https://files.agn-group.com/index.php/s/miRjWfATRN9KD8r/preview">

## License
This sample is licensed under the terms of the [AGPL-3.0 License](https://opensource.org/licenses/GPL-3.0). Please see the [LICENSE](https://github.com/agnBIM/ifc2room/blob/main/LICENSE) file for full details. <br>
<br>
Our app uses the Xbim-Library, so there outstanding work shouldn't be unrecognized:<br>
Lockley, S., Benghi, C., Černý M., 2017. Xbim.Essentials: a library for interoperable building information applications. Journal of Open Source Software, 2(20), 473, https://doi.org/10.21105/joss.00473
