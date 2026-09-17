/* IMPORT LIBRARIES */
using System;
using System.Collections.Generic;
using System.Linq;
// Libraries for Revit
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
// Libraries for ETABS
using ETABSv1;
// Other Libraries
using View = Autodesk.Revit.DB.View;

namespace ViewFiltersTransfer
{
    /* COMMAND CLASS ************************************************************ */

    [Transaction(TransactionMode.Manual)] 
    public class Command : IExternalCommand
    {
        /*ATTRIBUTES*/
        private Autodesk.Revit.UI.UIApplication uiApp;
        private Autodesk.Revit.UI.UIDocument uiDoc;
        private Autodesk.Revit.DB.Document doc;

        /*IMPLEMENTED METHODS*/

        // Execute Method from IExternalCommand Interface
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //1. CREATE DB AND UI DOCUMENT OBJECTS
                //Assignment
                this.uiDoc = commandData.Application.ActiveUIDocument;
                this.doc = uiDoc.Document;
                //2. CALL COMMAND FUNCTION
                transferViewFilters();
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                return Result.Failed;
            }
        }


        /* METHODS */

        //transferViewFilters()
        private void transferViewFilters()
        {
            /*1. GET THE ACTIVE VIEW */
            View activeView = doc.ActiveView;

            /*2. GET ALL THE FILTERS' NAMES AND COLORS ASSIGNED TO THE ACTIVE VIEW */
            Dictionary<String, ColorInterface> filterColorsDict = activeView.GetFilters().
                            ToDictionary(filterId => ((ParameterFilterElement)doc.GetElement(filterId)).Name.ToString().Replace(' ','\0').ToUpper(),
                                         filterId => (ColorInterface)new RevitColorAdapter(activeView.GetFilterOverrides(filterId).SurfaceBackgroundPatternColor));

            /*3. INITIALIZE CONNECTION TO OPENED ETABS INSTANCE */
            ETABSConnector.getInstance().initialize();

            /*4. CREATE THE GROUPS IN THE ETABS MODEL */
            PushGroups groupsPusher = new PushGroups(ETABSConnector.getInstance().getEtabsApp().SapModel, filterColorsDict);
            groupsPusher.push();

            /*5. GROUP ETABS FRAME NAMES BY CORRESPONDING FRAME SECTION PROPERTY */
            int numNames = 0;
            string[] frameNames = null;
            string[] framePropNames = null;
            ETABSConnector.getInstance().getEtabsApp().SapModel.FrameObj.GetNameList(ref numNames, ref frameNames);
            ETABSConnector.getInstance().getEtabsApp().SapModel.PropFrame.GetNameList(ref numNames, ref framePropNames);

            Dictionary<String, List<String>> etabsFramesDict = frameNames.
                GroupBy(((string frameName) => {string framePropName = "";
                                                 string sAuto = "";
                                                 ETABSConnector.getInstance().getEtabsApp().SapModel.
                                                    FrameObj.GetSection(frameName, ref framePropName, ref sAuto);
                                                 return framePropName;})).
                ToDictionary(iGroup => iGroup.Key.ToString().Replace(' ', '\0').ToUpper(), iGroup => iGroup.ToList());

            /*6. ASSIGN GROUPS TO ETABS FRAME OBJECTS */
            etabsFramesDict.Keys.ToList().ForEach(framePropName => {
                foreach (string frameName in etabsFramesDict[framePropName]) {
                    // Remove any pre-existing pushed group assignment
                    filterColorsDict.ToList().ForEach(kvpair=> ETABSConnector.getInstance().getEtabsApp().SapModel.FrameObj.SetGroupAssign(frameName,kvpair.Key,true));
                    // Assign corresponding group to each frame object
                    ETABSConnector.getInstance().getEtabsApp().SapModel.FrameObj.SetGroupAssign(frameName, framePropName,false); } });


            /* USE THE CLASS MODIFYOBJGROUPASSIGN TO ASSIGN THE GROUP TO THE ELEMENTS!!!! */
            //ModifyObjGroupAssign modifyGroupAssign = new ModifyObjGroupAssign()

            /* SHUT DOWN ETABS */
            //ETABSConnector.getInstance().dispose();


        }

    }
}
