﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using CMS;

[assembly: AssemblyDiscoverable]

[assembly: AssemblyTitle("CMS Output Filter")]
[assembly: AssemblyDescription("CMS Output Filter")]

[assembly: ComVisible(false)]
[assembly: Guid("4772289c-62d1-416d-b0a1-f58853c7a7c3")]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityRules(SecurityRuleSet.Level1)]

[assembly: InternalsVisibleTo("CMSOutputFilter.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]