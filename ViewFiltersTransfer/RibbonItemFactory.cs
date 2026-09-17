/* IMPORT LIBRARIES */
using System;
using System.IO;
using System.Reflection;
// Libraries for UI Buttons/Panels
using System.Windows.Media.Imaging;
using System.Drawing;
// Libraries for Revit
using Autodesk.Revit.UI;
using System.Windows.Media;

namespace ViewFiltersFactory
{
    public class RibbonItemFactory
    {
        /* ATTRIBUTES */
        // Private Static instance - SINGLETON PATTERN 
        private static RibbonItemFactory instance;

        /* CONSTRUCTORS */
        // Default Private - SINGLETON PATTERN
        private RibbonItemFactory() { }

        /* METHODS */

        // Public Static .getInstance() Method - SINGLETON PATTERN
        public static RibbonItemFactory getInstance()
        {
            if (instance == null) { return new RibbonItemFactory(); }
            return instance;
        }

        // Public .create Method - FACTORY PATTERN
        public RibbonItem create(RibbonPanel tabPanel, RibbonItemType ribItemType, String displayName, String imagePath,
                                 String largeImagePath, String toolTipImagePath, String toolTipText, String longDescription,
                                 String assemblyFullPath, String className)
        {
            RibbonItem ribbonItem;

            switch (ribItemType)
            {
                case RibbonItemType.PushButton:
                    {
                        return createPushButton(tabPanel, displayName, imagePath, largeImagePath, toolTipImagePath,
                                             toolTipText, longDescription, assemblyFullPath, className);
                    }
                case RibbonItemType.PulldownButton: return createPullDownButton();
                case RibbonItemType.SplitButton: return createSplitButton();
                case RibbonItemType.ToggleButton: return createToggleButton();
                case RibbonItemType.TextBox: return createTextBox();
                case RibbonItemType.ComboBox: return createComboBox();
                case RibbonItemType.ComboBoxMember: return createComboBoxMember();
                case RibbonItemType.RadioButtonGroup: return createRadioButtonGroup();
                default: return null;
            }
        }

        //Private Method createPushButton - FACTORY PATTERN
        private Autodesk.Revit.UI.PushButton createPushButton(RibbonPanel tabPanel, String displayName, String imagePath, String largeImagePath,
                 String toolTipImagePath, String toolTipText, String longDescription, String assemblyName, String className)
        {
            String innerName = "PushButton" + displayName;
            PushButtonData pushButtonData = new PushButtonData(innerName, displayName, assemblyName, className);

            PushButton pushButton = (PushButton)tabPanel.AddItem(pushButtonData);

            pushButton.Image = BitmapImageFactory.getInstance().create(imagePath);
            pushButton.LargeImage = BitmapImageFactory.getInstance().create(largeImagePath);
            pushButton.ToolTipImage = BitmapImageFactory.getInstance().create(toolTipImagePath);
            pushButton.ToolTip = toolTipText;
            pushButton.LongDescription = longDescription;

            return pushButton;

        }

        private Autodesk.Revit.UI.PulldownButton createPullDownButton()
        {
            return null;
        }
            
        private Autodesk.Revit.UI.SplitButton createSplitButton()
        {
            return null;
        }

        private Autodesk.Revit.UI.ToggleButton createToggleButton()
        {
            return null;
        }

        private Autodesk.Revit.UI.TextBox createTextBox()
        {
            return null;
        }

        private Autodesk.Revit.UI.ComboBox createComboBox()
        {
            return null;
        }

        private Autodesk.Revit.UI.ComboBoxMember createComboBoxMember()
        {
            return null;
        }

        private Autodesk.Revit.UI.RadioButtonGroup createRadioButtonGroup()
        {
            return null;
        }

    }


}
