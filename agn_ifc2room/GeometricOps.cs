// Copyright (C) by agn Niederberghaus & Partner GmbH
// provided by agn|apps - software@agn.de
// Gerrit Maedge; Torsten Moehlenhoff; Mario Billep


using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;

namespace agn.ifc2revitRooms
{
    class GeometricOps
    {
        public static CurveArray unitePolygons(List<int> Indices, List<Polybool.Net.Objects.Point> Points, IIfcProduct Product, XbimShapeGeometry Geometry, Document doc)
        {
            List<List<Polybool.Net.Objects.Point>> triPointList = new List<List<Polybool.Net.Objects.Point>>();
            List<Polybool.Net.Objects.Point> triPoints = new List<Polybool.Net.Objects.Point>();
            
            int counter = 0;

            IIfcProduct product = Product;

            foreach (int ind in Indices)
            {
                counter++;

                if (counter < 4)
                {
                    triPoints.Add(new Polybool.Net.Objects.Point(Convert.ToDecimal(Points[ind].X), Convert.ToDecimal(Points[ind].Y)));
                }
                if (counter == 3)
                {
                    triPointList.Add(triPoints);
                    counter = 0;
                    triPoints = new List<Polybool.Net.Objects.Point>();
                }
            }

            List<List<Polybool.Net.Objects.Segment>> curvesList = pointsToSegmentedTriangles(triPointList);

            Polybool.Net.Objects.Polygon solid;
            Polybool.Net.Objects.Polygon union = null;

            List<Curve> unitedPolygonsCurves = new List<Curve>();

            List<Polybool.Net.Objects.Polygon> failedSolids = new List<Polybool.Net.Objects.Polygon>();

            foreach (List<Polybool.Net.Objects.Segment> triangle in curvesList)
            {
                List<Polybool.Net.Objects.Point> pList = new List<Polybool.Net.Objects.Point>();

                foreach (Polybool.Net.Objects.Segment seg in triangle)
                {
                    pList.Add(seg.Start);
                    pList.Add(seg.End);
                }

                solid = new Polybool.Net.Objects.Polygon
                {
                    Regions = new List<Polybool.Net.Objects.Region> 
                    {
                        new Polybool.Net.Objects.Region
                        {
                            Points = pList
                        }
                    }
                };


                if (union != null)
                {
                    try
                    {
                        union = Polybool.Net.Logic.SegmentSelector.Union(solid, union);
                    }
                    catch
                    {
                        failedSolids.Add(solid);
                    }
                }
                else
                {
                    union = solid;
                }
            }

            List<Polybool.Net.Objects.Polygon> failedSolids2 = new List<Polybool.Net.Objects.Polygon>();

            //failed solids will be checked again for union possibility
            foreach (Polybool.Net.Objects.Polygon s in failedSolids)
            {
                try
                {
                    union = Polybool.Net.Logic.SegmentSelector.Union(union, s);
                }
                catch { failedSolids2.Add(s); }
            }

            foreach (Polybool.Net.Objects.Polygon s in failedSolids2)
            {
                try
                {
                    union = Polybool.Net.Logic.SegmentSelector.Union(union, s);
                }
                catch { }
            }

            XYZ pOld;
            XYZ pStart = null;
            Line c;

            Xbim.Ifc2x3.ProductExtension.IfcSpatialStructureElement prodName = product as Xbim.Ifc2x3.ProductExtension.IfcSpatialStructureElement;

            foreach (Polybool.Net.Objects.Region r in union.Regions)
            {
                pOld = null;

                foreach (Polybool.Net.Objects.Point p in r.Points)
                {
                    try
                    {
                        if (pOld != null)
                        {
                            c = Line.CreateBound(pOld, new XYZ(Convert.ToDouble(p.X), Convert.ToDouble(p.Y), 1));
                            unitedPolygonsCurves.Add(c);
                        }
                        else
                        {
                            pStart = new XYZ(Convert.ToDouble(p.X), Convert.ToDouble(p.Y), 1);
                        }

                        pOld = new XYZ(Convert.ToDouble(p.X), Convert.ToDouble(p.Y), 1);
                    }
                    catch { Logger.logContRoomInacc += product.Name + " - " + prodName.LongName + "\n"; Logger.logCountRoomInacc++; }
                }

                try
                {
                    c = Line.CreateBound(pOld, pStart);
                    unitedPolygonsCurves.Add(c);
                }
                catch { }

            }

            CurveArray unitedPolygonsArray = new CurveArray();

            foreach (Curve crv in unitedPolygonsCurves)
            {
                unitedPolygonsArray.Append(crv);
            }

            return unitedPolygonsArray;
        }


        private static List<List<Polybool.Net.Objects.Segment>> pointsToSegmentedTriangles(List<List<Polybool.Net.Objects.Point>> triPointList)
        {
            int counter2 = 0;
            int counter3 = 0;
            decimal firstX = 0;
            decimal firstY = 0;
            decimal secX = 0;
            decimal secY = 0;
            decimal thirdX = 0;
            decimal thirdY = 0;
            Polybool.Net.Objects.Point firstP = null;
            Polybool.Net.Objects.Point lastP = null;
            List<Polybool.Net.Objects.Segment> curves = new List<Polybool.Net.Objects.Segment>();
            List<List<Polybool.Net.Objects.Segment>> curvesList = new List<List<Polybool.Net.Objects.Segment>>();

            foreach (List<Polybool.Net.Objects.Point> tri in triPointList)
            {
                try
                {
                    foreach (Polybool.Net.Objects.Point x in tri)
                    {
                        Polybool.Net.Objects.Point p = new Polybool.Net.Objects.Point(Math.Round(x.X, 6), Math.Round(x.Y, 6));

                        counter2++;

                        if (counter2 == 1)
                        {
                            firstP = p;
                        }

                        if (counter2 == 2)
                        {
                            var seg = new Polybool.Net.Objects.Segment();
                            seg.Start = lastP;
                            seg.End = p;
                            curves.Add(seg);
                        }

                        if (counter2 == tri.Count())
                        {
                            var seg = new Polybool.Net.Objects.Segment();
                            seg.Start = lastP;
                            seg.End = p;
                            curves.Add(seg);
                            var seg2 = new Polybool.Net.Objects.Segment();
                            seg2.Start = p;
                            seg2.End = firstP;
                            curves.Add(seg2);

                            List<Polybool.Net.Objects.Point> pointList = new List<Polybool.Net.Objects.Point>();
                            pointList.Add(firstP);
                            pointList.Add(lastP);
                            pointList.Add(p);

                            //to do

                            foreach (Polybool.Net.Objects.Segment l in curves)
                            {
                                counter3++;
                                if (counter3 == 1)
                                {
                                    firstX = l.Start.X;
                                    firstY = l.Start.Y;
                                }
                                if (counter3 == 2)
                                {
                                    secX = l.Start.X;
                                    secY = l.Start.Y;
                                }
                                if (counter3 == 3)
                                {
                                    thirdX = l.Start.X;
                                    thirdY = l.Start.Y;
                                    counter3 = 0;
                                }
                            }

                            if (firstX == secX & firstX == thirdX)
                            {
                                curves = new List<Polybool.Net.Objects.Segment>();
                                counter2 = 0;
                                continue;
                            }

                            if (firstY == secY & firstY == thirdY)
                            {
                                curves = new List<Polybool.Net.Objects.Segment>();
                                counter2 = 0;
                                continue;
                            }

                            curvesList.Add(curves);
                            curves = new List<Polybool.Net.Objects.Segment>();
                            counter2 = 0;
                        }

                        lastP = p;
                    }
                }
                catch
                {
                    curves = new List<Polybool.Net.Objects.Segment>();
                    counter2 = 0;
                }
            }

            return curvesList;
        }


        public static double computePolygonArea(List<double> x, List<double> y)
        {
            if ((x == null) || (y == null)) //check for empty args
            {
                return 0.0;
            }
            int cornerCount = Math.Min(x.Count(), y.Count());
            if (cornerCount < 3) //check for min 3 corners
            {
                return 0.0;
            }
            double area = 0.0;

            //sum up
            for (int i = 0; i < cornerCount; i++)
            {
                //modulo-function for indexing coordinates
                area += (y[i] + y[(i + 1) % cornerCount]) * (x[i] - x[(i + 1) % cornerCount]);
            }
            return Math.Abs(area / 2.0);
        }


    }
}
