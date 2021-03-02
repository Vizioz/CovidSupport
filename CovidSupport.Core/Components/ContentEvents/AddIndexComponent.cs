using CovidSupport.Core.Components.Examine;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services.Implement;

namespace CovidSupport.Core.Components.ContentEvents
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class AddIndexComposer : ComponentComposer<AddIndexComponent>
    {
    }

    public class AddIndexComponent : IComponent
    {
        protected ResourceExamineComponent ExamineComponent { get; }

        public AddIndexComponent(ResourceExamineComponent examineComponent)
        {
            ExamineComponent = examineComponent;
        }

        public void Initialize()
        {
            ContentService.Published += this.ContentService_Published;
        }

        public void Terminate()
        {
        }

        private void ContentService_Published(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.ContentPublishedEventArgs e)
        {
            foreach (var publishedItem in e.PublishedEntities)
            {
                if (publishedItem.ContentType.Alias == "website")
                {
                    ExamineComponent.AttemptAddIndexForContent(publishedItem);
                }
            }
        }
    }
}
