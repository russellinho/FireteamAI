﻿namespace Koobando.AntiCheat
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    public class IllegalProgramsDetector : MonoBehaviour
    {
        void Awake()
        {
            StartCoroutine("ScanForIllegalPrograms");
        }

        // Scans background processes on the client machine for illegal programs based off of process names.
        // If one is found, the game quits immediately.
        // Words: Cheat, Engine, Editor, Hack, Tool, Opcode, Inject, Injector

        IEnumerator ScanForIllegalPrograms() {
            Process[] localProcesses = Process.GetProcesses();
            for (int i = 0; i < localProcesses.Length; i++) {
                try {
                    string localProcessName = localProcesses[i].ProcessName.ToLower();
                    if (localProcessName.Contains("cheat") ||
                        localProcessName.Contains("hack") ||
                        localProcessName.Contains("inject") ||
                        localProcessName.Contains("injector")) {
                            Application.Quit();
                    }
                } catch (Exception e) {
                    // Debug.LogWarning("An error occurred while trying to read one of the client's background processes: " + e.Message);
                }
            }
            yield return new WaitForSeconds(45f);
            StartCoroutine("ScanForIllegalPrograms");
        }
    }
}
