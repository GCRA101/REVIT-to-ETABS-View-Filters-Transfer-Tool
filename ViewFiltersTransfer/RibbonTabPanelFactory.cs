/* IMPORT LIBRARIES */
using System;
// Libraries for Revit
using Autodesk.Revit.UI;



namespace ViewFiltersTransfer
{
    public class RibbonTabPanelFactory
    {
        /* ATTRIBUTES */
        // Private Static Instance - SINGLETON PATTERN
        private static RibbonTabPanelFactory instance;

        /* CONSTRUCTORS */
        // Default Private - SINGLETON PATTERN
        public RibbonTabPanelFactory() { }

        /* METHODS */

        // Public Static .getInstance() Method - SINGLETON PATTERN
        public static RibbonTabPanelFactory getInstance()
        {
            if (instance == null) { instance = new RibbonTabPanelFactory(); }
            return instance;
        }

        // Public .create Method - FACTORY PATTERN
        public RibbonPanel create(UIControlledApplication application, String tabName, String panelName)
        {
            return application.CreateRibbonPanel(tabName, panelName);
        }

        // Public .getOrCreate Method - returns the existing panel, creating the tab/panel only if missing
        public RibbonPanel getOrCreate(UIControlledApplication application, String tabName, String panelName)
        {
            // CreateRibbonTab throws if the tab already exists (e.g. created by another add-in)
            try { application.CreateRibbonTab(tabName); }
            catch (Autodesk.Revit.Exceptions.ArgumentException) { }

            RibbonPanel ribbonPanel = application.GetRibbonPanels(tabName).Find(rbPanel => rbPanel.Name == panelName);
            return ribbonPanel ?? create(application, tabName, panelName);
        }
    }


}
