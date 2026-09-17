/* IMPORT LIBRARIES */
using System;
using System.Collections.Generic;
// Libraries for Revit
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Linq;
using ViewFiltersFactory;
using System.IO;
using System.Reflection;

namespace ViewFiltersTransfer
{
    [Transaction(TransactionMode.Manual)]
    public class RibbonUI : IExternalApplication
    {
        /* ATTRIBUTES */
        //private String projectFolderPath = System.IO.Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
        private String projectFolderPath = "c:\\Users\\galbieri\\OneDrive - Buro Happold\\Structures\\10_Computational\\HOI\\Revit API Plugins\\ViewFiltersTransfer\\ViewFiltersTransfer\\";
        private RibbonPanel ribbonPanel;


        /* CONSTRUCTOR */
        //Default
        public RibbonUI() { }


        /* METHODS */

        // OnStartup - Implemented from IExternalApplication Interface
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                //1. Get or create RibbonTab "BH Plugins" and Ribbon Panel "View Filters" (independent of add-in load order)
                RibbonPanel ribbonPanel = RibbonTabPanelFactory.getInstance().getOrCreate(application, "BH Plugins", "View Filters");

                //2. Buildup Inputs for RibbonItemFactory
                String imagePath = "ViewFiltersTransfer.AppLogo64x64.png";
                String largeImagePath = "ViewFiltersTransfer.AppLogo96x96.png";
                String toolTipImagePath = "ViewFiltersTransfer.AppLogo.png";
                String toolTipText = "View Filters Transfer";
                String longDescriptionFilePath = "ViewFiltersTransfer.AppLongDescription.txt";
                String longDescription = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(longDescriptionFilePath)).ReadToEnd();
                String assemblyFullPath = Assembly.GetExecutingAssembly().Location;
                String className = "ViewFiltersTransfer.Command";

                //3. Create RibbonItem (PushButton)
                RibbonItemFactory.getInstance().create(ribbonPanel, RibbonItemType.PushButton, "View Filters\n Transfer to ETABS", imagePath,
                                                       largeImagePath, toolTipImagePath, toolTipText, longDescription, assemblyFullPath,
                                                       className);
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                return Result.Failed;
            }
        }



        // OnShutdown - Implemented from IExternalApplication Interface
        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                return Result.Failed;
            }
        }
    }


}
