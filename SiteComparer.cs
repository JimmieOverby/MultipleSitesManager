/* *********************************************************************** *
 * File   : SiteComparer.cs                               Part of Sitecore *
 * Version: 2.1.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents custom comparer for the SiteInfo objects            *
 *                                                                         *
 * Bugs   : None known.                                                    *
 *                                                                         *
 * Status : Published.                                                     *
 *                                                                         *
 * Copyright (C) 1999-2007 by Sitecore A/S. All rights reserved.           *
 *                                                                         *
 * This work is the property of:                                           *
 *                                                                         *
 *        Sitecore A/S                                                     *
 *        Meldahlsgade 5, 4.                                               *
 *        1613 Copenhagen V.                                               *
 *        Denmark                                                          *
 *                                                                         *
 * This is a Sitecore published work under Sitecore's                      *
 * shared source license.                                                  *
 *                                                                         *
 * *********************************************************************** */

namespace Sitecore.Sites
{
    using System.Collections.Generic;
    using Sitecore.Web;

    /// <summary>
    /// Defines the site comparer class.
    /// </summary>
    public class SiteComparer : IComparer<SiteInfo>
    {
        /// <summary>
        /// Compares the specified x info.
        /// </summary>
        /// <param name="siteInfo1">The site info1.</param>
        /// <param name="siteInfo2">The site info2.</param>
        /// <returns>The int32.</returns>
        public int Compare(SiteInfo siteInfo1, SiteInfo siteInfo2)
        {
            int site1Value = MultiSitesManager.SitesOrders[siteInfo1.Name] != null ? (int)MultiSitesManager.SitesOrders[siteInfo1.Name] : int.MaxValue;
            int site2Value = MultiSitesManager.SitesOrders[siteInfo2.Name] != null ? (int)MultiSitesManager.SitesOrders[siteInfo2.Name] : int.MaxValue;

            return site1Value - site2Value;
        }
    }
}