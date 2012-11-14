using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;

namespace AgentSmith.Comments.Reflow
{
    [ActionHandler(new[] { "AgentSmith.Reflow" })]
    public class ReflowMenuAction : IActionHandler
    {
        #region Implementation of IActionHandler

        private IProjectFile GetProjectFile(IDataContext context)
        {
            IProjectModelElement element = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.PROJECT_MODEL_ELEMENT);
            IProjectFile file = element as IProjectFile;
            if (file == null) return null;
            
            if (!file.LanguageType.Equals(CSharpProjectFileType.Instance)) return null;
            return file;
        }

        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            if (GetProjectFile(context) == null) return nextUpdate();
            if (context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION) == null) return nextUpdate();
            return true;
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            IProjectFile projectFile = GetProjectFile(context);
            if (projectFile == null) return;

            IPsiSourceFile sourceFile = projectFile.ToSourceFile();
            if (sourceFile == null) return;
			IFile file = sourceFile.GetNonInjectedPsiFile<CSharpLanguage>();
            if (file == null) return;

            file.GetPsiServices().PsiManager.DoTransaction(
                () =>
                    {
                        using (WriteLockCookie.Create())
                            file.ProcessChildren<IDocCommentBlockOwnerNode>(x =>
                                                                            CommentReflowAction.ReFlowCommentBlockNode(x.GetSolution(), null, x.GetDocCommentBlockNode())
                                );
                    },
                "Reflow XML Documentation Comments");
        }

        #endregion
    }
}