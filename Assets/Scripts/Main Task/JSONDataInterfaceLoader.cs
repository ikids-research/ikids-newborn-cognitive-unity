using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
namespace JSONDataLoader
{
    //Object for storing the interface configuration which is compatible with any combination of Keyboard, XBoxController, and TCP stream (but only one of each at the moment)
    public class InterfaceConfiguration
    {
        public enum InterfaceType
        {
            Keyboard = 0, XBoxController = 1, TCP = 2
        };

        private bool _keyboardInterfacePresent;
        private bool _xBoxControllerInterfacePresent;
        private bool _tcpInterfacePresent;

        private string[] _keyboardKeys;
        private string[] _keyboardCommands;

        private string[] _xBoxControllerKeys;
        private string[] _xBoxControllerCommands;

        private string[] _tcpKeys;
        private string[] _tcpCommands;
        private int _tcpPort;

        private InterfaceType _masterInterface;

        public InterfaceConfiguration()
        {
            _keyboardInterfacePresent = false;
            _xBoxControllerInterfacePresent = false;
            _tcpInterfacePresent = false;
            _masterInterface = InterfaceType.Keyboard;
            _tcpPort = 11235;
        }

        public void setKeyboardMap(string[] keys, string[] commands)
        {
            if (keys.Length != commands.Length)
            {
                Debug.LogError("Error: KeyMap for Keyboard Interface has different length for keys and commands.");
                Application.Quit();
            }

            _keyboardInterfacePresent = true;
            _keyboardKeys = keys;
            _keyboardCommands = commands;
        }

        public void setXBoxControllerMap(string[] keys, string[] commands)
        {
            if (keys.Length != commands.Length)
            {
                Debug.LogError("Error: KeyMap for Keyboard Interface has different length for keys and commands.");
                Application.Quit();
            }

            _xBoxControllerInterfacePresent = true;
            _xBoxControllerKeys = keys;
            _xBoxControllerCommands = commands;
        }

        public void setTCPMap(string[] keys, string[] commands)
        {
            if (keys.Length != commands.Length)
            {
                Debug.LogError("Error: KeyMap for Keyboard Interface has different length for keys and commands.");
                Application.Quit();
            }

            _tcpInterfacePresent = true;
            _tcpKeys = keys;
            _tcpCommands = commands;
        }

        public void setTCPPort(int port)
        {
            _tcpPort = port;
        }

        public void setMaster(InterfaceType type)
        {
            switch (type)
            {
                case InterfaceType.Keyboard:
                    if (KeyboardInterfacePresent)
                        _masterInterface = InterfaceType.Keyboard;
                    else
                    {
                        Debug.LogError("Error: Attempt to set Keyboard interface as master but no Keyboard KeyMap is present.");
                        Application.Quit();
                    }
                    break;
                case InterfaceType.XBoxController:
                    if (XBoxControllerInterfacePresent)
                        _masterInterface = InterfaceType.XBoxController;
                    else
                    {
                        Debug.LogError("Error: Attempt to set XBoxController interface as master but no XBoxController KeyMap is present.");
                        Application.Quit();
                    }
                    break;
                case InterfaceType.TCP:
                    if (TcpInterfacePresent)
                        _masterInterface = InterfaceType.TCP;
                    else
                    {
                        Debug.LogError("Error: Attempt to set TCP interface as master but no TCP KeyMap is present.");
                        Application.Quit();
                    }
                    break;
            }
        }

        public bool isValidInterface()
        {
            return KeyboardInterfacePresent || XBoxControllerInterfacePresent || TcpInterfacePresent;
        }

        public bool KeyboardInterfacePresent
        {
            get
            {
                return _keyboardInterfacePresent;
            }
        }

        public bool XBoxControllerInterfacePresent
        {
            get
            {
                return _xBoxControllerInterfacePresent;
            }
        }

        public bool TcpInterfacePresent
        {
            get
            {
                return _tcpInterfacePresent;
            }
        }

        public string[] KeyboardKeys
        {
            get
            {
                return _keyboardKeys;
            }
        }

        public string[] KeyboardCommands
        {
            get
            {
                return _keyboardCommands;
            }
        }

        public string[] XBoxControllerKeys
        {
            get
            {
                return _xBoxControllerKeys;
            }
        }

        public string[] XBoxControllerCommands
        {
            get
            {
                return _xBoxControllerCommands;
            }
        }

        public string[] TcpKeys
        {
            get
            {
                return _tcpKeys;
            }
        }

        public string[] TcpCommands
        {
            get
            {
                return _tcpCommands;
            }
        }

        public InterfaceType MasterInterface
        {
            get
            {
                return _masterInterface;
            }
        }

        public int TcpPort
        {
            get
            {
                return _tcpPort;
            }
        }
        //Helper function that generates an InterfaceConfiguration object from a JSONClass which contains Interfaces in the appropriate format
        public static InterfaceConfiguration getInterfaceConfigurationFromJSON(JSONClass taskClass)
        {
            InterfaceConfiguration interfaceConfig = new InterfaceConfiguration();

            if (taskClass["Interfaces"] == null)
            {
                Debug.LogError("Error: JSON does not include Interfaces object. See example JSON for help.");
                Application.Quit();
            }
            else
            {
                JSONArray interfaces = taskClass["Interfaces"].AsArray;
                for (int i = 0; i < interfaces.Count; i++)
                {
                    JSONNode iFace = interfaces[i];
                    int port = iFace["Port"].AsInt;
                    JSONArray keyMap = iFace["KeyMap"].AsArray;
                    List<string[]> map = JSONDataLoader.getKeyMapStringArraysFromKeyMapJSON(keyMap);
                    switch (iFace["InterfaceType"])
                    {
                        case "Keyboard":
                            interfaceConfig.setKeyboardMap(map[0], map[1]);
                            break;
                        case "XBoxController":
                            interfaceConfig.setXBoxControllerMap(map[0], map[1]);
                            break;
                        case "TCP":
                            interfaceConfig.setTCPMap(map[0], map[1]);
                            if (port != 0) interfaceConfig.setTCPPort(port);
                            break;
                    }
                }
            }

            if (taskClass["InterfaceMaster"] == null)
            {
                Debug.LogError("Error: JSON does not include InterfaceMaster object. See example JSON for help.");
                Application.Quit();
            }
            else
            {
                switch (taskClass["InterfaceMaster"])
                {
                    case "Keyboard":
                        interfaceConfig.setMaster(InterfaceConfiguration.InterfaceType.Keyboard);
                        break;
                    case "XBoxController":
                        interfaceConfig.setMaster(InterfaceConfiguration.InterfaceType.XBoxController);
                        break;
                    case "TCP":
                        interfaceConfig.setMaster(InterfaceConfiguration.InterfaceType.TCP);
                        break;
                    default:
                        Debug.LogError("Error: InterfaceMaster value is not recognized. Please select either Keyboard, XBoxController, or TCP.");
                        Application.Quit();
                        break;
                }
            }

            if (!interfaceConfig.isValidInterface())
            {
                Debug.LogError("Error: Interface is not valid. This is likely because none of the specified interfaces in the JSON properly loaded or no interface was specified. See example JSON for help.");
                Application.Quit();
            }

            return interfaceConfig;
        }
    }
}
