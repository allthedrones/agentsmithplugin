using System;
using System.Collections.Generic;
using AgentSmith.Comments;
using AgentSmith.Options;
using AgentSmith.SpellCheck;
using AgentSmith.SpellCheck.NetSpell;
using AgentSmith.Strings;

using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Services;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace AgentSmith
{
    public class InlineCommentScanDaemonStageProcess : IDaemonStageProcess
    {
        /// <summary>
        /// Internal storage for the process that this stage is a part of
        /// </summary>
        private readonly IDaemonProcess _daemonProcess;
        private readonly ISolution _solution;

        private readonly IContextBoundSettingsStore _settingsStore;

        /// <summary>
        /// Create a new process within a stage that processes comments in the current file.
        /// </summary>
        /// <param name="daemonProcess">The current instance process that this stage will be a part of</param>
        /// <param name="settingsStore"> </param>
        public InlineCommentScanDaemonStageProcess(IDaemonProcess daemonProcess, IContextBoundSettingsStore settingsStore)
        {
            this._daemonProcess = daemonProcess;
            this._solution = daemonProcess.Solution;

            this._settingsStore = settingsStore;


        }

        #region IDaemonStageProcess Members

        /// <summary>
        /// The current instance process that this stage is a part of.
        /// </summary>
        public IDaemonProcess DaemonProcess { get { return this._daemonProcess; } }

        /// <summary>
        /// Execute this stage of the process.
        /// </summary>
        /// <param name="commiter">The function to call when we've finished the stage to report the results.</param>
        public void Execute(Action<DaemonStageResult> commiter)
        {
            IFile file = _daemonProcess.SourceFile.GetTheOnlyPsiFile(CSharpLanguage.Instance);
            if (file == null)
            {
                return;
            }
			var highlightings = new List<HighlightingInfo>();
            var commentSettings = _settingsStore.GetKey<CommentSettings>(SettingsOptimization.OptimizeDefault);


            file.ProcessChildren<ICSharpCommentNode>(commentNode => CheckComment(commentNode, highlightings, commentSettings));

            try
            {
                commiter(new DaemonStageResult(highlightings));
            }
            catch
            {
                // Do nothing if it doesn't work.
            }
        }


        public void CheckComment(ICSharpCommentNode commentNode,
                                List<HighlightingInfo> highlightings, CommentSettings settings)
        {
            // Ignore it unless it's something we're re-evalutating
            if (!_daemonProcess.IsRangeInvalidated(commentNode.GetDocumentRange())) return;

            // Only look for ones that are not doc comments
            if (commentNode.CommentType != CommentType.END_OF_LINE_COMMENT &&
                commentNode.CommentType != CommentType.MULTILINE_COMMENT) return;

            ISpellChecker spellChecker = SpellCheckManager.GetSpellChecker(_settingsStore, _solution, settings.DictionaryNames);

            SpellCheck(
                commentNode.GetDocumentRange().Document,
                commentNode,
                spellChecker,
                _solution, highlightings, _settingsStore, settings);
            
        }

        public static void SpellCheck(IDocument document, ITokenNode token, ISpellChecker spellChecker,
                                               ISolution solution, List<HighlightingInfo> highlightings, IContextBoundSettingsStore settingsStore, CommentSettings settings)
        {
            if (spellChecker == null) return;

            string buffer = token.GetText();
            ILexer wordLexer = new WordLexer(buffer);
            wordLexer.Start();
            while (wordLexer.TokenType != null)
            {
                string tokenText = wordLexer.GetCurrTokenText();
                if (SpellCheckUtil.ShouldSpellCheck(tokenText, settings.CompiledWordsToIgnore) &&
                    !spellChecker.TestWord(tokenText, true))
                {
                    IClassMemberDeclaration containingElement =
                        token.GetContainingNode<IClassMemberDeclaration>(false);
                    if (containingElement == null ||
                        !IdentifierResolver.IsIdentifier(containingElement, solution, tokenText))
                    {
                        CamelHumpLexer camelHumpLexer = new CamelHumpLexer(buffer, wordLexer.TokenStart, wordLexer.TokenEnd);
                        foreach (LexerToken humpToken in camelHumpLexer)
                        {
                            if (SpellCheckUtil.ShouldSpellCheck(humpToken.Value, settings.CompiledWordsToIgnore) &&
                                !spellChecker.TestWord(humpToken.Value, true))
                            {
                                int start = token.GetTreeStartOffset().Offset + wordLexer.TokenStart;
                                int end = start + tokenText.Length;

                                TextRange range = new TextRange(start, end);
                                DocumentRange documentRange = new DocumentRange(document, range);
                                TextRange textRange = new TextRange(humpToken.Start - wordLexer.TokenStart,
                                    humpToken.End - wordLexer.TokenStart);

                                highlightings.Add(
                                    new HighlightingInfo(
                                        documentRange,
                                        new StringSpellCheckHighlighting(document.GetText(range), documentRange,
                                            humpToken.Value, textRange,
                                            solution, spellChecker, settingsStore)));


                                break;
                            }
                        }
                    }
                }

                wordLexer.Advance();
            }
        }

        #endregion
    }
}