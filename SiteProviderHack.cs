namespace Sitecore.Sites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Sitecore.Configuration;
    using Sitecore.Sites;

    /// <summary>
    /// Defines the config site provider hack class.
    /// </summary>
    public class ConfigSiteProviderHack : ConfigSiteProvider
    {
        /// <summary>
        /// Gets a site.
        /// </summary>
        /// <param name="siteName">Name of the site.</param>
        /// <returns>Site instance</returns>
        public override Site GetSite(string siteName)
        {
            Site site = base.GetSite(siteName);
            if (site == null)
            {
                // use sites from factory as fallback (since that is how the Multisite Manager registers sites)
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
