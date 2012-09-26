/* *********************************************************************** *
 * File   : FlushRemoteEventHandler.cs                    Part of Sitecore *
 * Version: 2.2.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Handle local and remote event                                  *
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

using System;
using Sitecore.Diagnostics;
using Sitecore.Events;

namespace Sitecore.Sites.Events
{
    public class FlushRemoteEventHandler
    {
        /// <summary>
        /// This method does the work for the event
        /// </summary>
        public virtual void OnFlushRemoteEvent(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");

            MultiSitesManager.Flush();
        }

        /// <summary>
        /// This method is used to raise the local event
        /// </summary>
        public static void Run(FlushRemoteEvent @event)
        {
            Assert.ArgumentNotNull(@event, "@event");

            Log.Info("FlushRemoteEventHandler -> Run", typeof(FlushRemoteEventHandler));

            Event.RaiseEvent("multisitesmanager:flush:remote", new object[] { new FlushRemoteEventArgs(@event) });
        }
    }
}
