using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libManagedNetstat.a", LinkTarget.ArmV7 | LinkTarget.Simulator, ForceLoad = true)]
