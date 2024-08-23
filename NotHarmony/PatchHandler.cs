using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;

using CoreRuntime.Manager;
using HexedTools.HookUtils;
using System.Runtime.InteropServices;
using NotHarmony.DataObjects;
using Il2CppInterop.Runtime.Runtime;


namespace WCv2.Components.WorldPatches;

// Made by _1254 ^w^ (at like 2am 7-7)
public class PatchHandler {
    public class PatchCallBackType { // LETTTS GOOOOO HARD CODING!!11 (Many params to make sure to fit most methods) <---- THIS MIGHT CAUSE ISSUES
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public unsafe delegate void STATIC(IntPtr _param, IntPtr _param2, IntPtr _param3, IntPtr _param4, IntPtr _param5, IntPtr _param6, IntPtr _param7, IntPtr _param8, IntPtr _param9, IntPtr _param10, IntPtr _param11, IntPtr _param12);
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public unsafe delegate void INSTANCE(IntPtr _instance, IntPtr _param, IntPtr _param2, IntPtr _param3, IntPtr _param4, IntPtr _param5, IntPtr _param6, IntPtr _param7, IntPtr _param8, IntPtr _param9, IntPtr _param10, IntPtr _param11, IntPtr _param12);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public unsafe delegate IntPtr STATIC_RETURN(IntPtr returnV, IntPtr _param, IntPtr _param2, IntPtr _param3, IntPtr _param4, IntPtr _param5, IntPtr _param6, IntPtr _param7, IntPtr _param8, IntPtr _param9, IntPtr _param10, IntPtr _param11, IntPtr _param12);
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public unsafe delegate IntPtr INSTANCE_RETURN(IntPtr returnV, IntPtr _instance, IntPtr _param, IntPtr _param2, IntPtr _param3, IntPtr _param4, IntPtr _param5, IntPtr _param6, IntPtr _param7, IntPtr _param8, IntPtr _param9, IntPtr _param10, IntPtr _param11, IntPtr _param12);
    }

    internal unsafe class PatchManager : List<DelegateCallback> {
        const int EXT_StructHideReturn = 111;

        internal bool isStatic => Target.IsStatic;
        internal bool hasReturn => Target.ReturnType != typeof(void);

        internal bool GeneralBlock { get; private set; }
        internal bool HasPost { get; private set; }


        MethodInfo Target;
        IntPtr TargetPointer;

        NativeCallback Callback;

        PatchCallBackType.INSTANCE _instanceCallback;
        PatchCallBackType.STATIC _staticCallback;

        PatchCallBackType.INSTANCE_RETURN _instanceReturnCallback;
        PatchCallBackType.STATIC_RETURN _staticReturnCallback;

        List<object> PARAMSTORAGE = [];
        ParameterInfo[] Parms;


        internal PatchManager(MethodInfo target) {
            Target = target;
            TargetPointer = *(IntPtr*)(IntPtr)InteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(target).GetValue(null);
            Parms = Target.GetParameters();
            Callback = new(Parms);

            //var originalNativeMethodInfo = UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)InteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(target).GetValue(null));
            //TargetPointer = originalNativeMethodInfo.MethodPointer;

            if (isStatic && !hasReturn)
                _staticCallback = HookManager.Detour<PatchCallBackType.STATIC>(TargetPointer, new((_param, _param2, _param3, _param4, _param5, _param6, _param7, _param8, _param9, _param10, _param11, _param12) => {
                    IntPtr _instance = 0;

                    if ((bool)ShouldCallback(ref _instance, ref _param, ref _param2, ref _param3, ref _param4, ref _param5, ref _param6, ref _param7, ref _param8, ref _param9, ref _param10, ref _param11, ref _param12)) {
                        _staticCallback(_param, _param2, _param3, _param4, _param5, _param6, _param7, _param8, _param9, _param10, _param11, _param12);
                        CallDelegates(true);
                    }
                }));
            else if (!isStatic && !hasReturn)
                _instanceCallback = HookManager.Detour<PatchCallBackType.INSTANCE>(TargetPointer, new((_instance, _param, _param2, _param3, _param4, _param5, _param6, _param7, _param8, _param9, _param10, _param11, _param12) => {
                    if ((bool)ShouldCallback(ref _instance, ref _param, ref _param2, ref _param3, ref _param4, ref _param5, ref _param6, ref _param7, ref _param8, ref _param9, ref _param10, ref _param11, ref _param12)) {
                        _instanceCallback(_instance, _param, _param2, _param3, _param4, _param5, _param6, _param7, _param8, _param9, _param10, _param11, _param12);
                        CallDelegates(true);
                    }
                }));
            else if (isStatic && hasReturn) {
                IntPtr _instance = 0;

                _staticReturnCallback = HookManager.Detour<PatchCallBackType.STATIC_RETURN>(TargetPointer, new((retv, _param, _param2, _param3, _param4, _param5, _param6, _param7, _param8, _param9, _param10, _param11, _param12) => {
                    var returnObject = ShouldCallback(ref _instance, ref _param, ref _param2, ref _param3, ref _param4, ref _param5, ref _param6, ref _param7, ref _param8, ref _param9, ref _param10, ref _param11, ref _param12);
                    IntPtr returnData = 0;

                    if (returnObject.GetType() == typeof(bool)) {
                        if ((bool)returnObject)
                            returnData = _staticReturnCallback(retv, _param, _param2, _param3, _param4, _param5, _param6, _param7, _param8, _param9, _param10, _param11, _param12);
                        else return 0;
                    } else returnData = GetObjectPointer(returnObject);

                    if (HasPost) CallDelegates(true);
                    return returnData;
                }));
            } else {
                _instanceReturnCallback = HookManager.Detour<PatchCallBackType.INSTANCE_RETURN>(TargetPointer, new((retv, _instance, _param, _param2, _param3, _param4, _param5, _param6, _param7, _param8, _param9, _param10, _param11, _param12) => {
                    var returnObject = ShouldCallback(ref _instance, ref _param, ref _param2, ref _param3, ref _param4, ref _param5, ref _param6, ref _param7, ref _param8, ref _param9, ref _param10, ref _param11, ref _param12);
                    IntPtr returnData = 0;

                    if (returnObject.GetType() == typeof(bool)) {
                        if ((bool)returnObject)
                            returnData = _instanceReturnCallback(retv, _instance, _param, _param2, _param3, _param4, _param5, _param6, _param7, _param8, _param9, _param10, _param11, _param12);
                        else return 0;
                    } else returnData = GetObjectPointer(returnObject);

                    if (HasPost) CallDelegates(true);
                    return returnData;
                }));
            }
        }

        internal PatchManager AddDetour(DelegateCallback call) {
            Add(call);
            GeneralBlock = Count == 0 || !this.Any(x => x.HasParams) || this.All(x => x.IsNative);
            HasPost = this.Any(x => x.PostPatch);
            return this;
        }

        internal PatchManager RemoveDetour(Delegate @delegate) {
            Remove(this.First(x => x.Delegate == @delegate));
            GeneralBlock = Count == 0 || !this.Any(x => x.HasParams) || this.All(x => x.IsNative);
            HasPost = this.Any(x => x.PostPatch);
            return this;
        }

        object[] skipObjects;
        object[] _invokeParams;
        IntPtr[] _invokeP;

        object ShouldCallback(ref IntPtr _instance, ref IntPtr _param1, ref IntPtr _param2, ref IntPtr _param3, ref IntPtr _param4, ref IntPtr _param5, ref IntPtr _param6, ref IntPtr _param7, ref IntPtr _param8, ref IntPtr _param9, ref IntPtr _param10, ref IntPtr _param11, ref IntPtr _param12) {
            PARAMSTORAGE.Clear();


            if (!GeneralBlock) { // incase someone just wants to block something and not parse types
                try {
                    if (!isStatic) PARAMSTORAGE.Add(GetTypeObject(_instance, Target.DeclaringType));
                    
                    for (int i = 0; i < Parms.Length; i++) {
                        var param = Parms[i];
                        if (param == null)
                            break;

                        var pointer = i switch {
                            0 => _param1, 1 => _param2,
                            2 => _param3, 3 => _param4,
                            4 => _param5, 5 => _param6, 
                            6 => _param7, 7 => _param8,
                            8 => _param9, 9 => _param10,
                            10 => _param11, 11 => _param12,
                            _ => throw new Exception("Params surpassed pointers?")
                        };
                        if (pointer == IntPtr.Zero) {
                            PARAMSTORAGE.Add(null);
                            continue;
                        }

                        if (param.ParameterType.IsSubclassOf(typeof(ValueType))) {
                            PARAMSTORAGE.Add(PrimitiveType(pointer, param.ParameterType));     
                        } else PARAMSTORAGE.Add(GetTypeObject(pointer, param.ParameterType));

                    }
                } catch (Exception e) {
                    Logs.Error("Error During Patch To Managed Type Conversion, breaking back to Il2cpp Runtime", e);
                    return true;
                }
            }

            skipObjects = null;
            _invokeParams = [.. PARAMSTORAGE];
            _invokeP = [_instance, _param1, _param2, _param3, _param4, _param5, _param6, _param7, _param8, _param9, _param10, _param11, _param12];

            // Call the methods patching this... (before callback to game) this will affect the _invokePointers, skipObjects, _invokeParams & _invokeP
            object returnData = CallDelegates();



            if (!GeneralBlock) {
                // i could for loop it, but tbh theres already a few here and i dont wanna add that as this is invoked each call for a patch q-q
                int offset = 0;
                if (!isStatic) 
                    TrySetAtIndex(offset++, ref _invokeP[0]);
                TrySetAtIndex(offset++, ref _invokeP[1]);
                TrySetAtIndex(offset++, ref _invokeP[2]);
                TrySetAtIndex(offset++, ref _invokeP[3]);
                TrySetAtIndex(offset++, ref _invokeP[4]);
                TrySetAtIndex(offset++, ref _invokeP[5]);
                TrySetAtIndex(offset++, ref _invokeP[6]);
                TrySetAtIndex(offset++, ref _invokeP[7]);
                TrySetAtIndex(offset++, ref _invokeP[8]);
                TrySetAtIndex(offset++, ref _invokeP[9]);
                TrySetAtIndex(offset++, ref _invokeP[10]);
                TrySetAtIndex(offset++, ref _invokeP[11]);
                TrySetAtIndex(offset++, ref _invokeP[12]);
            }

            // LETSS GOOO!!!!1111111!!1111 7-7
            _instance = _invokeP[0]; _param1 = _invokeP[1];
            _param2 = _invokeP[2]; _param3 = _invokeP[3];
            _param4 = _invokeP[4]; _param5 = _invokeP[5];
            _param6 = _invokeP[6]; _param7 = _invokeP[7];
            _param8 = _invokeP[8]; _param9 = _invokeP[9];
            _param10 = _invokeP[10]; _param11 = _invokeP[11];
            _param12 = _invokeP[12];


            return returnData;
            void TrySetAtIndex(int offset, ref IntPtr pointer) {
                if (offset < _invokeParams.Length) {
                    var objectPtr = GetObjectPointer(_invokeParams[offset]);
                    if (objectPtr != EXT_StructHideReturn) // eh shut it 
                        pointer = objectPtr;
                }
            }
        }

        object CallDelegates(bool post = false) {
            object returnData = true;

            foreach (var call in this) {
                try {
                    if (!post && call.PostPatch || post && !call.PostPatch)
                        continue;

                    object callData = null;
                    if (call.HasParams) {
                        if (call.IsNative) {
                            IntPtr[] listData = [.. (isStatic ? _invokeP?.Skip(1) : _invokeP).Take(Parms.Length + (isStatic ? 0 : 1))]; //Logs.Error($"{call.Delegate.Method.GetParameters().Length} - {listData.Length}");
                            Callback.UpdatePointerData(listData);
                            try {
                                callData = call.Delegate.DynamicInvoke(Callback);
                            } catch (Exception e) {
                                Logs.Error("Error During native callback", e);
                            }
                        } else callData = call.Delegate.DynamicInvoke(isStatic || call.ContainsInstance ? _invokeParams : (skipObjects = [.. _invokeParams?.Skip(1)]));
                    } else callData = call.Delegate.DynamicInvoke(); // No Param

                    if (!post) { // Don't care if the type data was modifed 
                        if (callData != null) {
                            var callDataType = callData.GetType();
                            if (callDataType == typeof(bool)) {
                                if (!(bool)callData)
                                    return false;
                            } else if (callDataType == Target.ReturnType || callDataType == typeof(ReturnBool))
                                returnData = callData;
                            else if (Target.ReturnType != typeof(void))
                                throw new Exception($"Unknown/ Non-Matching object Returned - {callData} ({callData.GetType()})");
                        }

                        if (skipObjects != null) { // Method without instance in instance call
                            for (int i = 0; i < skipObjects.Length; i++)
                                _invokeParams[i + 1] = skipObjects[i];
                            skipObjects = null;
                        }
                        if (Callback?.NativePointers?.Length > 0) { // Method All pointers non static 
                            for (int i = 0; i < Callback.NativePointers.Length; i++)
                                _invokeP[i + (isStatic ? 1 : 0)] = Callback.NativePointers[i];
                            Callback.NativePointers = null;
                        }
                    }
                    
                } catch (TargetParameterCountException r) {
                    Logs.Error($"[{call?.Delegate?.Method?.GetParameters().Length} - {_invokeParams.Length} ({skipObjects?.Length})]Error During Delgate Invoke Call Params {GetParamInfo(call?.Delegate?.Method?.GetParameters(), Name: call?.Delegate?.Method?.Name)}\n_invokeParams ({_invokeParams.Length}) [{string.Join(" ---- ", PARAMSTORAGE)}]", r);
                } catch (Exception e) {
                    Logs.Error($"Error During Patch at {call?.Delegate?.Method?.DeclaringType?.FullName}::{call?.Delegate?.Method?.Name}, Not Passing to Il2cpp Runtime {e?.InnerException?.ToString() ?? $"{this} - {e}"}"); // InnerException excludes the call infomation from this class, full error if no InnerException
                }
            }
            return returnData;
        }

        IntPtr GetObjectPointer(object data) {
            if (data == null) return 0; // likely null pointer in the first place

            var type = data.GetType();
            return type == typeof(ReturnBool) ? ((data as ReturnBool).Value ? 1 : 0) : // Else --- >
                type == typeof(string) ? (!string.IsNullOrEmpty((string)data) ? IL2CPP.ManagedStringToIl2Cpp((string)data) : 0) : // Else --- >
                type.IsValueType || type.IsEnum ? _valuePointer() : // Else --- > (Convert Twice = Extra)
                (data as Il2CppObjectBase).Pointer;

            IntPtr _valuePointer() => EXT_StructHideReturn; // just not supporting ref value types for now q-q
        }

        object GetTypeObject(IntPtr pointer, Type type) {
            if (type == typeof(string))
                return IL2CPP.Il2CppStringToManaged(pointer);
            return type.GetConstructor([typeof(IntPtr)]).Invoke([pointer]);
        }

        static object? PrimitiveType(IntPtr value, Type targetType) {
            if (targetType == typeof(object) || targetType == typeof(IntPtr))
                return value;

            Type? type = targetType.GetType();
            if (type.IsEnum) 
                type = Enum.GetUnderlyingType(type);

            TypeCode from = Type.GetTypeCode(targetType);
                switch (from) {// this needs to be converted better but this for now, before talking shit q-q https://github.com/dotnet/runtime/blob/main/src/mono/System.Private.CoreLib/src/System/RuntimeType.Mono.cs#L1864
                    case TypeCode.Byte: return (byte)value;
                    case TypeCode.SByte: return (sbyte)value;
                    case TypeCode.Char: return (char)value;
                    case TypeCode.UInt16: return (ushort)value;
                    case TypeCode.Int16: return (short)value;
                    case TypeCode.Int32: return (int)value;
                    case TypeCode.UInt32: return (uint)value;
                    case TypeCode.Int64: return (long)value;
                    case TypeCode.UInt64: return (ulong)value;
                    case TypeCode.Single: return (float)value;
                    case TypeCode.Double: return (double)value;
                    case TypeCode.Object: return value; // custom struct
                    case TypeCode.Boolean: return value == 1; // custom struct
                    default:
                        Logs.Log($"Unknown TypeCode {targetType.FullName}({value}) {Enum.GetName(typeof(TypeCode), from)}");
                        break;
                }


            if (value > Int32.MaxValue) return 0;
            return (Int64)((IntPtr)value);
        }


        public override string ToString() => $"(PatchManager) Target : {Target.DeclaringType}.{Target.Name} ({Target.ReturnType.FullName})";
    }

    static Dictionary<MethodInfo, PatchManager> Patches { get; set; } = [];

    // fuck it, since i have returning true or false if we should call back. Issue is if the call orginally had a bool return u wanna change, then just return these ( Return PatchHandler.True )
    public static ReturnBool True = new(true), False = new(false);

    public static void Detour(MethodInfo target, Delegate delg) => Detour(target, delg, false);

    public static void Detour(MethodInfo target, Delegate delg, bool PostPatch) {
        var detour = delg.Method;
        var detourParams = detour.GetParameters();

        if (detourParams.Length > 0 && !(detourParams.Length == 1 && detourParams.All(x=>x.ParameterType == typeof(NativeCallback)))) { // Dont need to param check if its using the native handle
            if ((target.GetParameters().Length + (target.IsStatic ? 0 : 1)) != detourParams.Length) // Note: can prob make it work with partial param patch with only the first few params 
                throw new Exception($"Detour Patch NEED matching Parameters! Target Params != Detour {GetParamInfo(target.GetParameters(), Name: target.Name)}");

            for (int i = 0; i < target.GetParameters().Length; i++) {
                var detourParam = detourParams[i + (target.IsStatic ? 0 : 1)].ParameterType; // account for instance param in non static patches
                if (detourParam == typeof(object) || detourParam == typeof(IntPtr)) continue; // no need to clamp generics

                if (detourParam.Name.Replace("&", "") != target.GetParameters()[i].ParameterType.Name) // q-q replace prob isn't best but we need to for refs, this check needs to be reworked anyways for non ref-able base types
                    throw new Exception($"Detour Patch NEED matching Parameters! ({detourParam.Name} != {target.GetParameters()[i].ParameterType.Name})");
            }
        }

        if (Patches.TryGetValue(target, out var _patches))
            _patches.AddDetour(new(delg, target));
        else Patches.Add(target, new PatchManager(target).AddDetour(new(delg, target, PostPatch)));
    }

    public static void HookGet(PropertyInfo Prop, Delegate delg, bool PostPatch = false) => Detour(Prop.GetGetMethod() ?? throw new NullReferenceException("This prop DOES NOT Hold a GetMethod"), delg, PostPatch);
    public static void HookSet(PropertyInfo Prop, Delegate delg, bool PostPatch = false) => Detour(Prop.GetSetMethod() ?? throw new NullReferenceException("This prop DOES NOT Hold a SetMethod"), delg, PostPatch);

    public static void Remove(MethodInfo target, Delegate delg) {
        if (Patches.TryGetValue(target, out var _patches))
            _patches.RemoveDetour(delg);
    }

    public static string GetParamInfo(ParameterInfo[] paramss, bool isInstance = false, Type instanceType = null, string Name = "DetourMethod") { 
        string info = Name + "(";
        if (isInstance)
            info += $"{instanceType.FullName} _instance, ";

        foreach (var param in paramss) {
            if (param.Position != 0) info += ", ";
            info += $"{param.ParameterType.FullName} {param.Name}";
        }
        info += ")";
        return info;
    }

    public class ReturnBool {
        public bool Value { get; private set; }
        internal ReturnBool(bool value) => Value = value;
    }
}