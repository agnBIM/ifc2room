// Copyright (C) by agn Niederberghaus & Partner GmbH
// provided by agn|apps - software@agn.de
// Gerrit Maedge; Torsten Moehlenhoff; Mario Billep


using Autodesk.Revit.UI;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace agn.ifc2revitRooms
{
    class Ribbon : IExternalApplication
    {
        static String addinAssmeblyPath = typeof(Ribbon).Assembly.Location;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {

            // Create ribbon tab
            String tabName = "agn|apps";
            application.CreateRibbonTab(tabName);

            // Create ribbon panel
            RibbonPanel rp = application.CreateRibbonPanel(tabName, "agn|apps");

            // Create pushbutton
            PushButtonData pbd;
            pbd = new PushButtonData("ifc2room", "ifc2room", addinAssmeblyPath, "agn.ifc2revitRooms.Main");
            pbd.LongDescription = "Implements levels, room-geometries and room-parameters from selected ifc-file.";
            pbd.LargeImage = convertFromBitmap(agn.ifc2revitRooms.Properties.Resources.Icon2_ifc2room_32);
            pbd.Image = convertFromBitmap(agn.ifc2revitRooms.Properties.Resources.Icon2_ifc2room_16);

            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.ChmFile, path + "/Resources/ContextHelp.html");
            pbd.SetContextualHelp(contextHelp);

            rp.AddItem(pbd);

            return Result.Succeeded;
        }

        public static BitmapSource convertFromBitmap(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
