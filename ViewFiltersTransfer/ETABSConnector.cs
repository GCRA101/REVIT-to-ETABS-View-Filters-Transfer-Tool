
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using CSiAPIv1;
using ETABSv1;

using System.Threading;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.ComponentModel;
using System;

public class ETABSConnector : ETABSConnection {

    /*ATTRIBUTES ************************************************************************************/

    //Private Instance - SINGLETON PATTERN
	private static ETABSConnector instance;
    //ETABS OAPI Interoperability Variables
    private ETABSv1.cHelper helperObject;     // Helper Class Object Variable                  //O(1)
    private ETABSv1.cOAPI ETABSApp;           // ETABS Application Object Variable             //O(1)
    //Utility Variables
    private int ret;                                                                    	   //O(1)
    const Boolean etabsVisibility =false;      	   											   //O(1)


    /*PRIVATE CONSTRUCTOR ***************************************************************************/
    //Default
	private ETABSConnector() {}


    /*METHODS ***************************************************************************************/

    //GETINSTANCE() METHOD - SINGLETON PATTERN
	public static ETABSConnector getInstance() {
		if (instance==null) {
			instance = new ETABSConnector();}
		return instance;}
 

    //INITIALIZEETABS() METHOD
    public void initialize() {
        //Helper Class Object Variable
        helperObject = new ETABSv1.Helper();                                                   //O(1)
		//ETABS Application Object Variable                                                    //O(1)
		ETABSApp=null;
		//ETABSApp=helperObject.CreateObjectProgID("CSI.ETABS.API.ETABSObject");               //O(1)
        //ETABSApp = helperObject.CreateObject("c:\Program Files\Computers and Structures\ETABS 20\ETABS.exe");
        try
        {
            ETABSApp = helperObject.GetObject("CSI.ETABS.API.ETABSObject");                   //O(1)
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Could not connect to ETABS. Make sure ETABS is running with a model open " +
                                                "(and not as administrator if Revit is not).", e);
        }
        if (ETABSApp == null)
        {
            throw new InvalidOperationException("Could not connect to ETABS. Make sure ETABS is running with a model open.");
        }
    }


    // DISPOSEETABS() METHOD
	public void dispose() {
		//Close the ETABS Application
		ETABSApp.ApplicationExit(false);  														//O(1)
		//Release Memory
		ETABSApp=null;  																		//O(1)
	}


    // SETETABSVISIBILITY() METHOD
    public void setETABSVisibility(bool boolVal)
    {
        if (boolVal == false)
        {
            ret = ETABSApp.Hide();
        }
        else
        {
            ret = ETABSApp.Unhide();
        }
    }

    //Getters

    //GET ETABS APP
    public ETABSv1.cOAPI getEtabsApp() {return this.ETABSApp;}


}
