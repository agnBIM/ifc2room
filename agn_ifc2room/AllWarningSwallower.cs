// Copyright (C) by agn Niederberghaus & Partner GmbH
// provided by agn|apps - software@agn.de
// Gerrit Maedge; Torsten Moehlenhoff; Mario Billep


using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace agn.ifc2revitRooms
{
    public class AllWarningSwallower : IFailuresPreprocessor
    {

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            List<string> failList = new List<string>();

            IList<FailureMessageAccessor> failures = failuresAccessor.GetFailureMessages();

            foreach (FailureMessageAccessor f in failures)
            {
                FailureDefinitionId id = f.GetFailureDefinitionId();

                failList.Add(f.GetDescriptionText());

                failuresAccessor.DeleteWarning(f);
            }

            return FailureProcessingResult.Continue;
        }
    }
}