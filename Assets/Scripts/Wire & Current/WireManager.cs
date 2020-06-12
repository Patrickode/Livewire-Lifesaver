using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WireManager : MonoBehaviour
{
    /// <summary>
    /// A list of all the wires in the level. These are in sequential order once sorted.
    /// </summary>
    private static List<Wire> wireList = new List<Wire>();

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
            //First, find the start wire.
            Wire startWire = null;
            foreach (Wire wire in wireList)
            {
                bool isStartWire = true;

                foreach (Wire otherWire in wireList)
                {
                    //If otherWire precedes wire, then wire is not the start wire.
                    //We don't need to check any more wires if so.
                    if (otherWire != wire && GetNextWire(otherWire) == wire)
                    {
                        isStartWire = false;
                        break;
                    }
                }

                //If we got through the previous loop and isStartWire is still true, it is
                //actually the start wire.
                if (isStartWire)
                {
                    startWire = wire;
                }
            }

            //Now we'll travel the length of the wire and put each wire where it belongs along the way.
            List<Wire> wireListCopy = new List<Wire>(wireList);
            Wire currentWire = startWire;

            for (int i = 0; i < wireList.Count; i++)
            {
                //Set the current index to the current wire on the copied list, so we don't meddle with
                //the original.
                wireListCopy[i] = currentWire;
                //Now move onto the next wire by getting the next wire.
                currentWire = GetNextWire(currentWire);

                //GetNextWire should return null after all of the wires have been found. Warn the user if not.
                if (!currentWire && i < wireList.Count - 1)
                {
                    Debug.LogWarning("WireManager: GetNextWire returned null earlier than expected." +
                        " Make sure all of the wires are lined up correctly!" +
                        "\n(The blue axis should point in the direction you want the current to go.)");
                }
            }

            //Finally, now that the copy is properly sorted, set the wireList equal to it.
            wireList = wireListCopy;
        }
    }

    /// <summary>
    /// Get the wire that immediately follows a given wire.
    /// </summary>
    /// <param name="thisWire">The wire immediately before the one you want to get.</param>
    /// <returns>The wire immediately after the given wire.</returns>
    private static Wire GetNextWire(Wire thisWire)
    {
        //If thisWire is null, it obviously doesn't have a wire following it.
        if (!thisWire)
        {
            return null;
        }

        //If this wire isn't broken at its end, go through the wires to see if its end lines up with any other
        //wire's start.
        if (thisWire.type != WireType.BrokenEnd)
        {
            foreach (Wire otherWire in wireList)
            {
                //If thisWire's end overlaps with otherWire's start, then otherWire directly follows thisWire.
                if (thisWire.end.position == otherWire.start.position)
                {
                    return otherWire;
                }
            }
        }
        //If this wire IS broken at its end, go through the wires to see if its trigger lines up with another
        //wire's trigger.
        else
        {
            foreach (Wire otherWire in wireList)
            {
                //If thisWire and otherWire's triggers overlap, otherWire directly follows thisWire.
                //otherWire can only follow thisWire if it's broken at its start; broken wires come in pairs.
                if (otherWire.type == WireType.BrokenStart &&
                    thisWire.trigger.position == otherWire.trigger.position)
                {
                    return otherWire;
                }
            }
        }

        //If we got this far, no wire follows thisWire. Return null.
        return null;
    }
}