// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage ("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Nah, not doing that.", Scope = "module")]
[assembly: SuppressMessage ("Usage", "CA2227:Collection properties should be read only", Justification = "Model relation requires public setter.", Scope = "member", Target = "~P:access.analyser.Models.Log.LogEntries")]
[assembly: SuppressMessage ("Globalization", "CA1305:Specify IFormatProvider", Justification = "Nah, still not doing that.", Scope = "module")]
[assembly: SuppressMessage ("Globalization", "CA1307:Specify StringComparison", Justification = "Nah, definitely still not doing that.", Scope = "module")]
