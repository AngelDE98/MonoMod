# Using MonoMod.Patcher
Drop `MonoMod.exe`, all dependencies (Utils, cecil) and your patches into the game directory. Then, in your favorite shell (cmd, bash):

    MonoMod.exe Assembly.exe

MonoMod scans the directory for files named `[Assembly].*.mm.dll` and generates `MONOMODDED_[Assembly].exe`, which is the patched version of the assembly.

## Example Patch

You've got `Celeste.exe` and want to patch the method `public override void Celeste.Player.Added(Scene scene)`.

If you haven't created a mod project yet, create a shared C# library project called `Celeste.ModNameHere.mm`, targeting the same framework as `Celeste.exe`.  
Add `Celeste.exe`, `MonoMod.exe` and all dependencies (.Utils, cecil) as assembly references.  
*Note:* Make sure to set "Copy Local" to `False` on the game's assemblies. Otherwise your patch will ship with a copy of the game!
 
```cs
#pragma warning disable CS0626 // orig_ method is marked external and has no attributes on it.
namespace Celeste {
    // The patch_ class is in the same namespace as the original class.
    // This can be bypassed by placing it anywhere else and using [MonoModPatch("global::Celeste.Player")]

    // Visibility defaults to "internal", which hides your patch from runtime mods.
    // If you want to "expose" new members to runtime mods, create extension methods in a public static class PlayerExt
    class patch_Player : Player { // : Player lets us reuse any of its visible members without redefining them.
        // MonoMod creates a copy of the original method, called orig_Added.
        public extern void orig_Added(Scene scene);
        public override void Added(Scene scene) {
            // Do anything before.

            // Feel free to modify the parameters.
            // You can even replace the method's code entirely by ignoring the orig_ method.
            orig_Added(scene);
            
            // Do anything afterwards.
        }
    }
}
```

Build `Celeste.ModNameHere.mm.dll`, copy it into the game's directory and run `MonoMod.exe Celeste.exe`, which generates `MONOMODDED_Celeste.exe`.  
*Note:* This can be automated by a post-build step in your IDE and integrated in an installer, f.e. [Everest.Installer (GUI)](https://github.com/EverestAPI/Everest.Installer), [MiniInstaller (CLI)](https://github.com/EverestAPI/Everest/blob/master/MiniInstaller/Program.cs) or [PartialityLauncher (GUI)](https://github.com/PartialityModding/PartialityLauncher).

To make patching easy, yet flexible, the MonoMod patcher offers a few extra features:

- `MonoMod.MonoModRules` will be executed at patch time. Your rules can define relink maps (relinking methods, fields or complete assemblies), change the patch behavior per platform or [define custom modifiers](MonoMod/Modifiers/MonoModCustomAttribute.cs) to f.e. [modify a method on IL-level using cecil.](https://github.com/MonoMod/MonoMod/issues/15#issuecomment-344570625)
- For types and methods, to be ignored by MonoMod (because of a superclass that is defined in another assembly and thus shouldn't be patched), use the `MonoModIgnore` attribute.
- [Check the full list of standard modifiers with their descriptions](MonoMod/Modifiers), including "patch-time hooks", proxies via `[MonoModLinkTo]` and `[MonoModLinkFrom]`, conditinal patching via `[MonoModIfFlag]` + MonoModRules, and a few more. 

----

# FAQ

## How does the patcher work?
- MonoMod first checks for a `MonoMod.MonoModRules` type in your patch assembly, isolates it and executes the code.
- It then copies any new types, including nested types, except for patch types and ignored types.
- Afterwards, it copies each type member and patches the methods. Make sure to use `[MonoModIgnore]` on anything you don't want to change.
- Finally, all relink mappings get applied and all references get fixed (method calls, field get / set operations, ...).


## How can I check if my assembly has been modded?
MonoMod creates a type called "WasHere" in the namespace "MonoMod" in your assembly:

```cs
if (Assembly.GetExecutingAssembly().GetType("MonoMod.WasHere") != null) {
    Console.WriteLine("MonoMod was here!");
} else {
    Console.WriteLine("Everything fine, move on.");
}
```

*Note:* This can be easily bypassed. More importantly, it doesn't detect changes made using other tools like dnSpy.  
If you're a gamedev worried about cheating: Please don't fight against the modding community. Cheaters will find another way to cheat, and modders love to work together with gamedevs.


## Am I allowed to redistribute the patched assembly?
This depends on the licensing situation of the input assemblies. If you're not sure, ask the authors of the patch and of the game / program / library.


## Is it possible to use multiple patches?
Yes, as long as the patches don't affect the same regions of code.

While possible, its behaviour is not strictly defined and depends on the patching order.  
Instead, please use runtime detours / hooks instead, as those were built with "multiple mod support" in mind.
