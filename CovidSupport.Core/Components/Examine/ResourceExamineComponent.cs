using System;
using Examine;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace CovidSupport.Core.Components.Examine
{
    public class ResourceExamineComponent : IComponent
    {
        private readonly IExamineManager _examineManager;

        private readonly ResourceIndexCreator _indexCreator;

        protected IUmbracoContextFactory UmbracoContext { get; }

        public ResourceExamineComponent(IExamineManager examineManager, ResourceIndexCreator indexCreator, IUmbracoContextFactory context)
        {
            _examineManager = examineManager;
            _indexCreator = indexCreator;
            this.UmbracoContext = context ?? throw new System.ArgumentNullException(nameof(context));
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

        protected internal void AttemptAddIndexForContent(IContent content)
        {
            try
            {
                var index = _indexCreator.Create(content);
                _examineManager.AddIndex(index);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
