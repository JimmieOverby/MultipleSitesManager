/* *********************************************************************** *
 * File   : MultiSitesManager.cs                          Part of Sitecore *
 * Version: 2.2.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents MultiSites manager logic                            *
 *                                                                         *
 * Bugs   : v2.1.0: Does not work with Regional Laguages                   *
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.SecurityModel;
using Sitecore.Shell.Framework;
using Sitecore.Web;

namespace Sitecore.Sites
{
    /// <summary>
    /// Defines the multiple sites manager class.
    /// </summary>
    public class MultiSitesManager
    {
        #region Constants

        /// <summary>
        /// "Site template" template name
        /// </summary>
        public const string SiteDefinitionTemplateName = "Site template";

        /// <summary>
        /// "Site link template" template name
        /// </summary>
        public const string SiteReferenceTemplateName = "Site link template";

        /// <summary>
        /// Content database
        /// </summary>
        private static readonly Database ContentDatabase = Factory.GetDatabase("master");

        /// <summary>
        /// System sites paths which should be placed after custom sites
        /// </summary>
        private static readonly string[] SystemSitesNames = { "website", "scheduler", "system", "publisher" };

        #endregion

        #region Static Properties

        /// <summary>
        /// Sites collection. Values - indexes
        /// </summary>
        private static readonly Hashtable sitesOrders = new Hashtable();

        /// <summary>
        /// Whether is first scan
        /// </summary>
        private static bool IsFirstScan = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the site definitions root.
        /// </summary>
        /// <value>The site definitions root.</value>
        public static Item SiteDefinitionsRoot
        {
            get { return ContentDatabase.Items["/sitecore/system/Sites"]; }
        }

        /// <summary>
        /// Gets the site definitions.
        /// </summary>
        /// <value>The site definitions.</value>
        public static Item[] SiteDefinitions
        {
            get
            {
                return (from site in SiteDefinitionsRoot.Children
                        where site.TemplateName.Equals(SiteDefinitionTemplateName) ||
                              site.TemplateName.Equals(SiteReferenceTemplateName)
                        select site).ToArray();
            }
        }

        /// <summary>
        /// Gets the system sites.
        /// </summary>
        /// <value>The system sites.</value>
        public static Item[] SystemSiteDefinitions
        {
            get
            {
                var systemSites = from siteDefinition in SiteDefinitions
                                  where SystemSitesNames.Contains(siteDefinition.Name)
                                  select siteDefinition;

                return systemSites.ToArray();
            }
        }

        /// <summary>
        /// Gets the site refs.
        /// </summary>
        /// <value>The site refs.</value>
        public static Hashtable SitesOrders
        {
            get { return sitesOrders; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public static void Flush()
        {
            IsFirstScan = true;
        }

        /// <summary>
        /// Arranges the system sites.
        /// </summary>
        public static void ArrangeSitesContext()
        {
            // Change items index
            if (SystemSiteDefinitions != null)
            {
                Items.MoveLast(SystemSiteDefinitions);
            }

            SitesOrders.Clear();

            // Store indexes for using by comparer
            foreach (Item site in SiteDefinitions)
            {
                SitesOrders[site.Name] = MainUtil.GetInt(site.Fields[FieldIDs.Sortorder].Value, 0);
            }

            // Sort sites context
            lock (SiteContextFactory.Sites)
            {
                SiteContextFactory.Sites.Sort(new SiteComparer());
            }
        }

        /// <summary>
        /// Runs the processor.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Process(HttpRequestArgs args)
        {
            if (IsFirstScan)
            {
                using (new SecurityDisabler())
                {
                    AddCustomSites();
                    ArrangeSitesContext();
                    IsFirstScan = false;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the custom sites.
        /// </summary>
        private static void AddCustomSites()
        {
            using (new SecurityDisabler())
            {
                var removeList = new List<SiteInfo>();
                var addList = new List<SiteInfo>();

                lock (SiteContextFactory.Sites)
                {
                    SiteContextFactory.Reset();

                    foreach (Item child in SiteDefinitions.Where(x => x != null && x.TemplateName.Equals(SiteDefinitionTemplateName)))
                    {
                        // If a site.config entry exists lets remove it and replace it with the entry from the system MSM
                        SiteInfo site = SiteContextFactory.Sites.FirstOrDefault(y => y.Name.Equals(child.Fields["name"].Value));
                        if (site != null) removeList.Add(site);

                        addList.Add(CreateSiteInfo(child));
                    }

                    // Process removes
                    foreach (var site in removeList)
                    {
                        SiteContextFactory.Sites.Remove(site);
                    }

                    // Process adds
                    foreach (var site in addList)
                    {
                        SiteContextFactory.Sites.Add(site);
                    }
                }

                removeList.Clear();
                addList.Clear();
            }
        }

        /// <summary>
        /// Creates the site info.
        /// </summary>
        /// <param name="item">The request item.</param>
        /// <returns>The site info.</returns>
        public static SiteInfo CreateSiteInfo(Item item)
        {
            var doc = new XmlDocument();
            var stream = new StringWriter();
            var writer = new XmlTextWriter(stream);

            writer.WriteStartElement("site");

            ConvertSection(item, item.Template.GetSection("Data"), writer);
            ConvertSection(item, item.Template.GetSection("Caching"), writer);
            ConvertSection(item, item.Template.GetSection("Additional"), writer);
            ConvertSection(item, item.Template.GetSection("Options"), writer);

            // Sub items as attributes
            AddCustomAttributes(item, writer);

            writer.WriteEndElement();

            doc.LoadXml(stream.GetStringBuilder().ToString());
            FormatSiteInfo(doc);

            return new SiteInfo(doc.DocumentElement);
        }

        /// <summary>
        /// Formats the site info.
        /// </summary>
        /// <param name="doc">The writing doc.</param>
        private static void FormatSiteInfo(XmlDocument doc)
        {
            if (doc.DocumentElement != null)
                if (doc.DocumentElement.Attributes["startItem"] != null &&
                    doc.DocumentElement.Attributes["rootPath"] != null)
                {
                    var startItem = doc.DocumentElement.Attributes["startItem"].InnerText;
                    var rootPath = doc.DocumentElement.Attributes["rootPath"].InnerText;

                    if (startItem.Length > 0 && rootPath.Length > 0)
                    {
                        int pos = startItem.IndexOf(rootPath.Trim(), StringComparison.Ordinal);
                        if (pos == 0)
                        {
                            startItem = startItem.Remove(pos, rootPath.Length);
                            doc.DocumentElement.Attributes["startItem"].InnerText = startItem;
                        }
                    }
                }
        }

        /// <summary>
        /// Adds the custom attributes.
        /// </summary>
        /// <param name="item">The requested item.</param>
        /// <param name="writer">The writer.</param>
        private static void AddCustomAttributes(Item item, XmlTextWriter writer)
        {
            if (!item.HasChildren) return;

            foreach (Item attributeItem in item.Children)
            {
                Field field = attributeItem.Fields["Value"];
                if (field != null && !string.IsNullOrEmpty(field.Value))
                {
                    writer.WriteStartAttribute(attributeItem.Name, string.Empty);
                    writer.WriteString(field.Value);
                    writer.WriteEndAttribute();
                }
            }
        }

        /// <summary>
        /// Converts the section.
        /// </summary>
        /// <param name="item">The requested item.</param>
        /// <param name="section">The section.</param>
        /// <param name="writer">The writer.</param>
        private static void ConvertSection(Item item, TemplateSectionItem section, XmlTextWriter writer)
        {
            if (section == null) return;

            foreach (Field field in section.GetFields().Select(fieldInfo => item.Fields[fieldInfo.ID]).Where(field => field != null))
            {
                ConvertField(field, writer);
            }
        }

        /// <summary>
        /// Converts the field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="writer">The writer.</param>
        private static void ConvertField(Field field, XmlTextWriter writer)
        {
            Assert.ArgumentNotNull(field, "field");
            Assert.ArgumentNotNull(writer, "writer");

            switch (field.Type.ToLower())
            {
                case "lookup":
                    var lookupField = new LookupField(field);
                    if (lookupField.TargetItem != null)
                    {
                        var language = lookupField.TargetItem["Regional Iso Code"];
                        if (string.IsNullOrEmpty(language))
                        {
                            language = lookupField.TargetItem["Iso"];
                        }

                        if (!string.IsNullOrEmpty(language))
                        {
                            writer.WriteStartAttribute(field.Name, string.Empty);
                            writer.WriteString(language);
                            writer.WriteEndAttribute();
                        }
                    }

                    break;
                case "link":
                    var link = new LinkField(field);
                    if (!string.IsNullOrEmpty(link.InternalPath))
                    {
                        writer.WriteStartAttribute(field.Name, string.Empty);
                        writer.WriteString(link.InternalPath);
                        writer.WriteEndAttribute();
                    }

                    break;
                case "checkbox":
                    var checkbox = new CheckboxField(field);
                    if (field.Name.Equals("mode"))
                    {
                        if (!checkbox.Checked)
                        {
                            writer.WriteStartAttribute(field.Name, string.Empty);
                            writer.WriteString("off");
                            writer.WriteEndAttribute();
                        }
                    }
                    else
                    {
                        if (checkbox.Checked)
                        {
                            writer.WriteStartAttribute(field.Name, string.Empty);
                            writer.WriteString("true");
                            writer.WriteEndAttribute();
                        }
                    }

                    break;
                default:
                    if (!string.IsNullOrEmpty(field.Value))
                    {
                        writer.WriteStartAttribute(field.Name, string.Empty);
                        writer.WriteString(field.Value);
                        writer.WriteEndAttribute();
                    }

                    break;
            }
        }

        #endregion
    }
}
