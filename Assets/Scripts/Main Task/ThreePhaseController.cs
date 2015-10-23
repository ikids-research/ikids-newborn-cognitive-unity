using UnityEngine;
using System.Collections.Generic;

public class ThreePhaseController {

    public ThreePhaseController(JSONDataLoader.InterfaceConfiguration interfaces)
    {

    }

    private string[] keys = { "a", "b", "c", "d", "up", "down", "left", "right", "space" };
    private string[] commands = { "alpha", "beta", "charlie", "delta", "up", "down", "left", "right", "pause"};

	public string[] getMasterInterfaceCommands()
    {
        List<string> activeCommands = new List<string>();
        for(int i = 0; i < keys.Length;i++)
            if (Input.GetKey(keys[i]))
                activeCommands.Add(commands[i]);
        return activeCommands.ToArray();
    }
}
