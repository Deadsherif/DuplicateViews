using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Globalization;

namespace DuplicateViews
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
           
            //Panel
            RibbonPanel panel = ribbonpanel(a);
            //Assemblylocation
            string thisassemblypath = Assembly.GetExecutingAssembly().Location;
            //Images
            #region Images
            var img = Properties.Resources.DuplicateIcon;
            ImageSource imgsc = GetImageSource(img);

            #endregion
            //Buttons
            #region Buttons
            PushButton button = panel.AddItem(new PushButtonData("Duplicate Views", "Duplicate Views", thisassemblypath, "DuplicateViews.Command")) as PushButton;

            button.Image = imgsc;
            button.LargeImage = imgsc;
            button.Enabled = true;

  
            #endregion

            a.ApplicationClosing += a_ApplicationClosing;
            a.Idling += a_Idling;
            return Result.Succeeded;
        }
        private BitmapSource GetImageSource(System.Drawing.Image img)
        {
            BitmapImage bmp = new BitmapImage();

            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            return bmp;
        }
        void a_ApplicationClosing(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs e)
        {
            throw new NotImplementedException();
        }

        void a_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {

        }
        public RibbonPanel ribbonpanel(UIControlledApplication a)
        {
            string tab = "Duplicate Views";
            RibbonPanel ribbonpanel = null;
            //create tab
            try
            {
                a.CreateRibbonTab(tab);

            }
            catch { }
            //create panel  
            try
            {
                //a.createRibbonPanel(Tab Name, Panel Name)
                RibbonPanel panel = a.CreateRibbonPanel(tab, "Duplicate Views");
            }
            catch { }
            //check if panel exist
            List<RibbonPanel> panels = a.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels)
            {
                //check if the pannel exist if it exist return the pannel if not return the new pannel
                if (p.Name == "Duplicate Views")
                {
                    ribbonpanel = p;
                    break;
                }
            }
            return ribbonpanel;

        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
