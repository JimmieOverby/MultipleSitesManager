/* *********************************************************************** *
 * File   : FlushCommand.cs                               Part of Sitecore *
 * Version: 2.2.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents command "multisitesmanager:flush"                   *
 *          and command "multisitesmanager:flush:remote"                   *
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

using Sitecore.Eventing;
using Sitecore.Sites;
using Sitecore.Sites.Events;

namespace Sitecore.Shell.Framework.Commands
{
    /// <summary>
    /// Defines the flush command class.
    /// </summary>
    public class FlushCommand : Command
    {
        #region Overrides

        /// <summary>
        /// Executes the command in the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(CommandContext context)
        {
            // If the event manager is enabled then send the event through the queue
            // otherwise call the Flush method directly
            if (EventManager.Enabled)
            {
                // Send event into event queue to let farm instances know its time to refresh some stuff
                EventManager.QueueEvent(new FlushRemoteEvent(), true, true);
            }
            else
            {
                MultiSitesManager.Flush();
            }
        }

        #endregion
    }
}
