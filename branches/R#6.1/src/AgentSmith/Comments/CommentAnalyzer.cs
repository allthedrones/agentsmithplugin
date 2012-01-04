﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

using AgentSmith.MemberMatch;
using AgentSmith.Options;
using AgentSmith.SpellCheck;
using AgentSmith.SpellCheck.NetSpell;

using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;

using Match = AgentSmith.MemberMatch.Match;

namespace AgentSmith.Comments
{
    public class CommentAnalyzer
    {
        private readonly XmlDocumentationSettings _xmlDocumentationSettings;
        private readonly IdentifierSettings _identifierSettings;
        private readonly ISolution _solution;
        private readonly ISpellChecker _xmlDocumentationSpellChecker;

        private readonly IContextBoundSettingsStore _settingsStore;

        public CommentAnalyzer(ISolution solution, IContextBoundSettingsStore settingsStore)
        {
            _settingsStore = settingsStore;
            _solution = solution;
            _xmlDocumentationSettings = _settingsStore.GetKey<XmlDocumentationSettings>(SettingsOptimization.OptimizeDefault);
            _identifierSettings = _settingsStore.GetKey<IdentifierSettings>(SettingsOptimization.OptimizeDefault);
            _xmlDocumentationSpellChecker = SpellCheckManager.GetSpellChecker(
                _settingsStore,
                _solution,
                this._xmlDocumentationSettings.DictionaryName == null
                    ? null
                    : this._xmlDocumentationSettings.DictionaryNames
                );
        }

                #region Nested type: Range

        private struct Range
        {
            public readonly TreeTextRange TreeTextRange;
            public readonly string Word;

            public Range(string word, TreeTextRange range)
            {
                Word = word;
                TreeTextRange = range;
            }
        }

        #endregion



        public void CheckCommentSpelling(IClassMemberDeclaration decl, IDocCommentBlockNode docNode,
                                  List<HighlightingInfo> highlightings, bool spellCheck)
        {

            if (docNode == null) return;

            IFile file = decl.GetContainingFile();
            if (file == null) return;

            foreach (Range wordRange in this.GetWordsFromXmlComment(docNode))
            {
                DocumentRange range = file.GetDocumentRange(wordRange.TreeTextRange);
                string word = wordRange.Word;

                if (decl.DeclaredName != word)
                {
                    if ((IdentifierResolver.IsIdentifier(decl, _solution, word, _identifierSettings.DeclarationCacheLibraryScope) ||
                         IdentifierResolver.IsKeyword(decl, _solution, word)) &&
                        IdentifierResolver.AnalyzeForMetaTagging(word, _xmlDocumentationSettings.CompiledWordsToIgnoreForMetatagging))
                    {
                        highlightings.Add(
                            new HighlightingInfo(
                                range, new CanBeSurroundedWithMetatagsHighlight(word,
                            range, decl, _solution)));
                    }
                    else if (spellCheck)
                    {
                        this.CheckWordSpelling(decl, wordRange, highlightings, range);
                    }
                }
            }
        }

        private void CheckWordSpelling(IClassMemberDeclaration decl, Range wordRange,
                                       List<HighlightingInfo> highlightings, DocumentRange range)
        {

            // If we dont have a spell checker then go no further
            if (this._xmlDocumentationSpellChecker == null) return;

            // First check the whole word range.
            if (!SpellCheckUtil.ShouldSpellCheck(wordRange.Word, _xmlDocumentationSettings.CompiledWordsToIgnore) ||
                this._xmlDocumentationSpellChecker.TestWord(wordRange.Word, true))
            {
                return;
            }

            // We are checking this word and the whole range doesn't spell anything so try breaking the word into bits.
            CamelHumpLexer camelHumpLexer = new CamelHumpLexer(wordRange.Word, 0, wordRange.Word.Length);
            foreach (LexerToken humpToken in camelHumpLexer)
            {
                if (SpellCheckUtil.ShouldSpellCheck(humpToken.Value, _xmlDocumentationSettings.CompiledWordsToIgnore) &&
                    !this._xmlDocumentationSpellChecker.TestWord(humpToken.Value, true))
                {

                    highlightings.Add(
                        new HighlightingInfo(
                            range, new WordIsNotInDictionaryHighlight(wordRange.Word, range,
                                                                          humpToken, _solution, this._xmlDocumentationSpellChecker, _settingsStore)));
                    break;
                }
            }
        }

        private IEnumerable<Range> GetWordsFromXmlComment(IDocCommentBlockNode docBlock)
        {
            if (docBlock != null)
            {
                XmlDocLexer lexer = new XmlDocLexer(docBlock);
                lexer.Start();
                int inCode = 0;
                while (lexer.TokenType != null)
                {
                    if (lexer.TokenType == lexer.XmlTokenType.TAG_START)
                    {
                        lexer.Advance();
                        if (lexer.TokenType == lexer.XmlTokenType.IDENTIFIER &&
                            (lexer.TokenText == "code" || lexer.TokenText == "c"))
                        {
                            inCode++;
                        }
                        lexer.Advance();
                        if (lexer.TokenType == lexer.XmlTokenType.TAG_END1)
                        {
                            inCode--;
                        }
                    }
                    if (lexer.TokenType == lexer.XmlTokenType.TAG_START1)
                    {
                        lexer.Advance();
                        if (lexer.TokenType == lexer.XmlTokenType.IDENTIFIER &&
                            (lexer.TokenText == "code" || lexer.TokenText == "c"))
                        {
                            inCode--;
                        }
                    }
                    if (lexer.TokenType == lexer.XmlTokenType.TEXT && inCode == 0)
                    {
                        ILexer wordLexer = new WordLexer(lexer.TokenText);
                        wordLexer.Start();
                        while (wordLexer.TokenType != null)
                        {

                            int start = lexer.CurrentNode.GetTreeStartOffset().Offset + lexer.TokenStart + wordLexer.TokenStart;
                            int end = start + wordLexer.GetCurrTokenText().Length;
                            yield return new Range(wordLexer.GetCurrTokenText(), new TreeTextRange(new TreeOffset(start), new TreeOffset(end)));
                            
                            wordLexer.Advance();
                        }
                    }
                    lexer.Advance();
                }
            }
        }



        /// <summary>
        /// Check that the given declaration has an xml documentation comment.
        /// </summary>
        /// <param name="declaration">The declaration to check</param>
        /// <param name="docNode">The documentation node to check.</param>
        /// <param name="highlightings">The list of highlights (errors) that were found - add to this any new issues</param>
        public void CheckMemberHasComment(IClassMemberDeclaration declaration, XmlNode docNode,
                                                List<HighlightingInfo> highlightings)
        {

            // Only process this one if its range is invalid.
            //if (!_daemonProcess.IsRangeInvalidated(declaration.GetDocumentRange())) return;

            // Check if the parent doco is null
            if (_xmlDocumentationSettings.SuppressIfBaseHasComment)
            {
                if (docNode == null && declaration.GetXMLDoc(true) != null) return;
            }


            if (docNode != null) return;

            Match[] publicMembers = new[]
                {
                    new Match(
                        Declaration.Any, AccessLevels.Public | AccessLevels.Protected | AccessLevels.ProtectedInternal)
                };


            Match[] internalMembers = new[] { new Match(Declaration.Any, AccessLevels.Internal) };

            Match[] privateMembers = new[] { new Match(Declaration.Any, AccessLevels.Private) };



            Match match = ComplexMatchEvaluator.IsMatch(declaration, privateMembers, null, true);
            if (match != null)
            {
                highlightings.Add(
                    new HighlightingInfo(
                        declaration.GetNameDocumentRange(),
                        new PrivateMemberMissingXmlCommentHighlighting(declaration, match)));
            }

            match = ComplexMatchEvaluator.IsMatch(declaration, internalMembers, null, true);
            if (match != null)
            {
                highlightings.Add(
                    new HighlightingInfo(
                        declaration.GetNameDocumentRange(),
                        new InternalMemberMissingXmlCommentHighlighting(declaration, match)));
            }

            match = ComplexMatchEvaluator.IsMatch(declaration, publicMembers, null, true);
            highlightings.Add(
                new HighlightingInfo(
                    declaration.GetNameDocumentRange(),
                    new PublicMemberMissingXmlCommentHighlighting(declaration, match)));
        }

    }
}
