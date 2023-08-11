#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Controls;

#endregion

namespace Module02Challenge
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here

            //start transaction
            using (Transaction t = new Transaction(doc))
            {
                t.Start("create elements based on lines lineStyle");


                //select elements
                UIDocument uidoc = uiapp.ActiveUIDocument;
                IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select elements");

                TaskDialog.Show("Test", "You selected " + pickList.Count.ToString() + " elements");


                //filter selected elements for model curves - each selected line becomes part of the list
                List<CurveElement> modelCurves = new List<CurveElement>();
                foreach (Element elem in pickList)
                {
                    if (elem is CurveElement)
                    {
                        CurveElement curveElem = elem as CurveElement;
                        //CurveElement curveElem = (CurveElement) elem;

                        if (curveElem.CurveElementType == CurveElementType.ModelCurve)
                        {
                            Curve currentCurve = curveElem.GeometryCurve;

                            XYZ endPoint = null;
                            if (currentCurve.IsBound == true)
                            {
                                endPoint = currentCurve.GetEndPoint(0);
                            }

                            //modelCurves.Add(curveElem);
                        }
                    }
                }


                //method to create wall

                WallType wallType1 = GetWallTypeByName(doc, "Generic - 12\" Masonry");
                WallType wallType2 = GetWallTypeByName(doc, "Generic - 8\"");
                //method to create pipe
                PipeType pipeType1 = GetPipeType(doc, "Default");
                //method to create duct
                DuctType ductType1 = GetDuctType(doc, "Default");
                //method to create duct System Type
                MEPSystemType ductSystemType = GetDuctSystemType(doc, "Supply Air");
                //method to create pipe system type
                MEPSystemType pipeSystemType = GetPipeSystemType(doc, "Domestic Hot Water");

                //create level
                Level newLevel = Level.Create(doc, 1);

                //switch
                int i = 0;
                
                foreach (Element elem in modelCurves)
                {
                    Curve curCurve = modelCurves[i].GeometryCurve;
                    GraphicsStyle curve1GS = modelCurves[i].LineStyle as GraphicsStyle;
                    switch (curve1GS.Name)
                    {
                        case "<A-GLAZ>":
                            Wall.Create(doc, curCurve, wallType1.Id, newLevel.Id, 20, 0, false, false);
                            break;
                        case "<A-WALL>":
                            Wall.Create(doc, curCurve, wallType2.Id, newLevel.Id, 20, 0, false, false);
                            break;
                        case "<M-DUCT>":
                            Duct newDuct = Duct.Create(doc, ductSystemType.Id, ductType1.Id, newLevel.Id, curCurve.GetEndPoint(0), curCurve.GetEndPoint(1));
                            break;
                        case "<P-PIPE>":
                            Pipe newPipe = Pipe.Create(doc, pipeSystemType.Id, pipeType1.Id, newLevel.Id, curCurve.GetEndPoint(0), curCurve.GetEndPoint(1));
                            break;
                    }
                    i++;
                }
                t.Commit();

                return Result.Succeeded;
            }
        }



            //method to get wall type by name
            internal WallType GetWallTypeByName(Document doc, string typeName)
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfClass(typeof(WallType));
                //collector.WhereElementIsElementType();

                foreach (WallType curType in collector)
                {
                    if (curType.Name == typeName)
                    {
                        return curType;
                    }

                }
                return null;
            }
            //method to get system type by name
            internal PipeType GetPipeType(Document doc, string typeName)

            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfClass(typeof(PipeType));
                //collector.WhereElementIsElementType();

                foreach (PipeType curType in collector)
                {
                    if (curType.Name == typeName)
                    {
                        return curType;
                    }
                }
                return null;
            }
             
            internal DuctType GetDuctType(Document doc, string typeName)
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfClass(typeof(DuctType));
                //collector.WhereElementIsElementType();

                foreach (DuctType curType in collector)
                {
                    if (curType.Name == typeName)
                    {
                        return curType;
                    }
                }

                return null;
            }

            internal MEPSystemType GetDuctSystemType(Document doc, string typeName)
            {
                FilteredElementCollector systemCollector = new FilteredElementCollector(doc);
                systemCollector.OfClass(typeof(MEPSystemType));
                MEPSystemType ductSystemType = null;
                //FilteredElementCollector collector = new FilteredElementCollector(doc);
                //collector.OfClass(typeof(DuctSystemType));

                foreach (MEPSystemType curvType in systemCollector)
                {
                     if (curvType.Name == typeName)
                     {
                         ductSystemType = curvType;
                         return ductSystemType;
                     }
                 }

                  return null;
            }

            internal MEPSystemType GetPipeSystemType(Document doc, string typeName)
            {
                 FilteredElementCollector systemCollector = new FilteredElementCollector(doc);
                 systemCollector.OfClass(typeof(MEPSystemType)); 
                 MEPSystemType pipeSystemType = null;
                 foreach (MEPSystemType curType in systemCollector)
                 {
                    if (curType.Name == "Domestic Hot Water")
                    {
                    pipeSystemType = curType;
                    break;
                     }
                 }

                  return null;
            }



            public static String GetMethod()
            {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
             }
    }
}

