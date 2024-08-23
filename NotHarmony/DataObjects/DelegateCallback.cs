using System.Reflection;

namespace NotHarmony.DataObjects;

internal class DelegateCallback {
    internal MethodInfo Target { get; private set; }

    internal bool HasParams { get; private set; }
    internal bool IsNative { get; private set; }
    internal bool ContainsInstance { get; private set; }
    internal bool PostPatch { get; private set; }

    internal Delegate Delegate { get; }

    internal DelegateCallback(Delegate @delegate, MethodInfo target, bool postPatch = false) {
        Delegate = @delegate;
        Target = target;

        var detourParams = @delegate.Method.GetParameters();
        HasParams = detourParams.Length > 0;
        if (HasParams) {
            IsNative = detourParams.Length == 1 && detourParams.All(x => x.ParameterType == typeof(NativeCallback));
            if (!IsNative)
                ContainsInstance = !target.IsStatic && (detourParams[0].ParameterType == typeof(object) || detourParams[0].ParameterType.Name.Replace("&", "") == target.DeclaringType.Name);
        }

        PostPatch = postPatch;
    }
}
