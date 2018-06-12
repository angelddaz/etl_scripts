#region Help:  Introduction to the script task
/* The Script Task allows you to perform virtually any operation that can be accomplished in
 * a .Net application within the context of an Integration Services control flow. 
 * 
 * Expand the other regions which have "Help" prefixes for examples of specific ways to use
 * Integration Services features within this script task. */
#endregion

#region Namespaces
using System;
using System.Data;
using Microsoft.SqlServer.Dts.Runtime;
using System.Windows.Forms;
using Ionic.Zip;
using System.IO;
#endregion

namespace ST_bf02b3de9ee6429f9abd29a96f8887e9
{
    /// <summary>
    /// ScriptMain is the entry point class of the script.  Do not change the name, attributes,
    /// or parent of this class.
    /// </summary>
    [Microsoft.SqlServer.Dts.Tasks.ScriptTask.SSISScriptTaskEntryPointAttribute]
    public partial class ScriptMain : Microsoft.SqlServer.Dts.Tasks.ScriptTask.VSTARTScriptObjectModelBase
    {
        #region VSTA generated code
        enum ScriptResults
        {
            Success = Microsoft.SqlServer.Dts.Runtime.DTSExecResult.Success,
            Failure = Microsoft.SqlServer.Dts.Runtime.DTSExecResult.Failure
        };
        #endregion
        #region Help:  Using Integration Services variables and parameters in a script
        /* To use a variable in this script, first ensure that the variable has been added to 
         * either the list contained in the ReadOnlyVariables property or the list contained in 
         * the ReadWriteVariables property of this script task, according to whether or not your
         * code needs to write to the variable.  To add the variable, save this script, close this instance of
         * Visual Studio, and update the ReadOnlyVariables and 
         * ReadWriteVariables properties in the Script Transformation Editor window.
         * To use a parameter in this script, follow the same steps. Parameters are always read-only.
         * 
         * Example of reading from a variable:
         *  DateTime startTime = (DateTime) Dts.Variables["System::StartTime"].Value;
         * 
         * Example of writing to a variable:
         *  Dts.Variables["User::myStringVariable"].Value = "new value";
         * 
         * Example of reading from a package parameter:
         *  int batchId = (int) Dts.Variables["$Package::batchId"].Value;
         *  
         * Example of reading from a project parameter:
         *  int batchId = (int) Dts.Variables["$Project::batchId"].Value;
         * 
         * Example of reading from a sensitive project parameter:
         *  int batchId = (int) Dts.Variables["$Project::batchId"].GetSensitiveValue();
         * */

        #endregion

        #region Help:  Firing Integration Services events from a script
        /* This script task can fire events for logging purposes.
         * 
         * Example of firing an error event:
         *  Dts.Events.FireError(18, "Process Values", "Bad value", "", 0);
         * 
         * Example of firing an information event:
         *  Dts.Events.FireInformation(3, "Process Values", "Processing has started", "", 0, ref fireAgain)
         * 
         * Example of firing a warning event:
         *  Dts.Events.FireWarning(14, "Process Values", "No values received for input", "", 0);
         * */
        #endregion

        #region Help:  Using Integration Services connection managers in a script
        /* Some types of connection managers can be used in this script task.  See the topic 
         * "Working with Connection Managers Programatically" for details.
         * 
         * Example of using an ADO.Net connection manager:
         *  object rawConnection = Dts.Connections["Sales DB"].AcquireConnection(Dts.Transaction);
         *  SqlConnection myADONETConnection = (SqlConnection)rawConnection;
         *  //Use the connection in some code here, then release the connection
         *  Dts.Connections["Sales DB"].ReleaseConnection(rawConnection);
         *
         * Example of using a File connection manager
         *  object rawConnection = Dts.Connections["Prices.zip"].AcquireConnection(Dts.Transaction);
         *  string filePath = (string)rawConnection;
         *  //Use the connection in some code here, then release the connection
         *  Dts.Connections["Prices.zip"].ReleaseConnection(rawConnection);
         * */
        #endregion


        /// <summary>
        /// This method is called when this script task executes in the control flow.
        /// Before returning from this method, set the value of Dts.TaskResult to indicate success or failure.
        /// To open Help, press F1.
        /// </summary>
        public void Main()
        {
            string sourceDir = Dts.Variables["User::ArchivePath"].Value.ToString();
            string archiveDir = Dts.Variables["User::ArchiveFolderForZips"].Value.ToString();
            string zipFileName = Dts.Variables["User::NewFileName"].Value.ToString();
            string[] txtList = Directory.GetFiles(sourceDir, "*.txt");
            string[] zipList = Directory.GetFiles(archiveDir, "*.zip");


            /* First Step: 
             * 
             * Zips up all of the .txt files in sourceDir and then saves the .zip file into archiveDir. 
             * 
             */

            using (ZipFile zip = new ZipFile())
            try
            {
                {
                    // Put all of the *.txt files within the archive folder into a string array


                    // Loop through the files in the folder
                    foreach (string currentFile in txtList)
                    {
                        //Add file to new Zip
                        zip.AddFile(currentFile, "");
                    }
                    //Save the Zip file
                    zip.Save(archiveDir + zipFileName);
                }
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                Console.WriteLine(dirNotFound.Message);
                Dts.TaskResult = (int)ScriptResults.Failure;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed at zipping *.txt files", e);
                Dts.Events.FireError(0, "Zipping and Moving *txt files", e.Message + "\r" + e.StackTrace, String.Empty, 0);
            }


            /* Second Step: 
             * 
             * Delete the *.txt files in txtlist, which is the array (aka list), of .txt files in sourceDir 
             * 
             */
            try
            {
                foreach (string f in txtList)
                {
                    File.Delete(f);
                }
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                Console.WriteLine(dirNotFound.Message);
                Dts.TaskResult = (int)ScriptResults.Failure;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed at deleting *.txt files", ex);
                Dts.Events.FireError(0, "Deleting *txt files", ex.Message + "\r" + ex.StackTrace, String.Empty, 0);
            }


            /* Third Step: 
             * 
             * Delete zip files that were created more than 45 days ago
             * 
             */

            try
            {
                foreach (string zipFile in zipList)
                    //loops through the list of zip folders
                {
                    DateTime creation = File.GetCreationTime(zipFile);
                    if (creation < DateTime.Now.AddDays(-45))
                        //if zip filese were created more than 45 days ago, they will be deleted.
                    {
                        File.Delete(zipFile);
                    }
                }
            }
            catch (Exception ex)
            {              //An error occurred.
                Dts.Events.FireError(0, "Deleting Old Zip Files", ex.Message + "\r" + ex.StackTrace, String.Empty, 0);
                Dts.TaskResult = (int)ScriptResults.Failure;
            }
            Dts.TaskResult = (int)ScriptResults.Success;

        }
    }
}