//using System;
//using System.Collections.Generic;
//using System.Net.Http.Formatting;
//using Umbraco.Core;
//using Umbraco.Core.Composing;
//using Umbraco.Web;
//using Umbraco.Web.Actions;
//using Umbraco.Web.Models.Trees;
//using Umbraco.Web.Mvc;
//using Umbraco.Web.Trees;

//using Constants = Umbraco.Core.Constants;

//namespace CovidSupport.Core.Controllers.Tree
//{
//    [Tree("settings", "websiteThemes", TreeTitle = "Website Themes", TreeGroup = "themes", SortOrder = 10)]
//    [PluginController("favouriteThings")]
//    public class ThemesTreeController : TreeController
//    {
//        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
//        {
//            if (id == Constants.System.Root.ToInvariantString())
//            {
//                // you can get your custom nodes from anywhere, and they can represent anything...
//                Dictionary<int, string> favouriteThings = new Dictionary<int, string>();
//                favouriteThings.Add(1, "NC");
//                favouriteThings.Add(2, "American Theme");
//                // create our node collection
//                var nodes = new TreeNodeCollection();

//                // loop through our favourite things and create a tree item for each one
//                foreach (var thing in favouriteThings)
//                {
//                    // add each node to the tree collection using the base CreateTreeNode method
//                    // it has several overloads, using here unique Id of tree item, -1 is the Id of the parent node to create, eg the root of this tree is -1 by convention - the querystring collection passed into this route - the name of the tree node -  css class of icon to display for the node - and whether the item has child nodes
//                    var node = CreateTreeNode(thing.Key.ToString(), "-1", queryStrings, thing.Value, "icon-layers-alt", true);
//                    nodes.Add(node);
//                }
//                return nodes;
//            }
//            else
//            {
//                var nodes = new TreeNodeCollection();

//                nodes.Add(CreateTreeNode("templates", id, queryStrings, "Templates", "icon-layers-alt", false));
//                nodes.Add(CreateTreeNode("styles", id, queryStrings, "Styles", "icon-layers-alt", false));
//                nodes.Add(CreateTreeNode("scripts", id, queryStrings, "Scripts", "icon-layers-alt", false));
//                nodes.Add(CreateTreeNode("fonts", id, queryStrings, "Fonts", "icon-layers-alt", false));

//                return nodes;
//            }

//            // this tree doesn't support rendering more than 1 level
//            throw new NotSupportedException();
//        }

//        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
//        {
//            // create a Menu Item Collection to return so people can interact with the nodes in your tree
//            var menu = new MenuItemCollection();

//            if (id == Constants.System.Root.ToInvariantString())
//            {
//                // root actions, perhaps users can create new items in this tree, or perhaps it's not a content tree, it might be a read only tree, or each node item might represent something entirely different...
//                // add your menu item actions or custom ActionMenuItems
//                menu.Items.Add(new CreateChildEntity(Services.TextService));
//                // add refresh menu item (note no dialog)
//                menu.Items.Add(new RefreshNode(Services.TextService, true));
//                return menu;
//            }
//            // add a delete action to each individual item
//            menu.Items.Add<ActionDelete>(Services.TextService, true, opensDialog: true);

//            return menu;
//        }

//        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
//        {
//            var root = base.CreateRootNode(queryStrings);

//            // set the icon
//            root.Icon = "icon-hearts";
//            // could be set to false for a custom tree with a single node.
//            root.HasChildren = true;
//            //url for menu
//            root.MenuUrl = null;

//            return root;
//        }
//    }
//}
