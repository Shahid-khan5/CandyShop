// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace CleanArchitecture.Blazor.Infrastructure.PermissionSet;

public static partial class Permissions
{
    [DisplayName("Campaigns")]
    [Description("Campaigns Permissions")]
    public static class Campaigns
    {
        public const string View = "Permissions.Campaigns.View";
        public const string Create = "Permissions.Campaigns.Create";
        public const string Edit = "Permissions.Campaigns.Edit";
        public const string Delete = "Permissions.Campaigns.Delete";
        public const string Search = "Permissions.Campaigns.Search";
        public const string Export = "Permissions.Campaigns.Export";
        public const string Import = "Permissions.Campaigns.Import";
    }
}

