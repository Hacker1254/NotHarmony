# Ngl i give up man someone else should fix this
tbh its prob simple i just don't know, ValueTypes just are not supported and it just is some other value
also some stuff just doesn't patch
and it needs some qol patch/ bug fix stuff


# Incomplete!!

This is a tool i made for use in HEXEDWARE but honestly can be repurposed 

This allows you to hook onto (Currently ONLY) generated Il2Cpp methods, and Props
and lets the same method be hooked onto.

Supported - 
* Auto Type conversion For Params
* Patching Return Types
* Ref/ In-out for Il2CppBaseObjects
* Native IntPtr methods


Upcomming - 
* BaseType Params (Non ref-able)
* DirectIntPtr Params
* Ref/ In-out for Struct Base
* Dynamic Method Param invoke (Like skip params)





-------------
# Usage

`class ExampleClass { void MethodCall(string str, ExampleType param2) }` <-- Example Method

`void MethodCall(ExampleClass instance, string str, ExampleType param2)` <-- Example Delg

If the Target IS NOT static, First Param NEEDS to be the instance + Detour Target (delg) params NEEDS to match the Target  (Exception is thrown if not matching) 

Returns - 
* Returning NULL continues
* Returning a Boolen decides if the method should be invoked (False stops the call)
* <-------------------------- Return Type Methods -------------------------->
* Returning PatchHandler.True or PatchHandler.False Returns True or False for the Methods return value
* Returning the same ClassType Changes the Methods ReturnValue to the Type


### Methods
--
`PatchHandler.Detour` <-- MethodInfo target, Delegate delg

```c#
// Normal NONSTATIC Useage
// Notes: 
//      Target is void NonStaticMethodThreeParam(string str, int num, float val)
PatchHandler.Detour(typeof(Class).GetMethod(nameof(Class.NonStaticMethodThreeParam)), MethodPatch);

static bool MethodPatch(Class _instance, ref string str, int num, float val){
     str = "<color=red>str</color>";
     return true; // Don't block call
     return false; // Block Call
     return null; // Don't Block call/ continue normally 
}

// Normal STATIC Useage
// Notes: 
//      only diff from before is the first param
static bool MethodPatch(ref string str, int num, float val)...

// Normal BOOL Custom Return Value Useage
// Notes: 
//      Target is bool NonStaticMethodOneParam(string str)
PatchHandler.Detour(typeof(Class).GetMethod(nameof(Class.NonStaticMethodOneParam)), MethodPatch);

static object MethodPatch(Class _instance, ref string str){ // needs to be object
     if (str.Contains("keyWord")) return PatchHandler.False; // Makes the method return bool false
     return null; // Don't Block call/ continue normally 
     return true; // Don't block call
     return false; // Block Call
}

// Normal Custom Return Value Useage (Not Tested, Might be borked)
// Notes: 
//      Target is int StaticMethodOneParam(string str)
PatchHandler.Detour(typeof(Class).GetMethod(nameof(Class.StaticMethodOneParam)), MethodPatch);

static object MethodPatch(ref string str){ // needs to be object
     if (str.Contains("keyWord")) return 124; // The method return is an Int 
     return null; // Don't Block call/ continue normally 
     return true; // Don't block call
     return false; // Block Call
}

// Lambd Example
// Notes: 
//      !!! THIS APPLYS FOR PROPS ASWELL -> If a Delegate delg has no Params, its also valid, Same return Rules
PatchHandler.Detour(typeof(Class).GetMethod(nameof(Class.NonStaticMethodNoParam)), () => !Settings.BoolOpt);

// Lambd Example 2
// Notes: 
//      normal useage with param
PatchHandler.Detour(typeof(Class).GetMethod(nameof(Class.NonStaticMethodNoParam)), 
        (Class _instance) => !Settings.BoolOpt);

// Lambd Example 3
// Notes: 
//      Target is NonStaticMethodThreeParam(string str, int num, float val)
PatchHandler.Detour(typeof(Class).GetMethod(nameof(Class.NonStaticMethodThreeParam)), 
        (Class _instance, string str, int num, float val) => {
              return !Settings.BoolOpt
});
```

### Properties
`PatchHandler.HookGet || HookSet` <-- PropertyInfo target, Delegate delg

This just calls back to Method `PatchHandler.Detour`
For get, its `PatchHandler.HookGet` and is treated as a 0Param NONSTATIC method with a return value

```c#

#region Set 
// Lambd Example
// Notes: 
//   We can one line this because the Get returns void so the PatchHandler Ignores the return value
PatchHandler.HookSet(typeof(TMP_Text).GetProperty(nameof(TMP_Text.text)),
        (TMP_Text Instance, ref string Value) => Value = EditText(Value));

// Lambd Example 2
// Notes: 
//   
PatchHandler.HookSet(typeof(TMP_Text).GetProperty(nameof(TMP_Text.text)),
        (TMP_Text Instance, ref string Value) => {
              Value = EditText(Value);
});

// Normal
// Notes: 
//   delgate methods must be static, Void if you just want to mess with calls, bool if you want to block them
//        OR object if you want to do Both 
PatchHandler.HookSet(typeof(TMP_Text).GetProperty(nameof(TMP_Text.text)), TextEdit);

static object TextEdit(TMP_Text Instance, ref string Value) {
     Value = EditText(Value );

     return true; // Don't block call
     return false; // Block Call
     return null; // Don't Block call
}
#endregion



```