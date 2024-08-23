using System.Reflection;

namespace NotHarmony.DataObjects;

/// <summary>
/// Dude i didn't want to do it like this, but genericlly passing the pointers alone in the dynamicinvoke caused some sort of internal issue with the Runtime seeing diff param num, if you want you can test it ur self but this works for now man q-q
/// </summary>
public class NativeCallback {
    public IntPtr[] NativePointers { get; internal set; }   
    public ParameterInfo[] MethodParms { get; private set; }

    internal NativeCallback(ParameterInfo[] parms) => MethodParms = parms;
    internal void UpdatePointerData(params IntPtr[] @pointers) { // Method incase i need to add some like action shi
        NativePointers = @pointers; 
    }
}
