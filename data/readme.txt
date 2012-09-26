= Documentation for Sitecore 6.5 =

Sitecore CMS allows setting up several different sites in a single installation.

'''Note:''' this module is intended for single and multiple server solutions.

Sitecore always works in a certain context site. For example, when a user works in the Sitecore client, the context site is set to “shell”. At the same time, the front-end performs in the context of “website” site.

The clean Sitecore installation contains a number of sites configured by default, which are required for proper system work. The configuration settings are stored in the web.config file. The nature of web applications is such that it is not always convenient or even possible to edit the '''web.config''' every time you need to add a new web site. You can refer to the Configuring Multiple Sites article for more information on this subject.

Sitecore provides a possibility to add and configure multiple sites from inside the client without editing the web.config file. The Multiple Sites Manager module puts such possibility into practice. A user can easily manage and add custom sites. 

== 1.  Installation ==
The Multiple Sites Manager module is distributed as a standard Sitecore package. Thus to start using it, you should install the package. Please refer to the Installing Modules and Packages article if you are not familiar with the standard Sitecore Packager tool.

The module installs: 

 * A single DLL into the /bin directory;

 * '''!MultiSiteManager.config''' into the /App_Config/Include directory;

 * Three '''Site Templates''' under the '''templates/System/Site templates''' item.
  
 * Three appropriate '''branches''' under the '''templates/Branches/System/Site branches''' item.
  
 * The '''Sites folder''' with the '''Site Items''' in the System area.

The detailed description of each of these items is give below.

Check the /App_Config/Include/MultiSiteManager.config file after the package installation and modify it if you need to change the default behavior (be careful, settings order is very important there).

Default configuration settings adds the following processor into the httpRequestBegin pipeline before the standard !SiteResolver processor:

{{{
<processor type="Sitecore.Sites.MultiSitesManager, MultiSitesManager" />
}}}

Adds handler to the “item:added” event:

{{{
<handler type="Sitecore.Sites.AddSiteProcessor, MultiSitesManager" method="OnSiteAdded" />
}}}

Adds the following command:

{{{
<command type="Sitecore.Shell.Framework.Commands.FlushCommand,MultiSitesManager" name="multisitesmanager:flush" />
}}}

Adds a ":remote" event that handles remote events dispatched by the EventQueue in a multi-server environment:

{{{
<event name="multisitesmanager:flush:remote">
	<handler type="Sitecore.Sites.Events.FlushRemoteEventHandler, MultiSitesManager" method="OnFlushRemoteEvent"/>
</event>
}}}

== 2.  General Description ==
=== 2.1.  Site Templates ===
Three templates under the '''templates/System/Site templates''' item are added to the client after the package installation: Site Template, Site Attribute Template and the Site Link Template. 

'''Site Template''' 

Site Template represents the Site configuration information. It contains four sections with fields which match the site attributes of the '''web.config''' file:

This template is used to create Site Items which represent separate sites. 

'''Site Link Template''' 

The Site link template inherits from the Standard template and has no own fields. The purpose of the Site link Items is to reference the sites which already exist in the web.config.

'''Site Attribute Template''' 

Site Attribute Template provides a possibility to add custom attributes to a site definition. For example, it’s possible to add the htmlCacheSize attribute to the ‘website’ site. The item based on this template should be a child under the corresponding Site Item.

The template contains the only field called Value. The name of the Item serves as the attribute name, and the Value field stands for the attribute value.

== 2.2.  Site Item Folder ==
All Site Items are located under the '''system/Sites''' folder. During the package installation this folder is populated by the Site Link Items. 

=== 2.2.1.  Site Link Item ===
The purpose of the Site link Item is to reference the site which already exists in the web.config. Site Link Items are created automatically once the module is installed. If more sites are added to web.config after the module installation, an administrator will have to create appropriate Site Link Items manually. 

The order in which the sites stand under the Sites folder is important for the !SiteResolver method. The Sortorder field is used to sort the Sites collection. The default order is the same as in the web.config file. 

If the site exists in the web.config file but has no appropriate site link, its order is considered to be a very big integer value, ensuring the site is sorted last.

If the site name in web.config is changed, the name of the Site Link Item should be changed manually as well.

=== 2.2.2 Adding a New Site ===

 * Create the My New Site Item under '''sitecore/content'''.

 * Select the Sites node, click New and choose New Site.

   [[Image(multiple_sites_manager_05.png)]]

 * Enter a name, for example my_new_site. New site definition will be created. The attribute values for this site will be the same as those of the “website” site by  
   default. The site will be placed after all existing sites. 

 * Fill in the name filed with my_new_site, fill in the startItem field with /My New Site and publish the site. 

 * The newly created site should be placed before the next sites: “website”, “system”, “publisher”, “scheduler” (step performs automatically, please check it if you've changed order manually). 
   The order in which the sites are scanned by !SiteResolver is defined by the Sort order field.

   [[Image(multiple_sites_manager_06.png)]]

 * Switch to the Configure tab. It contains the Flush button. This button tells the system to arrange sites and rebuild (resort) the sites collection in memory according to the recent changes. 
   It is introduced because it is quite a resource consuming operation to rescan the sites subtree all the time. 

   [[Image(multiple_sites_manager_07.png)]]

 * Press the Flush button and start the website (!http://localhost in our case). You’ll see the My New Site site instead of the default website. This is because the My New
   Site site is placed before the website in the content tree. Change the site order, press Flush and start the website – you’ll see the default website again. 

 * After site adding and/or performing command "Flush sites", sites will be arranged automatically (to be placed before some of the system sites).

 '''Note:''' In a multi-server environment this command will flush the local AND remote server caches.
 
'''Developer note:'''
{{{
   The sites list loads from the Web.config only at the start of Web Application.
   If you want to change it by yourself (write the string to the Web.config), you provoke the Web Application to perform restart (and again it read the site list from Web.config at the start).

   What this module does is that at the start and/or after the “Flush” simply adds sites definitions to the dictionary in the memory (which was loaded once at the start).
}}}

Now consider a situation when a custom attribute should be added to the site.

Select a site Item in the content tree, click New and choose New site attribute. Provide a name for this Item – it will become the name of the attribute. Provide a value. Click save - the new attribute is added. 

Be sure that you are not duplicates attributes already defined as '''Site template''' field.

== 3.  Architectural Notes ==
All the sites defined in the web.config file populate the global sites collection. The !SiteResolver scans this collection upon each URL request and finds the best matching site. 

The solution presented here adds custom sites to the global sites collection and sorts it before the !SiteResolver interference. An administrator is responsible for providing the sort order of a site. In case the Sort Order is not specified, its value will be considered as zero.

The order in which the sites are sorted in the content tree takes precedence over the site order in the web.config file.

Below is a brief list of the steps required to implement the Multiple Sites solution:

 1. Define a “Site Template” that represents the Site configuration information.
  
 2. Create a set of “Site” items that define the Site configuration information for each site.
  
 3. Create a new project (class library).
  
 4. Create a new class with a Process() method which reads sites from the content tree and adds them into the global sites collection (!SiteContextFactory.Sites).
  
 5. Add a new <processor> definition for the newly created class into the <httpRequestBegin> section of the web.config file. The new <processor> definition should be 
    placed before the
    <processor type="Sitecore.Pipelines.!HttpRequest.!SiteResolver, Sitecore.Kernel" />
    definition. 

'''Internal Links Issue'''

Read the description of the issue by selecting this [http://sdn5.sitecore.net/SDN5/Articles/Administration/Configuring%20Multiple%20Sites/Known%20Issues/Internal%20Links.aspx link].

== 4.  Troubleshooting ==
It is recommended to place custom sites after shell and login site link items in the content tree; otherwise, you may experience problems when connecting to Sitecore Shell. If this happens and you cannot enter Sitecore Shell, just go to the !MultiSiteManager.config and comment out the following processor:

{{{
<pipelines>
 <httpRequestBegin>
 <!-- <processor type="Sitecore.Sites.MultiSitesManager, MultiSitesManager" /> -->
}}}

