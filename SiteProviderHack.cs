/* *********************************************************************** *
 * File   : ConfigSiteProviderHack.cs                     Part of Sitecore *
 * Version: 2.2.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Overrides ConfigSitePrivder GetSite method                     *
 *                                                                         *
 * Bugs   : None                                                           *
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

using Sitecore.Configuration;

namespace Sitecore.Sites
{
    public class ConfigSiteProviderHack : ConfigSiteProvider
    {
        /// <summary>
        /// Gets a site.
        /// </summary>
        /// <param name="siteName">Name of the site.</param>
        /// <returns>Site instance</returns>
        public override Site GetSite(string siteName)
        {
            var site = base.GetSite(siteName);
            if (site == null)
            {
                // Use sites from factory as fallback (since that is how the Multisite Manager registers sites)
                var siteContext = Factory.GetSite(siteName);
                if (siteContext != null)
                {
                    site = new Site(siteName);
                }
            }

            return site;
        }
    }
}
