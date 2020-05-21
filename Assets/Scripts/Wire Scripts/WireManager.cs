using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WireManager : MonoBehaviour
{
    /// <summary>
    /// An array of all the wires in the level. These are in sequential order.
    /// </summary>
    public static List<Wire> wireList = new List<Wire>();
}