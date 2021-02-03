using System.Collections.Generic;

namespace CovidSupport.Api.Models
{
    public interface IResourceItem : IResourceItemBase
    {
        List<OpeningTimes> OpenHours { get; set; }

        string Contact { get; set; }

        string Email { get; set; }

        string WebLink { get; set; }

        string Twitter { get; set; }

        string Instagram { get; set; }

        string Facebook { get; set; }

        bool Free { get; set; }
    }
}
