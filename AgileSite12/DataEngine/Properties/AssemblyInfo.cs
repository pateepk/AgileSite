﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using CMS;

[assembly: AssemblyDiscoverable]

[assembly: AssemblyTitle("CMS Data Engine")]
[assembly: AssemblyDescription("CMS Data Engine")]

[assembly: ComVisible(false)]
[assembly: Guid("bf58d582-c9b7-4ed7-bf8d-268252cf418c")]

[assembly: InternalsVisibleTo("CMS.OnlineForms, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("Synchronization.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("OnlineMarketing.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("OnlineForms.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("CMS.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("Internal.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("CMSTests.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("DataEngine.Base.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("DataEngine.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("FullApp.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("CMS.ContinuousIntegration, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("ContinuousIntegration.Base.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("ContinuousIntegration.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
[assembly: InternalsVisibleTo("CMS.WebFarmSync.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("DocumentEngine.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("DataProtection.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("CMS.Ecommerce.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]
[assembly: InternalsVisibleTo("Kentico.Content.Web.Mvc.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001009571e3ca0f161bd685ac9a9cd02f3dbf50e35764ffae5d4855c70b896bd35b0c77c6a0392f18a738efdb3d9d560007674112b6d38e867d7cd1e8ee2a39dec894071dd69ff156a4c7e52ac5926515b1f4720fd02508497ddde6cfa005b178fd9b0fb78a22f71243fad40d8280ab2895a104d6263753b63f777b11cbdd955cdabe")]