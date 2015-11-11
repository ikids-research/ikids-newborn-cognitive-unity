using UnityEngine;
using System.Collections;
using System.IO;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine.UI;
namespace JSONDataLoader
{
    public class JSONDataLoader
    {
        public static Configuration LoadTaskConfigurationDataFromJSON(string fullJSONFilePathAndName)
        {
            //Get the JSON contents from file
            string contents = getFileContents(fullJSONFilePathAndName);

            //Create the root task node for parsing
            JSONNode rootNode = JSONNode.Parse(contents);
            JSONClass rootClass = rootNode.AsObject;
            //Validate that the file is at least remotely formatted correctly by checking for the Task property (which contains everything)
            if (rootClass["Task"] == null)
            {
                Debug.LogError("Error: JSON does not include root Task object. See example JSON for help.");
                Application.Quit();
            }
            JSONClass taskClass = rootClass["Task"].AsObject;

            //From here on out, individual items in the Task class can be parsed

            //REQUIRED INPUTS

            //Construct the interface configuration from the Task JSONClass
            InterfaceConfiguration interfaceConfig = InterfaceConfiguration.getInterfaceConfigurationFromJSON(taskClass);

            //Construct the task procedure from the Task JSON Class
            TaskProcedure taskProc = TaskProcedure.getTaskProcedureFromJSON(taskClass);

            //Construct the default configuration given the required interface and task procedure inputs
            Configuration c = new Configuration(interfaceConfig, taskProc);

            //OPTIONAL INPUTS

            //Attempt to load GlobalPauseEnabled property if present
            if (taskClass["GlobalPauseEnabled"] == null)
                Debug.LogWarning("Warning: No GlobalPauseEnabled property set, defaulting to " + c.GlobalPauseEnabled + " for value.");
            else
                c.GlobalPauseEnabled = taskClass["GlobalPauseEnabled"].AsBool;

            //Attempt to load BackgroundColor property if present
            if (taskClass["BackgroundColor"] == null)
                Debug.LogWarning("Warning: No BackgroundColor property set, defaulting to " + c.BackgroundColor + " for value.");
            else
                c.BackgroundColor = HexToColor(taskClass["BackgroundColor"]);

            return c;
        }

        // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
        static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }

        //Helper function which just gets the text contents of a file and returns them as a string
        public static string getFileContents(string fullFilePathAndName)
        {
            FileStream fileReader = File.OpenRead(fullFilePathAndName);
            StreamReader reader = new StreamReader(fileReader);
            string contents = reader.ReadToEnd();
            reader.Close();
            fileReader.Close();

            return contents;
        }

        //Helper function that gets a keymap from a JSON array (assumed to be 2D array of strings)
        public static List<string[]> getKeyMapStringArraysFromKeyMapJSON(JSONArray keyMap)
        {
            JSONArray keysJSON = keyMap[0].AsArray;
            JSONArray commandsJSON = keyMap[1].AsArray;
            string[] keys = new string[keysJSON.Count];
            for (int i = 0; i < keysJSON.Count; i++)
                keys[i] = keysJSON[i];
            string[] commands = new string[commandsJSON.Count];
            for (int i = 0; i < commandsJSON.Count; i++)
                commands[i] = commandsJSON[i];
            List<string[]> output = new List<string[]>();
            output.Add(keys);
            output.Add(commands);
            return output;
        }
    }
    //Object for storing the configuration of the task (requires properly configured interfaces and task procedure to run)
    public class Configuration
    {
        private bool _globalPauseEnabled;
        private Color _backgroundColor;
        private InterfaceConfiguration _interfaces;
        private TaskProcedure _taskProcedure;

        public Configuration(InterfaceConfiguration interfaces, TaskProcedure taskProcedure)
        {
            _globalPauseEnabled = false;
            _backgroundColor = Color.black;
            _interfaces = interfaces;
            _taskProcedure = taskProcedure;
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        public bool GlobalPauseEnabled
        {
            get
            {
                return _globalPauseEnabled;
            }

            set
            {
                _globalPauseEnabled = value;
            }
        }

        public InterfaceConfiguration Interfaces
        {
            get
            {
                return _interfaces;
            }
        }

        public TaskProcedure TaskProcedure
        {
            get
            {
                return _taskProcedure;
            }
        }
    }
}
