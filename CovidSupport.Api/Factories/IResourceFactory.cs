using CovidSupport.Api.Models;
using Examine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace CovidSupport.Api.Factories
{
    public interface IResourceFactory
    {
        IResourceItem BuildResource(ISearchResult searchResult);

        IEnumerable<IResourceItem> BuildResources(IEnumerable<ISearchResult> searchResult);

        IEnumerable<IResourceItemBase> BuildResourcesList(IEnumerable<ISearchResult> searchResult);

        IContent BuildContent(JToken resourceItem, string resourceTypeAlias, int categoryNodeId);

        IContent BuildContent(JToken resourceItem, IContent content);
    }
}
