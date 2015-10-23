using UnityEngine;
using System.Collections.Generic;

public class ThreePhaseController {

    private TCPServer _tcpServer;
    private JSONDataLoader.InterfaceConfiguration _interfaces;

    public ThreePhaseController(JSONDataLoader.InterfaceConfiguration interfaces)
    {
        _tcpServer = new TCPServer(interfaces.TcpPort);
        _interfaces = interfaces;
    }

	public string[] getMasterInterfaceCommands()
    {
        if (_interfaces.MasterInterface == JSONDataLoader.InterfaceConfiguration.InterfaceType.TCP)
            return getTCPInterfaceCommands();
        else if (_interfaces.MasterInterface == JSONDataLoader.InterfaceConfiguration.InterfaceType.Keyboard)
            return getKeyboardCommands();
        else if (_interfaces.MasterInterface == JSONDataLoader.InterfaceConfiguration.InterfaceType.XBoxController)
            return getXBoxControllerCommands();
        return new string[0];
    }

    public string[] getKeyboardCommands()
    {
        if (_interfaces.KeyboardInterfacePresent)
        {
            List<string> activeCommands = new List<string>();
            for (int i = 0; i < _interfaces.KeyboardKeys.Length; i++)
                if (Input.GetKey(_interfaces.KeyboardKeys[i]))
                    activeCommands.Add(_interfaces.KeyboardCommands[i]);
            return activeCommands.ToArray();
        }
        return new string[0];
    }

    public string[] getXBoxControllerCommands()
    {
        if (_interfaces.XBoxControllerInterfacePresent)
        {
            List<string> activeCommands = new List<string>();
            for (int i = 0; i < _interfaces.XBoxControllerKeys.Length; i++)
                if (Input.GetButton(_interfaces.XBoxControllerKeys[i]))
                    activeCommands.Add(_interfaces.XBoxControllerCommands[i]);
            return activeCommands.ToArray();
        }
        return new string[0];
    }

    public string[] getTCPInterfaceCommands()
    {
        return _tcpServer.getCommands();
    }

    public void safeShutdown()
    {
        _tcpServer.safeShutdown();
    }
}
