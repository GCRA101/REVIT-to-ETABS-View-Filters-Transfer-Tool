/* IMPORT LIBRARIES */
using Autodesk.Revit.Attributes;
// Libraries for Revit
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
// Libraries for ETABS
using ETABSv1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
// Other Libraries
using View = Autodesk.Revit.DB.View;

namespace ViewFiltersTransfer
{
    /* COMMAND CLASS ************************************************************ */

    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        /*ATTRIBUTES*/
        // Shared dialog title used for both success and error dialogs
        private const String dialogTitle = "View Filters Transfer to ETABS"; 
        private Autodesk.Revit.UI.UIDocument uiDoc;
        private Autodesk.Revit.DB.Document doc;

        /*IMPLEMENTED METHODS*/

        // Execute Method from IExternalCommand Interface
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //1. CREATE DB AND UI DOCUMENT OBJECTS
                this.uiDoc = commandData.Application.ActiveUIDocument;
                // If uiDoc is null, send error message to user and stop running the addin
                if (this.uiDoc == null) 
                {
                    // Inform the user and abort
                    TaskDialog.Show(dialogTitle, "Open a Revit model before running the tool.");
                    // Cancel the command since there is nothing to do
                    return Result.Cancelled;
                }
                // Get the Revit Document
                this.doc = uiDoc.Document;
                //2. CALL COMMAND FUNCTION
                // Run the transfer and capture a summary report
                String report = transferViewFilters();
                // Show the summary to the user
                TaskDialog.Show(dialogTitle, report); 
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                // 3. REPORT THE ERROR TO THE USER
                // Build a dialog to show details of the error
                TaskDialog errorDialog = new TaskDialog(dialogTitle);
                // High-level failure message
                errorDialog.MainInstruction = "The View Filters could not be transferred to ETABS.";
                // Short exception message
                errorDialog.MainContent = e.Message;
                // Full exception details for troubleshooting
                errorDialog.ExpandedContent = e.ToString();
                // Display the dialog to the user
                errorDialog.Show();
                // Return Result.Failed
                return Result.Failed;
            }
        }


        /* METHODS */

        // Format names so that Revit View Filters and ETABS Frame Sections can be matched
        private static String formatName(String name) 
        {
            // Strip spaces and uppercase the name
            return name.Replace(" ", "").ToUpper(); 
        }

        // Get the colour assigned to a View Filter (surface background pattern first, then foreground)
        private static Autodesk.Revit.DB.Color getFilterColor(OverrideGraphicSettings overrides)
        {
            // Prefer the surface background pattern colour
            if (overrides.SurfaceBackgroundPatternColor.IsValid) { return overrides.SurfaceBackgroundPatternColor; }
            // Fall back to the surface foreground pattern colour
            if (overrides.SurfaceForegroundPatternColor.IsValid) { return overrides.SurfaceForegroundPatternColor; }
            // If no foreground/background patterns are found, no colour override is set on the filter
            return null; 
        }

        // Remove assignment of group to all corresponding frame objects before group reassignment
        private static void removeFrameAssignments(cSapModel sapModel, String groupName)
        {
            // ETABS object type code for frame objects
            const int frameObjectType = 2;
            // Number of assignments returned by the API
            int numItems = 0;
            // Object type codes for each assignment
            int[] objectTypes = null;
            // Object names for each assignment
            string[] objectNames = null;
            // Abort if the group has no assignments
            if (sapModel.GroupDef.GetAssignments(groupName, ref numItems, ref objectTypes, ref objectNames) != 0) { return; }
            // Iterate over every object currently assigned to the group
            for (int i = 0; i < numItems; i++)
            {
                // Unassign only frame objects (true = remove)
                if (objectTypes[i] == frameObjectType) { sapModel.FrameObj.SetGroupAssign(objectNames[i], groupName, true); }
            }
        }

        private String transferViewFilters()
        {
            /*1. GET THE ACTIVE VIEW */
            View activeView = doc.ActiveView;
            // Guard against views that cannot carry View Filters
            if (activeView == null || !activeView.AreGraphicsOverridesAllowed()) 
            {
                // Abort with an explanatory message
                throw new InvalidOperationException("The active view does not support View Filters. Activate a view with View Filters assigned and try again."); 
            }

            /*2. GET ALL THE FILTERS' NAMES AND COLORS ASSIGNED TO THE ACTIVE VIEW */
            // Maps normalized filter name -> filter colour
            Dictionary<String, ColorInterface> filterColorsDict = new Dictionary<String, ColorInterface>();
            // Tracks filters that could not be transferred, for the final report
            List<String> skippedFilters = new List<String>();
            // Walk every View Filter assigned to the active view
            foreach (ElementId filterId in activeView.GetFilters())
            {
                // Get the original and unformatted filter name
                String filterName = doc.GetElement(filterId).Name;
                // Get the colour assigned to the filter in the active view
                Autodesk.Revit.DB.Color filterColor = getFilterColor(activeView.GetFilterOverrides(filterId));
                // Skip filters having a null color assigned
                if (filterColor == null) { skippedFilters.Add(filterName + " (no surface pattern colour)"); continue; }
                // Format the name to use as the ETABS group name
                String groupName = formatName(filterName);
                // Skip the filters that get a formatted name that is already in use
                if (filterColorsDict.ContainsKey(groupName)) { skippedFilters.Add(filterName + " (duplicate name)"); continue; }
                // Record all the view filters by group name and colour
                filterColorsDict.Add(groupName, new RevitColorAdapter(filterColor)); 
            }
            // If no View Filters have been found, return error message to user
            if (filterColorsDict.Count == 0)
            {
                // Abort with an explanatory message
                throw new InvalidOperationException("No View Filters with a surface pattern colour are assigned to the active view \"" + activeView.Name + "\".");
            }

            /*3. INITIALIZE CONNECTION TO OPENED ETABS INSTANCE */
            // Initialize connector
            ETABSConnector.getInstance().initialize();
            // Store ETABS Model inside local variable
            cSapModel sapModel = ETABSConnector.getInstance().getEtabsApp().SapModel;
            // If no model file is currently open in ETABS, throw an exception and send warning message to user
            if (String.IsNullOrEmpty(sapModel.GetModelFilename(true)))
            {
                throw new InvalidOperationException("No model is open in the running ETABS instance.");
            }
            // If the model file is locked and cannot be edited, throw an exception and send warning message to user
            if (sapModel.GetModelIsLocked())
            {
                throw new InvalidOperationException("The ETABS model is locked. Unlock it and try again.");
            }

            /*4. CREATE THE GROUPS IN THE ETABS MODEL AND REMOVE ANY PRE-EXISTING ASSIGNMENT */
            // Create new instance of PushGroups class
            PushGroups groupsPusher = new PushGroups(sapModel, filterColorsDict);
            // Run the push() method of the PushGroups class
            groupsPusher.push();
            // Clear any pre-existing frame assignments for each group before reassigning
            filterColorsDict.Keys.ToList().ForEach(groupName => removeFrameAssignments(sapModel, groupName));

            /*5. GROUP ETABS FRAME NAMES BY CORRESPONDING FRAME SECTION PROPERTY */
            // Initialize utility variables
            int numNames = 0;
            string[] frameNames = null;
            // Retrieve all frame object names in the model
            sapModel.FrameObj.GetNameList(ref numNames, ref frameNames);
            // Guard against a null array when the model has no frames
            if (frameNames == null) { frameNames = new string[0]; } 

            Dictionary<String, List<String>> etabsFramesDict = frameNames.
                GroupBy((string frameName) => {string framePropName = "";                   // Frame section property name for this frame
                                                string sAuto = "";                          // Auto-select list output (unused)
                                                sapModel.FrameObj.GetSection(frameName, ref framePropName, ref sAuto); // Look up the frame's section property
                                                return formatName(framePropName ?? "");}).  // Normalize the section name to match filter group names
                ToDictionary(iGroup => iGroup.Key, iGroup => iGroup.ToList());              // Build a lookup of normalized section name -> frame names

            /*6. ASSIGN GROUPS TO ETABS FRAME OBJECTS */
            // Initialize counter for frames successfully assigned to a group
            int assignedFrames = 0;
            // Iterate over every group created from a View Filter
            foreach (String groupName in filterColorsDict.Keys) 
            {
                // Skip groups with no matching ETABS frame section
                if (!etabsFramesDict.ContainsKey(groupName)) { continue; }
                // Iterate over every frame using that section
                foreach (string frameName in etabsFramesDict[groupName]) 
                {
                    // Assign the frame to the group and count success (0 = OK)
                    if (sapModel.FrameObj.SetGroupAssign(frameName, groupName, false) == 0) { assignedFrames++; }
                }
            }
            // Refresh the ETABS view so the new group assignments are visible
            sapModel.View.RefreshView(0, false);

            /* REPORT */
            // Summarize how many groups have been pushed and how many frames have been assigned successfully
            String report = filterColorsDict.Count + " group(s) created/updated in ETABS.\n" + 
                            assignedFrames + " of " + frameNames.Length + " frame object(s) assigned to a group.";
            // If some filters could not be transferred, append them to the report
            if (skippedFilters.Count > 0)
            {
                report += "\n\nSkipped View Filters:\n" + String.Join("\n", skippedFilters);
            }
            // Return the summary back to Execute() for display
            return report;
        }

    }
}
