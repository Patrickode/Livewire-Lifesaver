using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BindingOverride : IEquatable<BindingOverride>
{
    public int ActionIndex { get; private set; }
    public int BindingIndex { get; private set; }
    public string Path { get; private set; }

    public BindingOverride(int actionIndex, int bindingIndex, string path)
    {
        this.ActionIndex = actionIndex;
        this.BindingIndex = bindingIndex;
        this.Path = path;
    }

    //Equatable Overrides
    //See https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.except?view=netcore-3.1#code-try-2
    //for what I modeled these after.
    public bool Equals(BindingOverride other)
    {
        if (other is null) { return false; }

        return ActionIndex == other.ActionIndex
            && BindingIndex == other.BindingIndex
            && Path == other.Path;
    }

    public override bool Equals(object obj) { return base.Equals(obj as BindingOverride); }
    public override int GetHashCode() { return (ActionIndex, BindingIndex, Path).GetHashCode(); }
}

[Serializable]
public class SaveData
{
    /// <summary>
    /// The last level the player played. Determines what levels
    /// </summary>
    public int LastCompletedIndex { get; private set; }
    public int[] CollectedBoltIndices { get; private set; }
    public BindingOverride[] BindingOverrides { get; private set; }
    //public (int actionIndex, int bindingIndex, string path)[] BindingOverrides { get; private set; }

    public SaveData(int lastCompletedIndex, int[] collectedBoltIndices, BindingOverride[] bindingOverrides)
    {
        LastCompletedIndex = lastCompletedIndex;
        CollectedBoltIndices = collectedBoltIndices;
        BindingOverrides = bindingOverrides;
    }
    public SaveData() : this(0, new int[] { }, new BindingOverride[] { }) { }
}