using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DuplicateViews.MVVM.ViewModel;

using System.Threading;

namespace DuplicateViews.MVVM.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }
        public static MainWindow instance { get; set; }
        public bool IsClosed { get; private set; }
        public MainWindow(MainViewModel viewModel)
        {
            //LocalizeDictionary.Instance.Culture.ClearCachedData();
            //LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("it"); 
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
            Loaded += MainWindow_Loaded;
            

        }
    
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Topmost = true;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            IsClosed = true;
        }
        public static MainWindow CreateInstance(MainViewModel viewModel)
        {
            if (instance == null || instance.IsClosed)
                instance = new MainWindow(viewModel);
            else
                instance.Activate();

            return instance;
        }
        protected override void OnClosed(EventArgs e) => IsClosed = true;

       

    }
}
