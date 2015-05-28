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

using System.IO;
using Sitecore.IO;

namespace Sitecore.Sites
{
    using System;
    /// <summary>
    /// Defines publish:end event
    /// </summary>
    public class PublishEndProcessor
    {
        /// <summary>
        /// Called when sites have been added and flushed
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void OnSitesPublished(object obj, EventArgs eventArgs)
        {
            Sitecore.Sites.MultiSitesManager.Flush();

        }
        public void RestartServer()
        {
            new FileInfo(FileUtil.MapPath("/web.config")).LastWriteTimeUtc = DateTime.UtcNow;
        }

    }
}
