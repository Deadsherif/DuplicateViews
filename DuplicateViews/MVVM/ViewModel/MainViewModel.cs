using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using DuplicateViews.External_Event_Handlers;

using DuplicateViews.MVVM.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;


namespace DuplicateViews.MVVM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Attributes
        private ExternalEvent ev;



        public ObservableCollection<ViewPlan> SourceViews { get; set; }
        public ObservableCollection<ViewPlan> TargetViews { get; set; }
        public ViewPlan SelectedSourceView { get; set; }
        public ViewPlan SelectedTargetView { get; set; }

        public ICommand CopyElementsCommand { get; private set; }

        private Document _doc;

        public MainViewModel(Document doc)
        {
            _doc = doc;
            LoadViews();
            CopyElementsCommand = new RelayCommand(P => CopyElements(P));
            

        }

        private void LoadViews()
        {
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            var viewPlans = collector.OfClass(typeof(ViewPlan)).Cast<ViewPlan>().OrderBy(X=>X.Name).ToList();
            SourceViews = new ObservableCollection<ViewPlan>(viewPlans);
            TargetViews = new ObservableCollection<ViewPlan>(viewPlans);
        }

 



        // public GridViewModel GridViewModel{ get; set; }


        #endregion
        #region Properties
        public int _Parameter { get; set; }


        public ExternalEvent Ev
        {
            get { return ev; }
            set { ev = value; OnPropertyChanged(nameof(Ev)); }
        }
 
     


        #endregion
        #region Functions
        

  

        public void CopyElements(object parameter)
        {
           
            Ev.Raise();


        }



        #endregion

   
        public MainWindow Window { get; internal set; }

    }


}
