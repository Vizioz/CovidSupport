using Umbraco.Core;
using Umbraco.Core.Composing;

namespace CovidSupport.Core.Components.Examine
{
    public class ResourceExamineComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<ResourceExamineComponent>();
            composition.RegisterUnique<ResourceIndexCreator>();
        }
    }
}
