﻿using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.Unity.Shaders.Cg.Psi.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.Unity.Shaders.Cg.Psi.Tree
{
    public interface ICgTreeNode : ITreeNode
    {
        void Accept([NotNull] TreeNodeVisitor visitor);

        void Accept<TContext>([NotNull] TreeNodeVisitor<TContext> visitor, TContext context);

        TReturn Accept<TContext, TReturn>([NotNull] TreeNodeVisitor<TContext, TReturn> visitor, TContext context);
    }
}