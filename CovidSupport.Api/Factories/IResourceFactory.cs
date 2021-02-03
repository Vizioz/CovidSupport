using CovidSupport.Api.Models;
using Examine;
using System.Collections.Generic;

namespace CovidSupport.Api.Factories
{
    public interface IResourceFactory
    {
        IResourceItem BuildResource(ISearchResult searchResult);

        IEnumerable<IResourceItem> BuildResources(IEnumerable<ISearchResult> searchResult);

        IEnumerable<IResourceItemBase> BuildResourcesList(IEnumerable<ISearchResult> searchResult);
    }
}
