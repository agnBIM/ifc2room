// Copyright (C) by agn Niederberghaus & Partner GmbH
// provided by agn|apps - software@agn.de
// Gerrit Maedge; Torsten Moehlenhoff; Mario Billep


using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace agn.ifc2revitRooms
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Main : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and documnet objects
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            appstoreWpf wpf = new appstoreWpf(doc);

            wpf.ShowDialog();

            if (wpf.DialogResult == true)
            {
                string filename = wpf.filename;

                using (Transaction transaction = new Transaction(doc))
                {
                    if (transaction.Start("ifc2room") == TransactionStatus.Started)
                    {
                        FailureHandlingOptions failOpt = transaction.GetFailureHandlingOptions();

                        //failures are supressed and get logged afterwards
                        failOpt.SetFailuresPreprocessor(new AllWarningSwallower());

                        transaction.SetFailureHandlingOptions(failOpt);

                        IfcRooms.deleteBoundaries(doc);

                        //rooms are searched from the selected ifc-file and get instantiated in own class
                        List<IfcRooms> rooms = IfcRooms.fetchRooms(filename, doc);

                        //all levels in revit file are deleted and all ifcStoreys are fetched from ifc and generated in revit-file
                        IfcRooms.lvlTrans(rooms, filename, doc, wpf.viewFam);

                        foreach (IfcRooms room in rooms)
                        {
                            room.placeRoom(doc);
                        }

                        transaction.Commit();

                    }
                }

                Logger.log();
            }

            return Result.Succeeded;
        }
    }
}

