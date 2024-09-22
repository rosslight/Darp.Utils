﻿// <auto-generated/>

#nullable enable
using System.Reflection;

namespace TestProject.Second
{
    /// <summary>A strongly typed resource class for '/0/Second/Resources.resx'</summary>
    internal sealed partial class Resources
    {
        private static Resources? _default;
        /// <summary>The Default implementation of <see cref="Resources"/></summary>
        public static Resources Default => _default ??= new Resources();

        public delegate void CultureUpdateDelegate(global::System.Globalization.CultureInfo? oldCulture, global::System.Globalization.CultureInfo? newCulture);
        /// <summary>Called after the <see cref="Culture"/> was updated. Provides previous culture and the newly set culture</summary>
        public event CultureUpdateDelegate? CultureUpdated;

        private global::System.Globalization.CultureInfo? _culture;
        /// <summary>Get or set the Culture to be used for all resource lookups issued by this strongly typed resource class.</summary>
        public System.Globalization.CultureInfo? Culture
        {
            get => _culture;
            set
            {
                System.Globalization.CultureInfo? oldCulture = _culture;
                _culture = value;
                if (!System.Collections.Generic.EqualityComparer<System.Globalization.CultureInfo>.Default.Equals(oldCulture, value))
                    CultureUpdated?.Invoke(oldCulture, value);
            }
        }

        ///<summary>Returns the cached ResourceManager instance used by this class.</summary>
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public global::System.Resources.ResourceManager ResourceManager { get; } = new global::System.Resources.ResourceManager(typeof(Resources));

        /// <summary>Get a resource of the <see cref="ResourceManager"/> with the configured <see cref="Culture"/> as a string</summary>
        /// <param name="resourceKey">The name of the resource to get</param>
        /// <returns>Returns the resource value as a string or the <paramref name="resourceKey"/> if it could not be found</returns>
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public string GetResourceString(string resourceKey) => ResourceManager.GetString(resourceKey, Culture) ?? resourceKey;

        /// <summary>Get the resource of <see cref="Keys.@Name"/></summary>
        /// <value>value</value>
        public string @Name => GetResourceString(Keys.@Name);

        /// <summary>All keys contained in <see cref="Resources"/></summary>
        public static class Keys
        {
            public const string @Name = @"Name";
        }
    }
}
