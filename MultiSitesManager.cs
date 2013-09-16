/* *********************************************************************** *
 * File   : MultiSitesManager.cs                          Part of Sitecore *
 * Version: 2.1.1                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Represents MultiSites manager logic                            *
 *                                                                         *
 * Bugs   : V2.1.0: Did not work with Regional Laguages                    *
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

namespace Sitecore.Sites
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Shell.Framework;
    using Sitecore;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Pipelines.HttpRequest;
    using Sitecore.Sites;
    using Sitecore.Web;
    using Sitecore.Collections;
    using Sitecore.Data.Managers;
    using System;

    /// <summary>
    /// Defines the multiple sites manager class.
    /// </summary>
    public class MultiSitesManager
    {
        #region constants

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
        /// updated by xcentium to switch to web database
        /// </summary>
        private static readonly Database ContentDatabase = Factory.GetDatabase(Settings.GetSetting("MultiSitesContentDatabase", "master"));

        /// <summary>
        /// System sites paths which should be placed after custom sites
        /// </summary>
        private static readonly string[] SystemSitesNames = { "website", "scheduler", "system", "publisher" };

        #endregion constants

        #region static fields

        /// <summary>
        /// Sites collection. Values - indexes
        /// </summary>
        private static readonly Hashtable sitesOrders = new Hashtable();

        /// <summary>
        /// Whether is first scan
        /// </summary>
        private static bool IsFirstScan = true;

        #endregion static fields

        #region public properties

        /// <summary>
        /// Gets the site definitions root.
        /// </summary>
        /// <value>The site definitions root.</value>
        public static Item SiteDefinitionsRoot
        {
            get
            {
                return ContentDatabase.Items["/sitecore/system/Sites"];
            }
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
                        where site.TemplateName == SiteDefinitionTemplateName ||
                        site.TemplateName == SiteReferenceTemplateName
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
            get
            {
                return sitesOrders;
            }
        }

        #endregion public properties

        #region public API

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public static void Flush()
        {
            IsFirstScan = true;
        }


        public static Handle PublishSites(List<Database> publishingTargets)
        {
            try
            {
                Database masterDB = Factory.GetDatabase("master");
                Item sitesRoot = masterDB.GetItem(SiteDefinitionsRoot.ID);
                LanguageCollection languages = LanguageManager.GetLanguages(masterDB);
                return Publishing.PublishManager.PublishItem(sitesRoot, publishingTargets.ToArray(), languages.ToArray(), true, true);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("Multisites Manager exception", ex, new object());
                return null;
            }
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
            foreach (var site in SiteDefinitions)
            {
                SitesOrders[site.Name] = MainUtil.GetInt(site["__Sortorder"], 0);
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
            using (new SecurityModel.SecurityDisabler())
            {
                if (IsFirstScan)
                {
                    AddCustomSites();

                    ArrangeSitesContext();

                    lock (SiteContextFactory.Sites)
                    {
                        foreach (SiteInfo site in SiteContextFactory.Sites)
                        {
                            Diagnostics.Log.Info("The site name:  " + site.Name, this);
                        }
                    }

                    IsFirstScan = false;
                }
            }
        }



        #endregion public API

        #region private helpers

        /// <summary>
        /// Adds the custom sites.
        /// </summary>
        private static void AddCustomSites()
        {
            using (new SecurityModel.SecurityDisabler())
            {
                lock (SiteContextFactory.Sites)
                {
                    foreach (Item child in SiteDefinitions)
                    {
                        if (string.Equals(child.TemplateName, SiteDefinitionTemplateName))
                        {
                            foreach (SiteInfo site in SiteContextFactory.Sites)
                            {
                                if (string.Equals(site.Name.ToLower(), child["name"].ToLower()))
                                {
                                    SiteContextFactory.Sites.Remove(site);
                                    break;
                                }
                            }

                            SiteInfo info = CreateSiteInfo(child);
                            SiteContextFactory.Sites.Add(info);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the site info.
        /// </summary>
        /// <param name="item">The request item.</param>
        /// <returns>The site info.</returns>
        private static SiteInfo CreateSiteInfo(Item item)
        {
            XmlDocument doc = new XmlDocument();

            System.IO.StringWriter stream = new System.IO.StringWriter();
            System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(stream);

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
                    string startItem = doc.DocumentElement.Attributes["startItem"].InnerText;
                    string rootPath = doc.DocumentElement.Attributes["rootPath"].InnerText;

                    if (startItem.Length > 0 && rootPath.Length > 0)
                    {
                        int pos = startItem.IndexOf(rootPath.Trim());
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
            if (item.HasChildren)
            {
                foreach (Item attributeItem in item.Children)
                {
                    Field field = attributeItem.Fields["Value"];
                    if (field != null && field.Value != string.Empty)
                    {
                        writer.WriteStartAttribute(attributeItem.Name, string.Empty);
                        writer.WriteString(field.Value);
                        writer.WriteEndAttribute();
                    }
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
            if (section != null)
            {
                foreach (TemplateFieldItem fieldInfo in section.GetFields())
                {
                    Field field = item.Fields[fieldInfo.ID];

                    if (field != null)
                    {
                        ConvertField(field, writer);
                    }
                }
            }
        }

        /// <summary>
        /// Converts the field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="writer">The writer.</param>
        private static void ConvertField(Field field, XmlTextWriter writer)
        {
            switch (field.Type.ToLower())
            {
                case "lookup":
                    LookupField lookupField = new LookupField(field);
                    if (lookupField.TargetItem != null)
                    {
                        string language = lookupField.TargetItem["Regional Iso Code"];
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
                    LinkField link = new LinkField(field);
                    if (link.InternalPath != string.Empty)
                    {
                        writer.WriteStartAttribute(field.Name, string.Empty);
                        writer.WriteString(link.InternalPath);
                        writer.WriteEndAttribute();
                    }

                    break;

                case "checkbox":
                    CheckboxField checkbox = new CheckboxField(field);
                    if (field.Name == "mode")
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
                    if (field.Value != string.Empty)
                    {
                        writer.WriteStartAttribute(field.Name, string.Empty);
                        writer.WriteString(field.Value);
                        writer.WriteEndAttribute();
                    }

                    break;
            }
        }

        #endregion private helpers

    }
}