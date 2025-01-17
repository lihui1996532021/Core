﻿using System.Collections.Generic;
using System.Linq;
using CreativeCoders.Core;
using CreativeCoders.Core.Visitors;

namespace CreativeCoders.Scripting.CSharp.SourceCodeGenerator;

public class ClassSyntaxTreeNode : IVisitableSubItems
{
    private readonly List<ClassSyntaxTreeNode> _subNodes = new List<ClassSyntaxTreeNode>();

    private IVisitable _asVisitable;

    public void AddSubNode(ClassSyntaxTreeNode subNode)
    {
        Ensure.IsNotNull(subNode, nameof(subNode));

        _subNodes.Add(subNode);
    }

    protected virtual IVisitable GetAsVisitable()
    {
        return new VisitableAction<SyntaxSourceCodeEmitVisitor>(null);
    }

    public IEnumerable<IVisitable> GetVisitableSubItems()
    {
        return _subNodes.Select(subNode => subNode.AsVisitable);
    }

    public IVisitable AsVisitable => _asVisitable ??= GetAsVisitable();

    public IEnumerable<ClassSyntaxTreeNode> SubNodes => _subNodes;
}
