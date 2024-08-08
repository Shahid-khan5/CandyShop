// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace CleanArchitecture.Blazor.Infrastructure.PermissionSet;

public static partial class Permissions
{
    [DisplayName("Sales")]
    [Description("Sales Permissions")]
    public static class Sales
    {
        public const string View = "Permissions.Sales.View";
        public const string Create = "Permissions.Sales.Create";
        public const string Edit = "Permissions.Sales.Edit";
        public const string Delete = "Permissions.Sales.Delete";
        public const string Search = "Permissions.Sales.Search";
        public const string Export = "Permissions.Sales.Export";
        public const string Import = "Permissions.Sales.Import";
    }
}

