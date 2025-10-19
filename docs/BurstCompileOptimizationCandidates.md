# BurstCompileAttribute Optimization Candidates

## Overview
This document provides a comprehensive analysis of source code candidates where `BurstCompileAttribute` optimization could be applied within the `src/Utils.Unity/Packages/jp.andantetribe.utils/Runtime` directory.

**Documentation Reference**: [Unity Burst API Documentation](https://docs.unity3d.com/Packages/com.unity.burst@1.8/api/Unity.Burst.BurstCompileAttribute.html)

**Status**: Currently, there are **NO existing implementations** of `BurstCompileAttribute` in the Runtime directory.

## What is Burst?
Burst is a compiler that translates C# Jobs and static methods into highly-optimized native code using LLVM. It's designed for:
- Unity Jobs System code (IJob, IJobParallelFor, etc.)
- Static methods with compatible types
- Math-heavy operations
- Performance-critical code paths

## Burst Compatibility Requirements
To be Burst-compatible, code must:
1. Use only blittable types or Unity mathematics types
2. Be static methods or implement IJob interfaces
3. Not use managed objects or references
4. Not use managed collections (use NativeArray, etc.)
5. Not call virtual methods or use reflection

## Analysis Methodology
All 28 C# files in the Runtime directory were analyzed for:
1. Use of Unity Jobs System (IJob, IJobParallelFor, etc.)
2. Math-heavy static methods with compatible types
3. Performance-critical extension methods
4. Struct-based value types suitable for Burst

## Current State: No Burst Implementation
**Finding**: The codebase does **NOT currently use** Unity's Burst compiler or Jobs System.

**Search Results**:
- No `using Unity.Burst` statements found
- No `[BurstCompile]` attributes found
- No `IJob`, `IJobParallelFor`, or `IJobFor` implementations found
- No Unity.Collections (NativeArray, etc.) usage found

## Potential Candidates for Future Burst Optimization

### Category 1: Mathematical Extension Methods (Highest Priority)

#### 1. VectorExtensions.cs
**File**: `Runtime/VectorExtensions.cs`  
**Current Status**: Uses `[MethodImpl(MethodImplOptions.AggressiveInlining)]` for optimization  
**Description**: Vector component extraction methods

**Methods**:
- `XZ(in this Vector3 target)` - Extracts X and Z components to Vector2
- `YZ(in this Vector3 target)` - Extracts Y and Z components to Vector2

**Burst Applicability**: 
- ⚠️ **Limited** - These are extension methods on Unity types
- Currently optimized with AggressiveInlining
- **Cannot directly apply BurstCompile** to extension methods
- **Alternative**: Could be refactored as static utility methods in a Burst-compiled static class if performance is critical

**Code Example**:
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static Vector2 XZ(in this Vector3 target) => new Vector2(target.x, target.z);
```

---

#### 2. TransformExtensions.cs
**File**: `Runtime/TransformExtensions.cs`  
**Current Status**: Uses `[MethodImpl(MethodImplOptions.AggressiveInlining)]`  
**Description**: Transform position manipulation methods

**Methods** (9 total):
- `SetX/SetY/SetZ` - Set individual position components
- `AddX/AddY/AddZ` - Add to individual position components
- `RemoveX/RemoveY/RemoveZ` - Subtract from individual position components

**Burst Applicability**:
- ❌ **Not Applicable** - Extension methods on Transform (managed Unity object)
- Uses Transform.position getter/setter which are not Burst-compatible
- **Cannot apply BurstCompile** - relies on managed object API calls

**Reason**: Transform is a managed Unity component and cannot be used in Burst-compiled code.

---

#### 3. RectTransformExtensions.cs
**File**: `Runtime/RectTransformExtensions.cs`  
**Current Status**: Uses `[MethodImpl(MethodImplOptions.AggressiveInlining)]`  
**Description**: RectTransform manipulation utilities

**Methods**:
- `SetSize` (2 overloads) - Set RectTransform size
- `SetWidth/SetHeight` - Set individual dimensions
- `GetSize` - Get RectTransform size
- `SetFullStretch` - Configure anchors for full stretch
- `GetWorldCorners` - Calculate world space corners with Span allocation
- `GetLocalCorners` - Calculate local space corners with Span allocation
- `GetCalculateLocalCorners` - Internal corner calculation

**Burst Applicability**:
- ❌ **Not Applicable** - Extension methods on RectTransform (managed Unity object)
- Uses managed Unity API calls
- Corner calculation methods (`GetCalculateLocalCorners`) could potentially be extracted as pure math functions
- **Cannot apply BurstCompile** directly

**Potential Refactoring**:
- The `GetCalculateLocalCorners` method has pure math logic that could be extracted:
```csharp
// Current code (line 313-318)
internal static void GetCalculateLocalCorners(this RectTransform rectTransform, in Span<Vector3> fourCornersSpan)
{
    var rect = rectTransform.rect;
    var (x, y, xMax, yMax) = (rect.x, rect.y, rect.xMax, rect.yMax);
    stackalloc Vector3[] { new(x, y), new(x, yMax), new(xMax, yMax), new(xMax, y) }.TryCopyTo(fourCornersSpan);
}
```
- Could be refactored to separate the math from the Unity object access

---

### Category 2: Component and GameObject Utilities (Not Applicable)

#### 4. ComponentExtensions.cs
**File**: `Runtime/ComponentExtensions.cs`  
**Burst Applicability**: ❌ **Not Applicable**
- Extension methods on Component (managed Unity object)
- Uses GameObject.TryGetComponent and transform operations
- Cannot be Burst-compiled due to managed object dependencies

---

### Category 3: Readonly Structs (Potential Future Candidates)

#### 5. AsyncSemaphore.Handle
**File**: `Runtime/Tasks/AsyncSemaphore.cs`  
**Structure**: `public readonly struct Handle : IEquatable<Handle>, IDisposable`

**Burst Applicability**: ⚠️ **Limited**
- Readonly struct is Burst-compatible
- However, it's a handle for async operations (not computation-heavy)
- Implements IDisposable which is typically not Burst-compatible
- **Not a priority candidate** - no computational benefit from Burst

---

#### 6. GameObjectPool<T>.Handle
**File**: `Runtime/Tasks/GameObjectPool.cs`  
**Structure**: `public readonly struct Handle : IDisposable`

**Burst Applicability**: ❌ **Not Applicable**
- Struct for GameObject pooling (managed objects)
- IDisposable pattern for resource management
- Not suitable for Burst compilation

---

#### 7. UIBlocker.Handle
**File**: `Runtime/UGUI/UIBlocker.cs`  
**Structure**: `public readonly struct Handle : IDisposable`

**Burst Applicability**: ❌ **Not Applicable**
- UI blocking handle (managed UI objects)
- Not suitable for Burst compilation

---

### Category 4: Other Files (Not Applicable)

The following files were analyzed and found to be incompatible with Burst:

- **Addressable/*** - All addressable asset management classes (managed objects)
- **ButtonAttribute.cs** - Attribute class (not executable code)
- **Initializer.cs** - Unity initialization code (managed)
- **ObjectReference.cs** - Object reference management (managed)
- **R3/ObservableExtensions.Debug.cs** - Reactive Extensions (managed)
- **SafeAreaAdjuster.cs** - MonoBehaviour component (managed)
- **SafeAreaContainer.cs** - VisualElement component (managed)
- **Tasks/TapEffect.cs** - UI effect class (managed)
- **Tasks/UnityWebRequestHttpMessageHandler.cs** - HTTP handler (managed)
- **UGUI/*** - All UI graphics classes (MonoBehaviour/Graphic derived, managed)
- **UnityCustomSpanFormatter.cs** - String formatting (managed)
- **VContainer/*** - Dependency injection classes (managed)

---

## Summary and Recommendations

### Current State
- **Zero** files currently use BurstCompileAttribute
- **Zero** files implement Unity Jobs System
- **Zero** files use Unity.Collections

### Viable Candidates for Burst: NONE (As-Is)

**Key Finding**: The current codebase architecture is **not designed for Burst compilation**. All identified code uses:
- Extension methods on managed Unity objects
- MonoBehaviour components
- Managed object references
- Unity's managed API calls

### Why No Viable Candidates?

1. **Extension Methods**: Most utility code is written as extension methods on Unity types (Transform, RectTransform, Component). Burst cannot compile extension methods that operate on managed objects.

2. **No Jobs System**: There are no IJob implementations or parallel processing patterns that would benefit from Burst.

3. **Managed API Dependencies**: All code relies on Unity's managed API calls (transform.position, GetComponent, etc.) which are not Burst-compatible.

4. **Architecture Pattern**: The codebase uses a convenience/utility pattern rather than a performance-critical computation pattern.

### Recommendations for Future Burst Implementation

If Burst optimization becomes a priority, consider:

#### Option 1: Refactor Math-Heavy Code
Extract pure mathematical operations from extension methods into static utility classes:
```csharp
// Example: Create a new BurstMath utility class
[BurstCompile]
public static class BurstMath
{
    [BurstCompile]
    public static Vector2 ExtractXZ(float x, float z) => new Vector2(x, z);
    
    [BurstCompile]
    public static void CalculateRectCorners(
        float x, float y, float xMax, float yMax,
        NativeArray<Vector3> corners)
    {
        corners[0] = new Vector3(x, y);
        corners[1] = new Vector3(x, yMax);
        corners[2] = new Vector3(xMax, yMax);
        corners[3] = new Vector3(xMax, y);
    }
}
```

#### Option 2: Implement Jobs for Batch Operations
If there's a need to process many transforms/vectors in parallel:
```csharp
[BurstCompile]
public struct TransformPositionJob : IJobParallelFor
{
    public NativeArray<Vector3> positions;
    public float deltaX;
    
    public void Execute(int index)
    {
        var pos = positions[index];
        pos.x += deltaX;
        positions[index] = pos;
    }
}
```

#### Option 3: Add Dependencies
To enable Burst, add to package.json dependencies:
```json
"dependencies": {
    "com.unity.burst": "1.8.0",
    "com.unity.collections": "2.1.0",
    "com.unity.mathematics": "1.2.0"
}
```

### Performance Note
The current use of `[MethodImpl(MethodImplOptions.AggressiveInlining)]` already provides significant optimization for simple mathematical operations. The incremental benefit of Burst would be minimal unless:
1. Processing large batches of data (thousands of operations)
2. Implementing parallel algorithms
3. Performing complex mathematical computations

---

## Conclusion

**Final Answer**: There are **ZERO viable candidates** for BurstCompileAttribute optimization in the current codebase as-is.

The utility library is designed for convenience and ease-of-use with Unity's managed object model, not for high-performance parallel computation. Burst optimization would require significant architectural changes to:
1. Separate pure computation from managed object access
2. Implement Unity Jobs System patterns
3. Use NativeContainers instead of managed collections
4. Refactor extension methods into static utility classes

Unless there's a specific performance bottleneck identified through profiling, the current optimization strategy using `AggressiveInlining` is appropriate for this type of utility library.

---

## File Inventory
Total files analyzed: **28**
- Extension method utilities: **4** (VectorExtensions, TransformExtensions, RectTransformExtensions, ComponentExtensions)
- MonoBehaviour/Component classes: **8**
- UI/UGUI classes: **6**
- Addressable/Asset management: **5**
- Container/Pool utilities: **3**
- Other utilities: **2**

**Date**: 2025-10-19  
**Analyzed By**: Automated code analysis
