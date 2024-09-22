using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using Autodesk.Revit.Creation;
using Document = Autodesk.Revit.DB.Document;
using System.Xml.Linq;
using DuplicateViews.MVVM.ViewModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Threading;

namespace DuplicateViews
{
    public class Helper
    {
        public static View CreateOrGetViewTemplate(Document doc)
        {
            string viewTemplateName = "SPEED STL Export";
            var viewTemplateExist = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().Cast<View>().Where(X => X.Name == viewTemplateName).FirstOrDefault();
            if (viewTemplateExist != null) return viewTemplateExist;

            var PlanView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().Cast<ViewPlan>().Where(viewPlan => viewPlan.ViewType == ViewType.FloorPlan && viewPlan.CanBePrinted).FirstOrDefault();
            // Start a transaction to make changes
            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("Create View Template");

                var viewTemplate = PlanView.CreateViewTemplate();
                viewTemplate.Name = viewTemplateName;
                viewTemplate.Scale = 192;
                var viewparams = new List<Parameter>();
                foreach (Parameter p in viewTemplate.Parameters)
                    viewparams.Add(p);

                // Get parameters by name (safety checks needed)

                var para1 = viewparams
                  .Where(p
                    => p.Definition.Name == "VIEW_ViewComments_PWProj")
                  .FirstOrDefault();

                var para2 = viewparams
                    .Where(p
                      => p.Definition.Name == "VIEW_ViewDescriptor_PW")
                    .FirstOrDefault();
                var para3 = viewparams
                 .Where(p
                   => p.Definition.Name == "VIEW_ViewID_PWProj")
                 .FirstOrDefault();
                var para4 = viewparams
                  .Where(p
                    => p.Definition.Name == "VIEW_ViewType_PWProj")
                  .FirstOrDefault();
                var para5 = viewparams
                    .Where(p
                      => p.Definition.Name == "zUTIL_DetailStatus_PWProj")
                    .FirstOrDefault();

                // Create a list to store non-null parameters
                var parameterIds = new List<ElementId>();

                // Add non-null parameters to the list
                if (para1 != null)
                    parameterIds.Add(para1.Id);
                if (para2 != null)
                    parameterIds.Add(para2.Id);
                if (para3 != null)
                    parameterIds.Add(para3.Id);
                if (para4 != null)
                    parameterIds.Add(para4.Id);
                if (para5 != null)
                    parameterIds.Add(para5.Id);

                // Set includes
                viewTemplate.SetNonControlledTemplateParameterIds(parameterIds);


                // Commit the transaction
                transaction.Commit();
                return viewTemplate;
            }
        }
     
        public static void IsolateElement(Document document, View3D view3D, Element element)
        {
            ICollection<ElementId> elementsIds = new List<ElementId>();


            elementsIds.Add(element.Id);

            if (null == element) return;


            try
            {
                view3D.UnhideElements(elementsIds);
                view3D.IsolateElementTemporary(element.Id);
                view3D.ConvertTemporaryHideIsolateToPermanent();
                document.Regenerate();
                //bcfView.IsSectionBoxActive = false;

            }
            catch (Exception ex)
            {

            }



        }
        public static View3D Create3DView(Autodesk.Revit.DB.Document document)
        {
            Transaction tr = new Transaction(document, "Navigation");
            tr.Start();

            View3D view3D;
            FilteredElementCollector collector = new FilteredElementCollector(document);
            Func<View3D, bool> isNotTemplate = v3 => !(v3.IsTemplate) && v3.Name == "STL_3D";
            view3D = collector
             .OfClass(typeof(View3D))
             .Cast<View3D>()
             .FirstOrDefault<View3D>(isNotTemplate);

            if (view3D == null)
            {
                //create 3d View 
                var viewFamilyType = new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType)).ToElements()
                   .Cast<ViewFamilyType>().FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);
                view3D = View3D.CreateIsometric(document, viewFamilyType.Id);

                if (view3D != null)
                {
                    view3D.DisplayStyle = DisplayStyle.Shading;
                    view3D.DetailLevel = ViewDetailLevel.Fine;
                    view3D.AreAnalyticalModelCategoriesHidden = true;
                    view3D.Name = "STL_3D";
                }
                else
                    tr.RollBack();
            }

            tr.Commit();
            return view3D;
        }
        public static Solid GetElementGeometry(Element element)
        {

            var listOfSolids = new List<Solid>();
            if (element == null)
                return null;
            var elementGeo = element.get_Geometry(new Options() { DetailLevel = ViewDetailLevel.Fine });

            foreach (GeometryObject geometryObject in elementGeo)
            {
                if (geometryObject is Solid solid)
                {
                    if (solid.Volume > 0) listOfSolids.Add(solid);

                }
                else if (geometryObject is GeometryInstance geometryInstance)
                {
                    foreach (var instanceGeo in geometryInstance.GetInstanceGeometry())
                    {
                        if (instanceGeo is Solid instanceSolid)
                        {
                            if (instanceSolid.Volume > 0) listOfSolids.Add(instanceSolid);

                        }

                    }

                }

            }

            return UnionAllSolid(listOfSolids);

        }
        private static Solid UnionAllSolid(List<Solid> listOfSolids)
        {
            // we have checked before if list of solid have already elements, so we have at least one element in our list.
            var unionSolid = listOfSolids.FirstOrDefault();

            if (listOfSolids.Count == 1) return unionSolid;

            for (int i = 1; i < listOfSolids.Count; i++)
            {
                unionSolid = BooleanOperationsUtils.ExecuteBooleanOperation(unionSolid, listOfSolids[i], BooleanOperationsType.Union);

            }
            return unionSolid;

        }
        public static PlanarFace GetTopFace(Solid solid)
        {
            var faces = solid.Faces;
            foreach (PlanarFace face in faces)
                if (face != null)
                    if (Math.Round(face.FaceNormal.Z) == 1)
                        return face;



            return null;
        }




    }
}
