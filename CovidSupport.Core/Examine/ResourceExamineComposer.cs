using Umbraco.Core;
using Umbraco.Core.Composing;

namespace CovidSupport.Core.Examine
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
