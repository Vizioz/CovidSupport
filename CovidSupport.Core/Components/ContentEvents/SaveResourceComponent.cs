using CovidSupport.Core.Components.Examine;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services.Implement;

namespace CovidSupport.Core.Components.ContentEvents
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class SaveResourceComposer : ComponentComposer<SaveResourceComponent>
    {
    }

    public class SaveResourceComponent : IComponent
    {
        public void Initialize()
        {
            ContentService.Saved += this.ContentService_Saved;
        }

        public void Terminate()
        {
        }

        private void ContentService_Saved(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.ContentSavedEventArgs e)
        {
            foreach (var publishedItem in e.SavedEntities)
            {
                if (publishedItem.HasProperty("autoGeoCode"))
                {
                    var lat = publishedItem.GetValue<decimal?>("lat");
                    var lon = publishedItem.GetValue<decimal?>("lon");
                    var latLon = new[] {lat, lon};
                    publishedItem.SetValue("autoGeocode", JsonConvert.SerializeObject(latLon));
                    sender.Save(publishedItem, raiseEvents: false);
                }
            }
        }
    }
}
