/* *********************************************************************** *
 * File   : SiteComparer.cs                               Part of Sitecore *
 * Version: 2.2.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents custom comparer for the SiteInfo objects            *
 *                                                                         *
 * Bugs   : None known.                                                    *
 *                                                                         *
 * Status : Published.                                                     *
 *                                                                         *
 * Copyright (C) 1999-2012 by Sitecore A/S. All rights reserved.           *
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

using System.Collections.Generic;
using Sitecore.Web;

namespace Sitecore.Sites
{
    /// <summary>
    /// Defines the site comparer class.
    /// </summary>
    public class SiteComparer : IComparer<SiteInfo>
    {
        #region IComparer<SiteInfo> Members

        /// <summary>
        /// Compares the specified x info.
        /// </summary>
        /// <param name="siteInfo1">The site info1.</param>
        /// <param name="siteInfo2">The site info2.</param>
        /// <returns>int</returns>
        public int Compare(SiteInfo siteInfo1, SiteInfo siteInfo2)
        {
            return GetSortOrder(siteInfo1) - GetSortOrder(siteInfo2);
        }

        /// <summary>
        /// Determine sort order based on the SitesOrders collection
        /// </summary>
        /// <param name="siteInfo">The site info</param>
        /// <returns>int</returns>
        public int GetSortOrder(SiteInfo siteInfo)
        {
            return (siteInfo != null && MultiSitesManager.SitesOrders[siteInfo.Name] != null)
                       ? (int)MultiSitesManager.SitesOrders[siteInfo.Name]
                       : int.MaxValue;
        }

        #endregion
    }
}
