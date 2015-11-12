using UnityEngine;
using System.Collections.Generic;
using JSONDataLoader;
using System;

public class ThreePhaseController {

    private TCPServer _tcpServer;
    private InterfaceConfiguration _interfaces;

    public ThreePhaseController(InterfaceConfiguration interfaces)
    {
        _tcpServer = new TCPServer(interfaces.TcpPort);
        _interfaces = interfaces;
    }

	public string[] getMasterInterfaceCommands()
    {
        if (_interfaces.MasterInterface == InterfaceConfiguration.InterfaceType.TCP)
            return getTCPInterfaceCommands(true);
        else if (_interfaces.MasterInterface == InterfaceConfiguration.InterfaceType.Keyboard)
            return getKeyboardCommands();
        else if (_interfaces.MasterInterface == InterfaceConfiguration.InterfaceType.XBoxController)
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

    public string[] getTCPInterfaceCommands(bool clearBuffer)
    {
        return _tcpServer.getCommands(clearBuffer);
    }

    public void safeShutdown()
    {
        _tcpServer.safeShutdown();
    }
}
