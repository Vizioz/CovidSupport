using Examine;
using Umbraco.Core.Composing;

namespace CovidSupport.Core.Examine
{
    public class ResourceExamineComponent : IComponent
    {
        private readonly IExamineManager _examineManager;

        private readonly ResourceIndexCreator _indexCreator;

        public ResourceExamineComponent(IExamineManager examineManager, ResourceIndexCreator indexCreator)
        {
            _examineManager = examineManager;
            _indexCreator = indexCreator;
        }

        public void Initialize()
        {
            foreach (var index in _indexCreator.Create())
            {
                _examineManager.AddIndex(index);
            }
        }

        public void Terminate()
        {
        }
    }
}
