using Examine;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;

namespace CovidSupport.Core.Components.Examine
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

        protected internal void AddIndexForContentIfItDoesNotExist(IContent content)
        {
            if (!this.IndexForContentExists(content))
            {
                var index = _indexCreator.Create(content);
                _examineManager.AddIndex(index);
            }
        }

        private bool IndexForContentExists(IContent content)
        {
            return ExamineManager.Instance.TryGetIndex(Constants.Examine.ResourceIndexName + "-" + content.Name,
                out var index);
        }
    }
}
