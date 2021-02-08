using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CovidSupport.Api.Factories;
using CovidSupport.Api.Models;
using Examine;
using Examine.LuceneEngine.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace CovidSupport.Api.Controllers
{
    public class ResourceController : BaseApiController
    {
        public ResourceController (IVariationContextAccessor variationContextAccessor) : base(variationContextAccessor)
        {
        }

        [HttpGet]
        public HttpResponseMessage Settings()
        {
            try
            {
                var settings = new ResourceSettings
                {
                    Categories = this.GetCategories(),
                    Regions = null
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, settings);
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        ////[HttpGet]
        ////public HttpResponseMessage GetAll()
        ////{
        ////    try
        ////    {
        ////        var results = this.Searcher.CreateQuery("content").All().Execute();

        ////        var items = results.Select(this.BuildResourceListItem);

        ////        return this.Request.CreateResponse(HttpStatusCode.Accepted, items, this.FormatterConfiguration);
        ////    }
        ////    catch (ApiNotFoundException e)
        ////    {
        ////        return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message, this.FormatterConfiguration);
        ////    }
        ////}

        [HttpGet]
        public HttpResponseMessage GetByCategory(int id)
        {
            try
            {
                IEnumerable<IResourceItemBase> items;
                var categories = this.GetCategories();
                var category = this.FindInCategoryTree(categories, id);

                if (category != null)
                {
                    var results = this.Index.GetSearcher().CreateQuery("content").Field("parentID", category.Id.ToString()).Execute();
                    items = this.BuildResourceList(results).ToList();
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Category not found.");
                }
                
                return this.Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetByRegion(string id)
        {
            try
            {
                IEnumerable<IResourceItemBase> items;

                var regionNode = int.TryParse(id, out int intId)
                    ? this.Website.DescendantOfType("regions").FirstChild(x => x.Id == intId)
                    : this.Website.DescendantOfType("regions").FirstChild(x => x.UrlSegment == id);

                if (regionNode != null)
                {
                    var searcher = this.Index.GetSearcher();

                    var query = (LuceneSearchQueryBase)searcher.CreateQuery("content");
                    query.QueryParser.AllowLeadingWildcard = true;

                    var val = "*" + regionNode.Key.ToString().Replace("-", string.Empty);
                    var results = query.Field("region", val.MultipleCharacterWildcard()).Execute();

                    items = this.BuildResourceList(results);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Region not found.");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            try
            {
                var result = this.Index.GetSearcher().CreateQuery("content").Id(id.ToString()).Execute(1).FirstOrDefault();

                if (result != null)
                {
                    var item = this.BuildResource(result);

                    return this.Request.CreateResponse(HttpStatusCode.OK, item);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Resource not found.");
                }
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetByTag(int tagId)
        {
            try
            {
                IEnumerable<IResourceItemBase> items;

                var tagNode = this.Website.DescendantOfType("resourceTags").FirstChild(x => x.Id == tagId);

                if (tagNode != null)
                {
                    var searcher = this.Index.GetSearcher();

                    var query = (LuceneSearchQueryBase)searcher.CreateQuery("content");
                    query.QueryParser.AllowLeadingWildcard = true;

                    var val = "*" + tagNode.Key.ToString().Replace("-", string.Empty);
                    var results = query.Field("tags", val.MultipleCharacterWildcard()).Execute();

                    items = this.BuildResourceList(results);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Resource tag not found.");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetByPopulation(int id)
        {
            try
            {
                IEnumerable<IResourceItemBase> items;

                var tagNode = this.Website.DescendantOfType("populationTypes").FirstChild(x => x.Id == id);

                if (tagNode != null)
                {
                    var searcher = this.Index.GetSearcher();

                    var query = (LuceneSearchQueryBase)searcher.CreateQuery("content");
                    query.QueryParser.AllowLeadingWildcard = true;

                    var val = "*" + tagNode.Key.ToString().Replace("-", string.Empty);
                    var results = query.Field("populationsServed", val.MultipleCharacterWildcard()).Execute();

                    items = this.BuildResourceList(results);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Population type not found.");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetByLanguageServed(int id)
        {
            try
            {
                IEnumerable<IResourceItemBase> items;

                var tagNode = this.Website.DescendantOfType("languages").FirstChild(x => x.Id == id);

                if (tagNode != null)
                {
                    var searcher = this.Index.GetSearcher();

                    var query = (LuceneSearchQueryBase)searcher.CreateQuery("content");
                    query.QueryParser.AllowLeadingWildcard = true;

                    var val = "*" + tagNode.Key.ToString().Replace("-", string.Empty);
                    var results = query.Field("populationsServed", val.MultipleCharacterWildcard()).Execute();

                    items = this.BuildResourceList(results);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Language not found.");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpPut]
        public HttpResponseMessage Add(JToken putValue)
        {
            if (putValue == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "PUT Value cannot be null");
            }

            var resourceType = putValue.Value<string>("resourceTypeId");
            var categoryNode = this.GetCategoryNodeForAllowedContent(resourceType);

            if (categoryNode == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Cannot find resource category for resource type " + resourceType);
            }

            var content = this.BuildContent(putValue, resourceType, categoryNode.Id);
            this.Services.ContentService.SaveAndPublish(content, raiseEvents: true);

            return this.Request.CreateResponse(HttpStatusCode.NotImplemented);
        }

        [HttpPut]
        public HttpResponseMessage Edit(int id, JToken putValue)
        {
            var content = this.Umbraco.Content(id);

            if (content == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Resource with id " + id + " not found");
            }

            var contentNode = this.BuildContent(putValue, content);
            this.Services.ContentService.SaveAndPublish(contentNode, raiseEvents: true);

            return this.Request.CreateResponse(HttpStatusCode.NotImplemented);
        }

        private IEnumerable<ResourceCategory> GetCategories()
        {
            var resourcesNode = this.Website.FirstChildOfType("communityResources");

            return resourcesNode.Children().Select(this.GetCategory);
        }

        private ResourceCategory GetCategory(IPublishedContent content)
        {
            if (content == null)
            {
                return null;
            }

            var category = new ResourceCategory
            {
                Id = content.Id,
                Name = content.Name
            };

            var isContainer = this.Services.ContentTypeService.Get(content.ContentType.Id).IsContainer;

            if (!isContainer)
            {
                var subcategories = content.Children().Select(this.GetCategory);
                category.Subcategories = subcategories.Any() ? subcategories : null; ;
            }

            return category;
        }

        private ResourceCategory FindInCategoryTree(IEnumerable<ResourceCategory> categories, int id)
        {
            ResourceCategory findCategory = null;

            foreach (var category in categories)
            {
                if (category.Id == id)
                {
                    findCategory = category;
                }
                else if (category.Subcategories.Any())
                {
                    findCategory = this.FindInCategoryTree(category.Subcategories, id);
                }

                if (findCategory != null)
                {
                    break;
                }
            }

            return findCategory;
        }

        private IContentType GetCategoryNodeForAllowedContent(string resourceType)
        {
            var categoryNodes = this.Website.FirstChildOfType("communityResources").DescendantsOrSelf();

            foreach (var node in categoryNodes)
            {
                var contentType = this.Services.ContentTypeService.Get(node.Id);

                if (!contentType.IsContainer && contentType.AllowedContentTypes.Any(x => x.Alias == resourceType)) {
                    return contentType;
                }
            }

            return null;
        }

        private IEnumerable<Region> GetRegions()
        {
            var regionsNode = this.Website.DescendantOfType("regions");

            return regionsNode != null
                ? regionsNode.Children.Select(x => new Region {Name = x.Name, Alias = x.UrlSegment})
                : new List<Region>();
        }        

        private IResourceItem BuildResource(ISearchResult searchResult)
        {
            var resourceType = searchResult.GetValues("__NodeTypeAlias").FirstOrDefault();
            var item = ResourceFactoryProvider.GetResourceFactory(resourceType, this.Umbraco, this.Services.ContentService, this.CultureName).BuildResource(searchResult);

            return item;
        }

        private IEnumerable<IResourceItemBase> BuildResourceList(ISearchResults searchResults)
        {
            var items = new List<IResourceItemBase>();
            var groupedResults = searchResults.GroupBy(GetProviderName);

            foreach (var group in groupedResults)
            {
                items.AddRange(ResourceFactoryProvider.GetResourceFactory(group.Key, this.Umbraco, this.Services.ContentService, this.CultureName).BuildResourcesList(group.AsEnumerable()));
            }

            return items;
        }

        private IContent BuildContent(JToken putValue, string resourceType, int categoryNodeId)
        {
            return ResourceFactoryProvider.GetResourceFactory(resourceType, this.Umbraco, this.Services.ContentService, this.CultureName).BuildContent(putValue, resourceType, categoryNodeId);
        }

        private IContent BuildContent(JToken putValue, IPublishedContent content)
        {
            var contentNode = this.Services.ContentService.GetById(content.Id);

            return ResourceFactoryProvider.GetResourceFactory(content.ContentType.Alias, this.Umbraco, this.Services.ContentService, this.CultureName).BuildContent(putValue, contentNode);
        }

        private string GetProviderName(ISearchResult result)
        {
            return ResourceFactoryProvider.GetResourceFactoryName(result.GetValues("__NodeTypeAlias").FirstOrDefault());
        }
    }
}
