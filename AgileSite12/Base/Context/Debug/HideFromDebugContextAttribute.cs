using System;

namespace CMS.Base
{
    /// <summary>
    /// Marks a specific member or class to be excluded from debug context information
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class HideFromDebugContextAttribute : Attribute
    {
    }
}