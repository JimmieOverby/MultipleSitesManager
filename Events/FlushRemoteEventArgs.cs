/* *********************************************************************** *
 * File   : FlushRemoteEventArgs.cs                       Part of Sitecore *
 * Version: 2.2.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Object to be stored in the event queue database                *
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
using Sitecore.Events;

namespace Sitecore.Sites.Events
{
    public class FlushRemoteEventArgs : EventArgs, IPassNativeEventArgs
    {
        public bool RestartServer
        {
            get;
            protected set;
        }

        /// <summary>
        /// This is the object stored in the event queue for processing
        /// </summary>
        /// <param name="event">The event</param>
        /// <param name="restartServer"></param>
        public FlushRemoteEventArgs(FlushRemoteEvent @event)
        {
            RestartServer = @event.RestartServer;
        }
    }
}
