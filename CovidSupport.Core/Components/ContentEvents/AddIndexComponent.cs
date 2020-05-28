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
            ContentService.Saved += this.ContentService_Saved;
        }

        public void Terminate()
        {
        }

        private void ContentService_Saved(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.ContentSavedEventArgs e)
        {
            foreach (var publishedItem in e.SavedEntities)
            {
                if (publishedItem.ContentType.Alias == "website")
                {
                    ExamineComponent.AttemptAddIndexForContent(publishedItem);
                }
            }
        }
    }
}
