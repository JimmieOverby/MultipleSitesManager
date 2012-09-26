/* *********************************************************************** *
 * File   : FlushHook.cs                                  Part of Sitecore *
 * Version: 2.2.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Hook that calls the event handler Run method                   *
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
using Sitecore.Eventing;
using Sitecore.Events.Hooks;
using Sitecore.Sites.Events;

namespace Sitecore.Sites.Hooks
{
    public class FlushHook : IHook
    {
        #region IHook Members

        /// <summary>
        /// This hook will call the event handler Run method
        /// </summary>
        public void Initialize()
        {
            EventManager.Subscribe(new Action<FlushRemoteEvent>(FlushRemoteEventHandler.Run));
        }

        #endregion
    }
}
