/* *********************************************************************** *
 * File   : AddSiteProcessor.cs                           Part of Sitecore *
 * Version: 2.2.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents item:added event handler                            *
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

using System;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Shell.Framework;

namespace Sitecore.Sites
{
    /// <summary>
    /// Defines the add site processor class.
    /// </summary>
    public class AddSiteProcessor
    {
        /// <summary>
        /// Called when the site has added.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void OnSiteAdded(object obj, EventArgs eventArgs)
        {
            var item = Event.ExtractParameter(eventArgs, 0) as Item;

            if (item == null || item.Parent == null || !item.Parent.Name.Equals("Sites")) return;

            if (!item.TemplateName.Equals(MultiSitesManager.SiteDefinitionTemplateName) &&
                !item.TemplateName.Equals(MultiSitesManager.SiteReferenceTemplateName)) return;

            Items.MoveLast(new[] { item });

            MultiSitesManager.ArrangeSitesContext();
        }
    }
}
