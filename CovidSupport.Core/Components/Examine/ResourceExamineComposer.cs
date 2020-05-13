using Umbraco.Core;
using Umbraco.Core.Composing;

namespace CovidSupport.Core.Components.Examine
{
    public class ResourceExamineComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register(typeof(ResourceIndexCreator));
            composition.Components().Append<ResourceExamineComponent>();
        }
    }
}
