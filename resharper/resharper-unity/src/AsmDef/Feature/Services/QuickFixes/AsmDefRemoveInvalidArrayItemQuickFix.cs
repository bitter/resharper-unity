﻿using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Intentions.Util;
using JetBrains.ReSharper.Plugins.Unity.AsmDef.Daemon.Errors;
using JetBrains.ReSharper.Plugins.Unity.JsonNew.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.Unity.AsmDef.Feature.Services.QuickFixes
{
    [QuickFix]
    public class AsmDefRemoveInvalidArrayItemQuickFix : QuickFixBase
    {
        [CanBeNull] private readonly IJsonNewLiteralExpression myLiteral;

        public AsmDefRemoveInvalidArrayItemQuickFix(ReferencingSelfError error)
        {
            myLiteral = error.Reference.GetTreeNode() as IJsonNewLiteralExpression;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var arrayLiteral = JsonNewArrayNavigator.GetByValue(myLiteral);
            arrayLiteral?.RemoveArrayElement(myLiteral);
            return null;
        }

        public override string Text => "Remove invalid value";
        public override bool IsAvailable(IUserDataHolder cache) => ValidUtils.Valid(myLiteral);
    }
}