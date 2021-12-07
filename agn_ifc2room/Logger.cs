// Copyright (C) by agn Niederberghaus & Partner GmbH
// provided by agn|apps - software@agn.de
// Gerrit Maedge; Torsten Moehlenhoff; Mario Billep


using Autodesk.Revit.UI;
using System;
using System.IO;

namespace agn.ifc2revitRooms
{
    public static class Logger
    {
        private static string dateNow = DateTime.Now.ToString("yyyy-dd-M_HH-mm-ss");

        private static string logTextHeader = "agn | ifc2room BETA - Logfile\n";
        public static string logContFilename;
        public static string logContIfcVersion;
        public static string logContNativeApp;
        private static string logTextRoomCount = "Rooms generated: ";
        //public static string logContRoomCount;
        private static string logTextRoomInacc = "Rooms with small geometrical inaccuracy:\n";
        public static string logContRoomInacc;
        public static string logTextRoomsFailed = "Following Rooms could not be generated:\n";
        public static string logContRoomsFailed;
        private static string logTextFooter = "© agn Niederberghaus & Partner GmbH - agn|apps - software@agn.de - " + dateNow;

        public static int logCountRoomsGen = 0;
        public static int logCountRoomInacc = 0;
        public static int logCountRoomsFailed = 0;


        public static void log()
        {
            if (logContRoomInacc == null)
            {
                logTextRoomInacc = "";
            }

            if (logContRoomsFailed == null)
            {
                logTextRoomsFailed = "";
            }

            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\agn_ifc2room");

            string logfilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\agn_ifc2room\" + dateNow + "_agn_ifc2room_log.txt";

            using (StreamWriter writetext = new StreamWriter(logfilePath))
            {
                var logAll = logTextHeader + "\n" +
                                logContFilename + "\n" +
                                logContIfcVersion + "\n" +
                                logContNativeApp + "\n\n" +
                                logTextRoomCount + " " +
                                logCountRoomsGen.ToString() + "\n\n" +
                                logTextRoomInacc + "\n" +
                                logContRoomInacc + "\n" +
                                logTextRoomsFailed + "\n" +
                                logContRoomsFailed + "\n" +
                                logTextFooter;

                writetext.WriteLine(logAll);
            }


            TaskDialog dial = new TaskDialog("Info");

            if (logCountRoomsGen == 0)
            {
                TaskDialog.Show("Info", "No rooms (ifcSpace) in ifc-file");
            }
            else
            {
                dial.MainInstruction = "Rooms generated: " + logCountRoomsGen.ToString() + "\n" +
                                "Rooms geometrical interpolated: " + logCountRoomInacc.ToString() + "\n" +
                                "Rooms not generated: " + logCountRoomsFailed.ToString() + "\n\n" +
                                "You can find further information from the logfile." + "\n\n" +
                                "© agn Niederberghaus & Partner GmbH - agn|apps - software@agn.de";

                dial.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "View Logfile");
                dial.AllowCancellation = true;

                TaskDialogResult tResult = dial.Show();

                if (TaskDialogResult.CommandLink1 == tResult)
                {
                    System.Diagnostics.Process.Start(logfilePath);
                }
            }


        }


    }
}
