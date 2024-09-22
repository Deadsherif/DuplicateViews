using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DuplicateViews.External_Event_Handlers;
using DuplicateViews.MVVM.View;
using DuplicateViews.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateViews
{
    [Transaction(TransactionMode.Manual)]
    internal class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document _doc = commandData.Application.ActiveUIDocument.Document;


            RunDuplicateViewsExternalEventHandler DuplicateViewsExternalEventHandler = new RunDuplicateViewsExternalEventHandler();
            var ev = ExternalEvent.Create(DuplicateViewsExternalEventHandler);


            MainViewModel viewModel = new MainViewModel(_doc);
            var ui = MainWindow.CreateInstance(viewModel);

            DuplicateViewsExternalEventHandler.MainviewModel = viewModel;

            viewModel.Ev = ev;
            viewModel.Window = ui;



            ui.DataContext = viewModel;
            ui.ViewModel = viewModel;
            DuplicateViewsExternalEventHandler.Mainview = ui;






            ui.Show();
            return Result.Succeeded;
        }
    }

}
