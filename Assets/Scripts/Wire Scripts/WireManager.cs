using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WireManager : MonoBehaviour
{
    /// <summary>
    /// A list of all the wires in the level. These are in sequential order.
    /// </summary>
    private readonly static List<Wire> wireList = new List<Wire>();

    /// <summary>
    /// Adds a wire to the wire list and puts it at the correct index.
    /// </summary>
    /// <param name="addedWire">The wire to add to the wire list.</param>
    public static void AddWire(Wire addedWire)
    {
        wireList.Add(addedWire);
    }

    /// <summary>
    /// Removes a given wire from the wire list.
    /// </summary>
    /// <param name="wireToRemove">The wire to remove from the list.</param>
    /// <returns>Whether the wire was removed successfully or not.</returns>
    public static bool RemoveWire(Wire wireToRemove)
    {
        return wireList.Remove(wireToRemove);
    }

    /// <summary>
    /// Gets a wire by its index in the list, or its order in the chain.
    /// </summary>
    /// <param name="wireIndex">The index of the wire to get.</param>
    /// <returns>The wire at the given index.</returns>
    public static Wire GetWire(int wireIndex)
    {
        return wireList[wireIndex];
    }

    /// <summary>
    /// Gets the number of wires in the wire list.
    /// </summary>
    /// <returns>The number of wires in the list.</returns>
    public static int GetCount()
    {
        return wireList.Count;
    }

    /// <summary>
    /// Sort the list of wires according to their sequence in the level.
    /// </summary>
    public static void SortWires()
    {
        if (wireList.Count > 1)
        {
            //First, find the start and end wires.
            Wire startWire = null;
            Wire endWire = null;
            foreach (Wire wire in wireList)
            {
                if (startWire && endWire)
                {
                    break;
                }

                //If startWire hasn't been assigned and this wire isn't the endWire, check
                //if it's the start wire.
                if (!startWire && endWire != wire)
                {
                    startWire = IsStartWire(wire) ? wire : null;
                }
                //Otherwise, if the end wire hasn't been assigned and this wire isn't the startWire, check
                //if it's the end wire.
                if (!endWire && startWire != wire)
                {
                    endWire = IsEndWire(wire) ? wire : null;
                }
            }

            //Next, actually put the start and end wires at the start and end of the list.
            if (wireList[0] != startWire)
            {
                SwapWires(0, wireList.IndexOf(startWire));
            }
            if (wireList[wireList.Count - 1] != endWire)
            {
                SwapWires(wireList.Count - 1, wireList.IndexOf(endWire));
            }

            //Finally, if there are four or more elements between the start and end, sort them.
            if (wireList.Count > 3)
            {
                //-1 means wire1 precedes wire2, and 1 means the opposite. 0 means do nothing.
                //Since the wires are in sequence, if a wire's end touches another's start, the former wire
                //precedes the latter.
                wireList.Sort
                (
                    (wire1, wire2) =>
                    {
                        //If one or both of the wires aren't broken, check their starts / ends, see if
                        //they overlap, and return accordingly.
                        if (!wire1.trigger || !wire2.trigger)
                        {
                            if (wire1.end.position == wire2.start.position)
                            {
                                return -1;
                            }
                            else if (wire1.start.position == wire2.end.position)
                            {
                                return 1;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        //If we got here, both are broken. That means we need to check their triggers,
                        //because there will be a gap between these wires even if they're in sequence.
                        else
                        {
                            //If the triggers of these wires correspondy,
                            if (wire1.trigger.position == wire2.trigger.position)
                            {
                                //Figure out which is the start and end, then return accordingly.
                                if (Vector3.Distance(wire1.trigger.position, wire1.end.position) <
                                Vector3.Distance(wire1.trigger.position, wire1.start.position))
                                {
                                    return -1;
                                }
                                else
                                {
                                    return 1;
                                }
                            }
                        }

                        //The lambda function thinks this code is reachable despite the else.
                        return 0;
                    }
                );
            }
        }
    }

    private static bool IsStartWire(Wire thisWire)
    {
        //If this wire is broken, and the broken end is the start of the wire, this can't be the start wire.
        if (thisWire.trigger &&
            Vector3.Distance(thisWire.trigger.position, thisWire.start.position) <
            Vector3.Distance(thisWire.trigger.position, thisWire.end.position))
        {
            return false;
        }

        //Check each wire to see if this wire's start is connected to another wire.
        foreach (Wire otherWire in wireList)
        {
            if (thisWire.start.position == otherWire.end.position)
            {
                return false;
            }
        }

        //If it's not, this is the starting wire.
        return true;
    }

    private static bool IsEndWire(Wire thisWire)
    {
        //If this wire is broken, and the broken end is the end of the wire, this can't be the end wire.
        if (thisWire.trigger &&
            Vector3.Distance(thisWire.trigger.position, thisWire.end.position) <
            Vector3.Distance(thisWire.trigger.position, thisWire.start.position))
        {
            return false;
        }

        //Check each wire to see if this wire's end is connected to another wire.
        foreach (Wire otherWire in wireList)
        {
            if (thisWire.end.position == otherWire.start.position)
            {
                return false;
            }
        }

        //If it's not, this is the starting wire.
        return true;
    }

    private static void SwapWires(int index1, int index2)
    {
        Wire swapper = wireList[index1];
        wireList[index1] = wireList[index2];
        wireList[index2] = swapper;
    }
}