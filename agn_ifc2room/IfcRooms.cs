// Copyright (C) by agn Niederberghaus & Partner GmbH
// provided by agn|apps - software@agn.de
// Gerrit Maedge; Torsten Moehlenhoff; Mario Billep


using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Xbim.Ifc;
using Xbim.ModelGeometry.Scene;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc2x3.ProductExtension;
using Autodesk.Revit.DB.Architecture;

namespace agn.ifc2revitRooms
{
    class IfcRooms
    {
        private string name;
        private string globalId;
        private string number;
        private string level;
        private CurveArray footprint;
        private XYZ tagPoint;
        private ViewPlan view;
        private double height;


        public IfcRooms(string Name, string Number, string Level, CurveArray Footprint, XYZ TagPoint, double Height,
            string GlobalId)
        {
            name = Name;
            number = Number;
            level = Level;
            footprint = Footprint;
            tagPoint = TagPoint;
            height = Height;
            globalId = GlobalId;
        }

        public static List<IfcRooms> fetchRooms(string IfcPath, Document doc)
        {
            List<IfcRooms> roomList = new List<IfcRooms>();

            //ifc file is opened in xbim
            using (IfcStore model = IfcStore.Open(IfcPath, null))
            {
                Logger.logContFilename = System.IO.Path.GetFileName(model.FileName);
                Logger.logContIfcVersion = model.Header.SchemaVersion;
                Logger.logContNativeApp = model.Header.CreatingApplication;

                var context = new Xbim3DModelContext(model);
                context.CreateContext();
                var instances = context.ShapeInstances();

                double scalingFactor = model.ModelFactors.LengthToMetresConversionFactor;

                foreach (var instance in instances)
                {
                    var productId = instance.IfcProductLabel;
                    IIfcProduct product = model.Instances[productId] as IIfcProduct;

                    //ifcspaces are filtered
                    if (product.ToString().Contains("SPACE"))
                    {
                        //get geometrical data
                        try
                        {
                            var geometry = context.ShapeGeometry(instance);

                            XbimMatrix3D transform = instance.Transformation;

                            List<XYZ> pointsTri = new List<XYZ>();
                            List<Polybool.Net.Objects.Point> pointsGeom = new List<Polybool.Net.Objects.Point>();
                            pointsGeom.Clear();
                            pointsTri.Clear();

                            foreach (XbimPoint3D str in geometry.Vertices)
                            {
                                var newPoint = transform.Transform(str);
#if DBG20
                                pointsTri.Add(new XYZ(UnitUtils.ConvertToInternalUnits(newPoint.X*scalingFactor, DisplayUnitType.DUT_METERS_CENTIMETERS), UnitUtils.ConvertToInternalUnits(newPoint.Y*scalingFactor, DisplayUnitType.DUT_METERS_CENTIMETERS), UnitUtils.ConvertToInternalUnits(newPoint.Z*scalingFactor, DisplayUnitType.DUT_METERS_CENTIMETERS)));
                                pointsGeom.Add(new Polybool.Net.Objects.Point(Convert.ToDecimal(UnitUtils.ConvertToInternalUnits(newPoint.X*scalingFactor, DisplayUnitType.DUT_METERS_CENTIMETERS)), Convert.ToDecimal(UnitUtils.ConvertToInternalUnits(newPoint.Y*scalingFactor, DisplayUnitType.DUT_METERS_CENTIMETERS))));

#else
                                pointsTri.Add(new XYZ(UnitUtils.ConvertToInternalUnits(newPoint.X*scalingFactor, UnitTypeId.Meters), UnitUtils.ConvertToInternalUnits(newPoint.Y * scalingFactor, UnitTypeId.Meters), UnitUtils.ConvertToInternalUnits(newPoint.Z * scalingFactor, UnitTypeId.Meters)));
                                pointsGeom.Add(new Polybool.Net.Objects.Point(Convert.ToDecimal(UnitUtils.ConvertToInternalUnits(newPoint.X * scalingFactor, UnitTypeId.Meters)), Convert.ToDecimal(UnitUtils.ConvertToInternalUnits(newPoint.Y * scalingFactor, UnitTypeId.Meters))));
 
#endif                           
                            }

                            WexBimMeshFace footPrintFace = null;

                            //bottom face of ifcspace is selected
                            foreach (WexBimMeshFace face in geometry.Faces)
                            {
                                if (face.Normals.First().Z == 1)
                                {
                                    footPrintFace = face;
                                }
                            }

                            List<int> indices = new List<int>();

                            foreach (int i in footPrintFace.Indices)
                            {
                                indices.Add(i);
                            }

                            CurveArray curves = GeometricOps.unitePolygons(indices, pointsGeom, product, geometry, doc);

                            List<XYZ> firstTri = new List<XYZ>();
                            int count = 0;

                            //triangle for taggingpoint ist searched
                            foreach (int i in indices)
                            {
                                count++;

                                if (firstTri.Count() < 3 && count > 3)
                                {
                                    firstTri.Add(pointsTri[i]);
                                }
                                if (firstTri.Count() == 3)
                                {
                                    List<double> x = new List<double>();
                                    List<double> y = new List<double>();

                                    foreach (XYZ p in firstTri)
                                    {
                                        x.Add(p.X);
                                        y.Add(p.Y);
                                    }

                                    double polArea = GeometricOps.computePolygonArea(x, y);
                                    if (polArea < 0.3)
                                    {
                                        firstTri = new List<XYZ>();
                                    }
                                }
                            }

                            //triangled centroid for tagging is calculated
                            XYZ triCentroid = new XYZ((firstTri[0].X + firstTri[1].X + firstTri[2].X) / 3, (firstTri[0].Y + firstTri[1].Y + firstTri[2].Y) / 3, 0);

                            //get information data
                            string instanceLevel = "";
                            string roomName = "";
                            string roomNumber = "";

                            //get the element properties
                            IEnumerable<IIfcPropertySingleValue> properties = product.IsDefinedBy.Where(r => r.RelatingPropertyDefinition is IIfcPropertySet).SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties).OfType<IIfcPropertySingleValue>();

                            //set level in instance from ifcparam
                            instanceLevel = getFloor(product as IfcSpace).Name.ToString();

                            //set name and number in instance from ifcparam
                            IfcSpatialStructureElement param = product as IfcSpatialStructureElement;
                            roomName = param.LongName;
                            roomNumber = product.Name;
                            var globalId = product.GlobalId.ToString();

#if DBG20

                            double roomHeight = UnitUtils.ConvertToInternalUnits(instance.BoundingBox.SizeZ * scalingFactor, DisplayUnitType.DUT_METERS_CENTIMETERS);
#else
                            double roomHeight = UnitUtils.ConvertToInternalUnits(instance.BoundingBox.SizeZ * scalingFactor, UnitTypeId.Meters);

#endif

                            roomList.Add(new IfcRooms(roomName, roomNumber, instanceLevel, curves, triCentroid, roomHeight, globalId));

                        }
                        catch 
                        {
                            IfcSpatialStructureElement x = product as IfcSpatialStructureElement;
                            Logger.logContRoomsFailed += product.Name + " - " + x.LongName + "\n";
                            Logger.logCountRoomsFailed++;
                        }
                    }
                }
            }

            Logger.logCountRoomsGen = roomList.Count();

            return roomList;
        }

        public static void lvlTrans(List<IfcRooms> Rooms, string IfcPath, Document doc, ViewFamilyType viewFam)
        {
            List<IfcBuildingStorey> allstories = null;

            double scalingFactor;

            //open ifc-file and get storeys
            using (IfcStore model = IfcStore.Open(IfcPath, null))
            {
                allstories = model.Instances.OfType<IfcBuildingStorey>().ToList();

                scalingFactor = model.ModelFactors.LengthToMetresConversionFactor;
            }

            //get existing levels from revit-file and delete them
            FilteredElementCollector lvlCollector = new FilteredElementCollector(doc);
            IList<Element> lvl = lvlCollector.OfClass(typeof(Level)).ToElements();

            IList<ElementId> lvlIds = new List<ElementId>();

            foreach (Element ele in lvl)
            {
                lvlIds.Add(ele.Id);
            }

            try
            {
                doc.Delete(lvlIds);
            }
            catch
            {
                foreach (ElementId eleId in lvlIds)
                {
                    try
                    {
                        doc.Delete(eleId);
                    }
                    catch
                    {
                        doc.GetElement(eleId).Name = "DELETION_UNSUCCESSFULL(View_Opened)_Please_delete_manually";
                    };
                }
            }
            

            List<ViewPlan> newViews = new List<ViewPlan>();

            //generate new levels from ifc-info in revit-file
            foreach (IfcBuildingStorey storey in allstories)
            {
#if DBG20

                Level newLvl = Level.Create(doc, UnitUtils.ConvertToInternalUnits(storey.Elevation.Value * scalingFactor, DisplayUnitType.DUT_METERS_CENTIMETERS));

#else

                Level newLvl = Level.Create(doc, UnitUtils.ConvertToInternalUnits(storey.Elevation.Value * scalingFactor, UnitTypeId.Meters));

#endif
                newLvl.Name = storey.Name;

                ViewPlan newViewPlan = ViewPlan.Create(doc, viewFam.Id, newLvl.Id);

                newViewPlan.Name = storey.Name;

                newViews.Add(newViewPlan);
            }

            //set viewplan in room instance for boundary placement
            foreach (IfcRooms room in Rooms)
            {
                foreach (ViewPlan vp in newViews)
                {
                    if (room.level == vp.Name)
                    {
                        room.view = vp;
                    }
                }
            }

        }

        public void placeRoom(Document doc)
        {
            CurveArray newCurves = new CurveArray();

            //create lines on associated levelheight from footprint in instance
            foreach (Curve c in this.footprint)
            {
                try
                {
                    XYZ startOld = c.GetEndPoint(0);
                    XYZ endOld = c.GetEndPoint(1);

                    XYZ start = new XYZ(startOld.X, startOld.Y, this.view.GenLevel.Elevation);
                    XYZ end = new XYZ(endOld.X, endOld.Y, this.view.GenLevel.Elevation);

                    Line line = Line.CreateBound(start, end);

                    newCurves.Append(line);
                }
                catch { }        
            }

            List<string> failures = new List<string>();

            //place boundaries, rooms and set parameters
            try
            {
                doc.Create.NewRoomBoundaryLines(this.view.SketchPlane, newCurves, this.view);

                Room newRoom = doc.Create.NewRoom(this.view.GenLevel, new UV(this.tagPoint.X, this.tagPoint.Y));

                newRoom.Name = this.name;
                newRoom.Number = this.number;
                        
                //limitoffsett property is not working therefore builtinparameter
                newRoom.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(this.height);
            }
            catch { };

        }


        private static IIfcBuildingStorey getFloor(IIfcSpace space)
        {
            return
                //get all objectified relations which model decomposition by this space
                space.Decomposes

                //select decomposed objects (these might be either other space or building storey)
                .Select(r => r.RelatingObject)

                //get only storeys
                .OfType<IIfcBuildingStorey>()

                //get the first one
                .FirstOrDefault();
        }


        public static void deleteBoundaries(Document doc)
        {
            FilteredElementCollector viewCollector = new FilteredElementCollector(doc);
            IList<Element> views = viewCollector.OfClass(typeof(View)).ToElements();

            foreach (View v in views)
            {
                if (v.ViewType == ViewType.FloorPlan && v.IsTemplate == false)
                {
                    FilteredElementCollector roomBoundCollector = new FilteredElementCollector(doc, v.Id);
                    IList<Element> modelCurves = roomBoundCollector.OfClass(typeof(CurveElement)).ToElements();

                    foreach (CurveElement mC in modelCurves)
                    {
                        doc.Delete(mC.Id);
                    }
                }
            }
        }
    }

}
