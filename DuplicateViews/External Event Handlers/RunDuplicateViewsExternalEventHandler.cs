using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using DuplicateViews.MVVM.ViewModel;
using System.Windows;
using System.Linq.Expressions;
using System.Windows.Controls;
using System.Xml.Linq;
using Autodesk.Revit.DB.Mechanical;

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using DuplicateViews.MVVM.View;
using System.Threading;
using System.Windows.Documents;

using System.Data;
using Microsoft.Win32;
using System.IO.Packaging;
using System.Diagnostics;
using static DuplicateViews.Helper;
using System.Security.Cryptography;
using System.Windows.Media.Media3D;
using Autodesk.Revit.Creation;
using Document = Autodesk.Revit.DB.Document;
using Autodesk.Revit.DB.Architecture;

namespace DuplicateViews.External_Event_Handlers
{
    internal class RunDuplicateViewsExternalEventHandler : IExternalEventHandler
    {

        public MainViewModel MainviewModel { get; set; }
        public MainWindow Mainview { get; set; }


        public RunDuplicateViewsExternalEventHandler()
        {

        }
        public void Execute(UIApplication app)
        {
            try
            {
                var uidoc = app.ActiveUIDocument;
                var _doc = app.ActiveUIDocument.Document;


                if (MainviewModel.SelectedSourceView != null && MainviewModel.SelectedTargetView != null)
                {
                    using (Transaction tx = new Transaction(_doc, "Delete and Copy Elements"))
                    {
                        tx.Start();

                        // Step 1: Delete all elements in the target view except stairs and shafts
                        DeleteElementsInTargetView(_doc);

                        // Step 2: Copy elements from source view to target view, excluding stairs and shafts
                        CopyElementsFromSourceToTarget(_doc);

                        DeleteRooms(_doc);
                        // Step 3: Rename parameters for Rooms and Doors in the target view
                        //RenameParametersInTargetView(_doc);

                        tx.Commit();
                    }
               
                }


            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }



        }

        public string GetName() => "Run Tool";
        private void DeleteElementsInTargetView(Document _doc)
        {
            // Collect all elements in the target view, except stairs and shafts
            var targetCollector = new FilteredElementCollector(_doc, MainviewModel.SelectedTargetView.Id)
                .WhereElementIsNotElementType()
                .Where(e => e.Category != null &&

                            e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_Stairs &&
                            e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_ShaftOpening
                            &&
                            e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_StairsLandings &&
                            e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_StairsRuns
                            );

            // Delete collected elements
            foreach (Element elem in targetCollector)
            {

                try
                {
                    _doc.Delete(elem.Id);
                }
                catch (Exception)
                {


                }
            }
        }
        private void DeleteRooms(Document _doc)
        {
            // Collect all elements in the target view, except stairs and shafts
            var targetCollector = new FilteredElementCollector(_doc, MainviewModel.SelectedTargetView.Id).OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>().Where(X => X.GetBoundarySegments( new SpatialElementBoundaryOptions()).Count==0);

            // Delete collected elements
            foreach (Element elem in targetCollector)
            {

                try
                {
                    _doc.Delete(elem.Id);
                }
                catch (Exception)
                {


                }
            }
        }

        private void CopyElementsFromSourceToTarget(Document _doc)
        {
            var sourceCollector = new FilteredElementCollector(_doc, MainviewModel.SelectedSourceView.Id)
                .WhereElementIsNotElementType()
                .Where(e => e.Category != null &&
                            e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_Stairs &&
                            e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_ShaftOpening &&
                            e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_StairsLandings &&
                            e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_StairsRuns &&
                            e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_RvtLinks)
                .Select(X => X.Id).ToList();

           var elementIds =  ElementTransformUtils.CopyElements(MainviewModel.SelectedSourceView, sourceCollector, MainviewModel.SelectedTargetView, null, new CopyPasteOptions() );
            var rooms = new FilteredElementCollector(_doc, MainviewModel.SelectedSourceView.Id).OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType().Cast<Room>().ToList();
            var doors = new FilteredElementCollector(_doc, MainviewModel.SelectedSourceView.Id).OfCategory(BuiltInCategory.OST_Doors)
               .WhereElementIsNotElementType().Cast<FamilyInstance>().ToList();
            RenameParametersInTargetView(_doc, elementIds.ToList(),rooms,doors);
        }

        private void RenameParametersInTargetView(Document _doc,List<ElementId> elementIds,List<Room> sourceRooms,List<FamilyInstance> sourceDoors)
        {
            // Regular expression to match the floor number part (e.g., F02, F03, etc.)
            Regex regex = new Regex(@"F\d{2}");
            // Rename Room parameters
           
            foreach (ElementId id in elementIds)
            {
                if(_doc.GetElement(id) != null && _doc.GetElement(id) is  Room room)
                {
                    var targetRoom = sourceRooms.FirstOrDefault(X => Math.Round(X.Area,4)== Math.Round(room.Area, 4));
                    if (targetRoom == null) continue;
                    Parameter param = targetRoom.LookupParameter("Number");
                    if (param != null && param.HasValue)
                    {
                        // Get the current value of the "Number" parameter
                        string currentNumber = param.AsString();

                        // Match the number and extract the floor number part (e.g., F02, F03)
                        Match match = regex.Match(currentNumber);
                        if (match.Success)
                        {
                            // Extract the floor number from the view name (dynamically)
                            string newFloorNumber = ExtractFloorNumberFromView(MainviewModel.SelectedTargetView);

                            // Replace the old floor number (e.g., F02) with the new one (e.g., F04)
                            string updatedNumber = regex.Replace(currentNumber, $"F{newFloorNumber}");


                            Parameter paramTargetRoom = room.LookupParameter("Number");
                            // Set the new parameter value
                            paramTargetRoom.Set(updatedNumber);
                        }
                    }
                }
                else if (_doc.GetElement(id) != null && _doc.GetElement(id) is FamilyInstance door)
                {
                    if(door.Category.Id.IntegerValue==(int)BuiltInCategory.OST_Doors)
                    {

                        var targetDoor = sourceDoors
                     .FirstOrDefault(X =>
                         (X.Location as LocationPoint)?.Point != null &&
                         (door.Location as LocationPoint)?.Point != null &&
                         (X.Location as LocationPoint).Point.X == (door.Location as LocationPoint).Point.X &&
                         (X.Location as LocationPoint).Point.Y == (door.Location as LocationPoint).Point.Y);
                        if (targetDoor == null) continue;
                        Parameter param = targetDoor.LookupParameter("Mark");
                        if (param != null && param.HasValue)
                        {
                            // Get the current value of the "Number" parameter
                            string currentNumber = param.AsString();
                            // Match the number and extract the floor number part (e.g., F02, F03)
                            Match match = regex.Match(currentNumber);
                            if (match.Success)
                            {
                                // Extract the floor number from the view name (dynamically)
                                string newFloorNumber = ExtractFloorNumberFromView(MainviewModel.SelectedTargetView);
                                // Replace the old floor number (e.g., F02) with the new one (e.g., F04)
                                string updatedNumber = regex.Replace(currentNumber, $"F{newFloorNumber}");
                                Parameter paramTargetDoor = door.LookupParameter("Mark");
                                // Set the new parameter value
                                paramTargetDoor.Set(updatedNumber);
                            }
                        }
                    }
                }
               
            }
      

        
        }
        private string ExtractFloorNumberFromView(View targetView)
        {
            // This function extracts the floor number based on the view name
            // Assume the view name contains the floor like "3rd FLOOR" or "4th FLOOR"

            Regex floorRegex = new Regex(@"\b(\d+)(st|nd|rd|th)?\s*FLOOR\b", RegexOptions.IgnoreCase);
            Match match = floorRegex.Match(targetView.Name);

            if (match.Success)
            {
                // Extract the floor number and pad it to 2 digits (e.g., "3" becomes "03")
                string floorNumber = match.Groups[1].Value.PadLeft(2, '0');
                return floorNumber;
            }

            // Return a default floor number if it couldn't be extracted
            return "01";  // Default to first floor if not found
        }



    }

}
