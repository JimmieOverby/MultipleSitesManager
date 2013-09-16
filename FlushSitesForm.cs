

namespace Sitecore.Shell.Applications.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.Web.UI.Pages;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Diagnostics;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Jobs;
    using Sitecore.Globalization;
    using Sitecore.Data.Items;
    using Sitecore.Data;
    using System.Web.UI.HtmlControls;
    using Sitecore.Configuration;
    using Sitecore.Text;
    using System.Web.UI;
    using Sitecore.Sites;
    using Sitecore.Publishing;
    using Sitecore.Extensions;
    public class FlushSitesForm : WizardForm
    {
        #region fields
        //Settings page
        protected Scrollbox SettingsPane;
        protected Groupbox FlushingTargetsPanel;
        protected Border FlushTargets;

        //retry page
        protected Memo ErrorText;
        // last page
        protected Literal Status;
        protected Memo ResultText;

        //first page
        protected Literal Welcome;

        #endregion
        protected string JobHandle
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["JobHandle"]);
            }
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");
                base.ServerProperties["JobHandle"] = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                BuildCheckList();

            }
        }
        protected override void ActivePageChanged(string page, string oldPage)
        {
            base.ActivePageChanged(page, oldPage);
            if (page == "Settings")
            {
            }
            else if (page == "Flushing")
            {
                base.NextButton.Disabled = true;
                base.BackButton.Disabled = true;
                base.CancelButton.Disabled = true;
                SheerResponse.SetInnerHtml("PublishingTarget", "");
                SheerResponse.Timer("StartJob", 10);
            }
        }

        private void BuildCheckList()
        {
            this.FlushTargets.Controls.Clear();
            string str2 = Settings.DefaultPublishingTargets.ToLowerInvariant();
            ListString str = new ListString(Registry.GetString("/Current_User/Publish/Targets"));
            // add master database

            Sitecore.Publishing.PublishManager.GetPublishingTargets(Context.ContentDatabase).ForEach(target =>
            {
                string str3 = "pb_" + ShortID.Encode(target.ID);
                HtmlGenericControl child = new HtmlGenericControl("input");
                this.FlushTargets.Controls.Add(child);
                child.Attributes["type"] = "checkbox";
                child.ID = str3;
                bool flag = str2.IndexOf('|' + target.Key + '|', StringComparison.InvariantCulture) >= 0;
                if (str.Contains(target.ID.ToString()))
                {
                    flag = true;
                }
                if (flag)
                {
                    child.Attributes["checked"] = "checked";
                }
                child.Disabled = !target.Access.CanWrite();
                HtmlGenericControl control2 = new HtmlGenericControl("label");
                this.FlushTargets.Controls.Add(control2);
                control2.Attributes["for"] = str3;
                control2.InnerText = target.DisplayName;
                this.FlushTargets.Controls.Add(new LiteralControl("<br>"));

            });

        }

        protected void CheckStatus()
        {
            Handle handle = Handle.Parse(this.JobHandle);
            if (!handle.IsLocal)
            {
                SheerResponse.Timer("CheckStatus", Settings.Publishing.PublishDialogPollingInterval);
            }
            else
            {
                PublishStatus status = PublishManager.GetStatus(handle);
                if (status == null)
                {
                    throw new Exception("The flushing process was unexpectedly interrupted.");
                }
                if (status.Failed)
                {
                    this.Status.Text = Translate.Text("Could not process. Please Try again", new object[] { status.Processed.ToString() });
                    base.Active = "LastPage";
                    base.BackButton.Disabled = true;
                    string str2 = StringUtil.StringCollectionToString(status.Messages, "\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        this.ResultText.Value = str2;
                    }
                }
                string str;
                if (status.State == JobState.Running)
                {
                    object[] objArray = new object[] { Translate.Text("Database:"), " ", StringUtil.Capitalize(status.CurrentTarget.NullOr<Database, string>(db => db.Name)), "<br/><br/>", Translate.Text("Language:"), " ", status.CurrentLanguage.NullOr<Language, string>(lang => lang.CultureInfo.DisplayName), "<br/><br/>", Translate.Text("Processed:"), " ", status.Processed };
                    str = string.Concat(objArray);
                }
                else if (status.State == JobState.Initializing)
                {
                    str = Translate.Text("Initializing.");
                }
                else
                {
                    str = Translate.Text("Queued.");
                }
                if (status.IsDone)
                {
                    this.Status.Text = Translate.Text("Items processed: {0}.", new object[] { status.Processed.ToString() });
                    base.Active = "LastPage";
                    base.BackButton.Disabled = true;
                    string str2 = StringUtil.StringCollectionToString(status.Messages, "\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        this.ResultText.Value = string.Format("{0}{1}{2}", str2, Environment.NewLine, Translate.Text("Job ended: Initialize Sites Refresh."));
                    }
                }
                else
                {
                    SheerResponse.SetInnerHtml("PublishingTarget", str);
                    SheerResponse.Timer("CheckStatus", Settings.Publishing.PublishDialogPollingInterval);
                }


            }

        }

        protected void StartJob()
        {

            List<Database> publishingTargetDatabases = GetPublishingTargetDatabases();
            if (publishingTargetDatabases.Any())
            {
                Handle publishHandle = null;
                publishHandle = MultiSitesManager.PublishSites(publishingTargetDatabases);
                if (publishHandle != null)
                {
                    this.JobHandle = publishHandle.ToString();
                    SheerResponse.Timer("CheckStatus", 400);
                }
                else
                {
                    base.Active = "LastPage";
                    base.BackButton.Disabled = true;
                    this.ResultText.Value = this.Status.Text = Translate.Text("Oops looks like something went wrong. Please try again or have a developer check the logs.");
                }
            }
            else
            {
                MultiSitesManager.Flush();
                base.Active = "LastPage";
                base.BackButton.Disabled = true;
                this.ResultText.Value = this.Status.Text = Translate.Text("Flushed");

            }
        }

        private static List<Database> GetPublishingTargetDatabases()
        {
            List<Database> list = new List<Database>();
            foreach (Item item in GetPublishingTargets())
            {
                string name = item["Target database"];
                Database database = Factory.GetDatabase(name);
                Assert.IsNotNull(database, typeof(Database), Translate.Text("Database \"{0}\" not found."), new object[] { name });
                list.Add(database);
            }
            return list;
        }
        private static List<Item> GetPublishingTargets()
        {
            List<Item> list = new List<Item>();
            foreach (string str in Context.ClientPage.ClientRequest.Form.Keys)
            {
                if ((str != null) && str.StartsWith("pb_", StringComparison.InvariantCulture))
                {
                    string str2 = ShortID.Decode(str.Substring(3));
                    Item item = Context.ContentDatabase.Items[str2];
                    Assert.IsNotNull(item, typeof(Item), "Publishing target not found.", new object[0]);
                    list.Add(item);
                }
            }
            return list;
        }



    }
}
