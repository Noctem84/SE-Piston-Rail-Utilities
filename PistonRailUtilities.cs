using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class PistonRailUtilities
        {
            public static IEnumerator<bool> waitForPistonsToLimit(List<IMyPistonBase> pistons, int newStateIndex, bool isMax, IMovable movable, DisplayService displayService)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("check wait condition").Append("\n");
                while (movable.getRunState() != RunState.Stopped && !isLimitReached(pistons, isMax, stringBuilder))
                {
                    stringBuilder.Append("waitForPiston...").Append("\n");
                    displayService.writeToDisplays(stringBuilder, false, 3);
                    stringBuilder.Clear();
                    stringBuilder.Append("check wait condition").Append("\n");
                    yield return true;
                }
                if (newStateIndex != -1 && movable.getRunState() != RunState.Stopped)
                {
                    stringBuilder.Append("Setting new index to ").Append(newStateIndex).Append(" for ").Append(movable.getRunState()).Append("\n");
                    movable.getStates()[movable.getRunState()].Index = newStateIndex;
                }
                stringBuilder.Append("waitForPiston... done").Append("\n");
                displayService.writeToDisplays(stringBuilder, false, 3);
            }

            public static IEnumerator<bool> waitForRotorPosition(IMyMotorStator rotor, float position, int newStateIndex, IMovable movable, DisplayService displayService)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("check wait condition").Append("\n");
                while (movable.getRunState() != RunState.Stopped && rotor.Angle <= position + 0.01f && position >= position - 0.01f)
                {
                    stringBuilder.Append("angel " + (position - 0.1f) + " >= " + rotor.Angle + " <= " + (position + 0.1f)).Append("\n");
                    stringBuilder.Append("waitForRotor...").Append("\n");
                    displayService.writeToDisplays(stringBuilder, false, 3);
                    stringBuilder.Clear();
                    stringBuilder.Append("check wait condition").Append("\n");
                    yield return true;
                }
                rotor.SetValueFloat("Velocity", 0);
                if (newStateIndex != -1 && movable.getRunState() != RunState.Stopped)
                {
                    movable.getStates()[movable.getRunState()].Index = newStateIndex;
                }
                stringBuilder.Append("waitForRotor... done").Append("\n");
                displayService.writeToDisplays(stringBuilder, false, 3);
            }

            public static IEnumerator<bool> waitForConnectionState(Lock shipLock, bool state, int newStateIndex, IMovable movable, DisplayService displayService)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("check wait condition").Append("\n");
                while (movable.getRunState() != RunState.Stopped && !isLockInState(shipLock, state, state))
                {
                    stringBuilder.Append("wait for lock state " + state + "; " + shipLock.mergeBlock.DisplayName + " is " + (shipLock.mergeBlock.IsConnected ? "" : "dis") + "connected and " + (shipLock.mergeBlock.Enabled ? "enabeld" : "disabeled") + " and connector is " + shipLock.connector.Status).Append("\n");
                    displayService.writeToDisplays(stringBuilder, false, 3);
                    stringBuilder.Clear();
                    stringBuilder.Append("check wait condition").Append("\n");
                    yield return true;
                }
                if (newStateIndex != -1 && movable.getRunState() != RunState.Stopped)
                {
                    movable.getStates()[movable.getRunState()].Index = newStateIndex;
                }
                displayService.writeToDisplays(stringBuilder, false, 3);
            }
            public static void extendPiston(List<IMyPistonBase> pistons, float velocity)
            {
                foreach (var piston in pistons)
                {
                    piston.Velocity = velocity;
                }
            }
            public static void resetPiston(List<IMyPistonBase> pistons)
            {
                foreach (var piston in pistons)
                {
                    piston.Velocity = 0;
                }
            }

            public static void retractPiston(List<IMyPistonBase> pistons, float velocity)
            {
                extendPiston(pistons, -1*velocity);
            }
            public static bool setLockStatus(Lock shipLock, bool status, StringBuilder stringBuilder)
            {
                if (status && shipLock.connector.Status == MyShipConnectorStatus.Connectable)
                {
                    stringBuilder.Append("connecting connector").Append("\n");
                    shipLock.mergeBlock.Enabled = status;
                    shipLock.connector.Connect();
                    return true;
                }
                else if (status && shipLock.connector.Status == MyShipConnectorStatus.Connected)
                {
                    stringBuilder.Append("connector already connected").Append("\n");
                    shipLock.mergeBlock.Enabled = status;
                    return true;
                }
                else if (!status)
                {
                    stringBuilder.Append("connector disconnected").Append("\n");
                    shipLock.mergeBlock.Enabled = status;
                    shipLock.connector.Disconnect();
                    return true;
                }
                shipLock.mergeBlock.Enabled = status;
                return false;
            }
            public static bool isLockInState(Lock shipLock, bool state, bool autoConnect)
            {
                if (state && autoConnect && shipLock.connector.Status == MyShipConnectorStatus.Connectable)
                {
                    shipLock.connector.Connect();
                }
                if (state && !shipLock.mergeBlock.IsConnected && shipLock.connector.Status == MyShipConnectorStatus.Connected)
                {
                    shipLock.connector.Disconnect();
                }
                if (state && shipLock.mergeBlock.IsConnected && shipLock.connector.Status == MyShipConnectorStatus.Connected)
                {
                    return true;
                }
                if (!state && !shipLock.mergeBlock.IsConnected && shipLock.connector.Status != MyShipConnectorStatus.Connected)
                {
                    return true;
                }
                return false;
            }
            public static int getBuildableBlockCount(List<IMyProjector> projectors)
            {
                int blockCount = 0;
                foreach (var project in projectors)
                {
                    blockCount += project.BuildableBlocksCount;
                }
                return blockCount;
            }
            public static bool isLimitReached(List<IMyPistonBase> pistons, bool isMax, StringBuilder stringBuilder)
            {
                if (stringBuilder != null)
                {
                    stringBuilder.Append("Check limit " + (isMax ? "max" : "min")).Append("\n");
                }
                foreach (var piston in pistons)
                {
                    if (stringBuilder != null)
                    {
                        stringBuilder.Append("Piston " + piston.DisplayName).Append(piston.CustomName).Append("\n");
                        stringBuilder.Append("Max: " + piston.MaxLimit + " Min: " + piston.MinLimit).Append("\n");
                        stringBuilder.Append("Waiting for piston\n" + piston.DisplayName + " Position: " + piston.CurrentPosition + "\nTarget: " + (isMax ? "> " + (piston.MaxLimit - (piston.MaxLimit / 500)) : "< " + (piston.MinLimit == 0 ? 0 : piston.MinLimit + (piston.MinLimit / 500)))).Append("\n");
                    }
                    if (isMax)
                    {
                        if ((piston.MaxLimit - (piston.MaxLimit / 500)) > piston.CurrentPosition)
                        {
                            if (stringBuilder != null)
                            {
                                stringBuilder.Append((piston.MaxLimit - (piston.MaxLimit / 500)) + " is greater than " + piston.CurrentPosition + ": " + ((piston.MaxLimit - (piston.MaxLimit / 500)) > piston.CurrentPosition)).Append("\n");
                            }
                            return false;
                        }
                    }
                    else
                    {
                        if (piston.CurrentPosition > (piston.MinLimit == 0 ? 0 : piston.MinLimit + (piston.MinLimit / 500)))
                        {
                            return false;
                        }
                    }
                    /*
                    if (piston.CurrentPosition != (isMax ? piston.MaxLimit : piston.MinLimit))
                    {
                        return false;
                    }
                    */
                }
                return true;
            }
            public static float getPistonLimitTotal(List<IMyPistonBase> pistonsDrills)
            {
                float limit = 0;
                foreach (var piston in pistonsDrills)
                {
                    limit += piston.MaxLimit;
                }
                return limit;
            }
            public static bool isPistonMaxLimit(List<IMyPistonBase> pistons)
            {
                foreach (var piston in pistons)
                {
                    if (piston.MaxLimit != 10f)
                    {
                        return false;
                    }
                }
                return true;
            }

            internal static void setMaxLimit(List<IMyPistonBase> pistons, float v)
            {
                foreach (var piston in pistons)
                {
                    piston.MaxLimit = v;
                }
            }
            internal static void setMinLimit(List<IMyPistonBase> pistons, float v)
            {
                foreach (var piston in pistons)
                {
                    piston.MinLimit = v;
                }
            }
        }
    }
}
