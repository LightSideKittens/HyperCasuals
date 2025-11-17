using System;
using System.Collections.Generic;

namespace FunGames.Core.Editor.Analyzer
{
    public abstract class FGAnalyzer: IDisposable
    {
        public virtual string Id => GetType().Name;
        public virtual string ParentId => "";
        public virtual string Name => "";

        public readonly List<FGIssue> Issues = new();
        private readonly List<FGAnalyzer> children = new ();
        public List<FGIssue> FilteredIssues = new();
        protected abstract List<FGIssue> OwnIssues();

        public void Add(FGAnalyzer analyzer)
        {
            children.Add(analyzer);
        }

        public void RunAnalysis()
        {
            Issues.Clear();
            Issues.AddRange(OwnIssues());

            foreach (var child in children)
            {
                child.RunAnalysis();
                Issues.AddRange(child.Issues);
            }
        }

        public void Dispose()
        {
            DisposeSelf();
            DisposeChildren();
        }

        protected virtual void DisposeSelf()
        {
        }

        private void DisposeChildren()
        {
            children.ForEach(a=>a.Dispose());
        }
    }
}