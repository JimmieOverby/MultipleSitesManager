/* *********************************************************************** *
 * File   : FlushCommand.cs                               Part of Sitecore *
 * Version: 2.1.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents command "multisitesmanager:flush"                   *
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

namespace Sitecore.Shell.Framework.Commands
{
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Text;
    using Sitecore.Web.UI.Sheer;
    using Sites;

    /// <summary>
    /// Defines the flush command class.
    /// </summary>
    public class FlushCommand : Command
    {
        #region overrides

        /// <summary>
        /// Executes the command in the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(CommandContext context)
        {
            // updated to publish the site configuration nodes. flushing will be handled by publish end events
            UrlString str = new UrlString(UIUtil.GetUri("control:MultisitesManager.FlushSites"));

            SheerResponse.ShowModalDialog(str.ToString());
        }

        #endregion overrides
    }
}
