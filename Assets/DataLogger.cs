using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Text;

public class DataLogger : MonoBehaviour {

    public string savePath = ".";
    private StreamWriter stateLog;
    private StreamWriter inputLog;
    private StreamWriter configLog;
    private bool _appendTimeStamp;

    // Use this for initialization
    void Awake() {
        DateTime logDateTime = DateTime.Now;

        DirectoryInfo subdir = Directory.CreateDirectory(savePath + string.Format("\\ikids_newborn_cognitive_unity_log-{0:yyyy-MM-dd_hh-mm-ss-tt}", logDateTime));

        string stateLogFilename = string.Format("ikids_newborn_cognitive_unity_state_log-{0:yyyy-MM-dd_hh-mm-ss-tt}.log", logDateTime);
        string inputLogFilename = string.Format("ikids_newborn_cognitive_unity_input_log-{0:yyyy-MM-dd_hh-mm-ss-tt}.log", logDateTime);
        string configLogFilename = string.Format("ikids_newborn_cognitive_unity_config_log-{0:yyyy-MM-dd_hh-mm-ss-tt}.log", logDateTime);

        string path = subdir.FullName + "\\";

        stateLog = new StreamWriter(path + stateLogFilename);
        inputLog = new StreamWriter(path + inputLogFilename);
        configLog = new StreamWriter(path + configLogFilename);

        stateLog.AutoFlush = true;
        inputLog.AutoFlush = true;
        configLog.AutoFlush = true;

        _appendTimeStamp = false;

        LogConfig("Start Time From System Clock:" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now));
        LogConfig("Start Time From Unity Unscaled Timer: " + Time.unscaledTime);

        _appendTimeStamp = true;
    }

    void OnApplicationQuit()
    {
        stateLog.Close();
        inputLog.Close();
        configLog.Close();
    }

    public void SetAppendTimestamp(bool appendTimestamp)
    {
        _appendTimeStamp = appendTimestamp;
    }

    public void LogConfig(string logData, bool newline)
    {
        if (_appendTimeStamp)
            configLog.Write(getTimestamp() + " : ");
        if (newline)
            configLog.WriteLine(logData);
        else
            configLog.Write(logData);
    }

    public void LogConfig(string logData)
    {
        LogConfig(logData, true);
    }

    public void LogState(string stateData, bool newline)
    {
        if (_appendTimeStamp)
            stateLog.Write(getTimestamp() + " : ");
        if (newline)
            stateLog.WriteLine(stateData);
        else
            stateLog.Write(stateData);
    }

    public void LogState(string stateData)
    {
        LogState(stateData, true);
    }

    public void LogInput(string inputData, bool newline)
    {
        if (_appendTimeStamp)
            inputLog.Write(getTimestamp() + " : ");
        if (newline)
            inputLog.WriteLine(inputData);
        else
            inputLog.Write(inputData);
    }

    public void LogInput(string inputData)
    {
        LogInput(inputData, true);
    }

    private string getTimestamp()
    {
        return Time.unscaledTime.ToString();
    }

    public static DataLogger Log;
}
