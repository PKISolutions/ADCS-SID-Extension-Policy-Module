using System;

namespace ADCS.SidExtension.PolicyModule;

/// <summary>
/// Contains enumeration of supported SID extension location.
/// <para>This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a bitwise combination of its member values.</para>
/// </summary>
[Flags]
public enum SidValueLocation {
    /// <summary>
    /// No location specified.
    /// </summary>
    None         = 0,
    /// <summary>
    /// SID extension is located in its own extension.
    /// </summary>
    SidExtension = 1,
    /// <summary>
    /// SID Extension is part of SAN extension, URI name type.
    /// </summary>
    SanExtension = 2,
}
