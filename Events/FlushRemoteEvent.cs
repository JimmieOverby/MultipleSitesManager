/* *********************************************************************** *
 * File   : FlushRemoveEvent.cs                           Part of Sitecore *
 * Version: 2.2.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Object to be serialized to the event queue database            *
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

using System.Runtime.Serialization;

namespace Sitecore.Sites.Events
{
    /// <summary>
    /// This is the object serialized to the EventQueue database
    /// </summary>
    [DataContract]
    public class FlushRemoteEvent
    {
    }
}
