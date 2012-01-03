using System;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Xml.XmlDocComments;
using JetBrains.Text;
using JetBrains.Util;

namespace AgentSmith.Comments
{
    public class XmlDocLexer : ILexer
    {
        public readonly XmlTokenTypes XmlTokenType = XmlTokenTypes.GetInstance(XmlDocLanguage.Instance);
        private readonly IDocCommentBlockNode _myDocCommentBlock;
        private ITreeNode _myCurrentNode;
        private IDocCommentNode _myCurrentCommentNode;
        private XmlLexerGenerated _myLexer;

        public XmlDocLexer(IDocCommentBlockNode docCommentBlock)
        {
            
            _myDocCommentBlock = docCommentBlock;
            Start();
        }

        public ITreeNode CurrentNode
        {
            get { return _myCurrentNode; }
        }

        public IDocCommentNode CurrentCommentNode
        {
            get { return _myCurrentCommentNode; }
        }

        public TextRange TokenLocalRange
        {
            get
            {
                if (_myCurrentNode == null)
                {
                    return TextRange.InvalidRange;
                }
                LeafElementBase leaf = (LeafElementBase)_myCurrentNode;
                int offset = leaf.GetTreeStartOffset().Offset - leaf.GetDocumentRange().TextRange.StartOffset;
                return new TextRange(TokenStart - offset, TokenEnd - offset);
            }
        }

        #region ILexer Members

        public void Advance()
        {
            if (_myCurrentNode != null)
            {
                uint state = _myLexer.LexerStateEx;

                _myLexer.Advance();
                if (_myLexer.TokenType == null)
                {
                    restartLexer(_myCurrentNode.NextSibling, state);
                    Logger.LogMessage("TokenStart=" + TokenStart);
                }
            }
        }

        public object CurrentPosition { get { return _myLexer.CurrentPosition; } set { _myLexer.CurrentPosition = (XmlLexerState)value; } }

        public void RestoreState(object state)
        {
            throw new InvalidOperationException();
        }

        public object SaveState()
        {
            throw new InvalidOperationException();
        }

        public void Start()
        {
            _myCurrentNode = null;
            _myCurrentCommentNode = null;
            restartLexer(_myDocCommentBlock.FirstChild, 0);
        }

        public IBuffer Buffer
        {
            get
            {
                if (_myLexer != null)
                {
                    return _myLexer.Buffer;
                }
                return null;
            }
        }

        public int TokenEnd
        {
            get
            {
                if (_myLexer != null)
                {
                    return _myLexer.TokenEnd;
                }
                return -1;
            }
        }

        public int TokenStart
        {
            get
            {
                if (_myLexer != null)
                {
                    return _myLexer.TokenStart;
                }
                return -1;
            }
        }

        public string TokenText
        {
            get
            {
                if (_myLexer != null)
                {
                    return _myLexer.GetCurrTokenText();
                }
                return null;
            }
        }

        public TokenNodeType TokenType
        {
            get
            {
                if (_myLexer != null)
                {
                    return _myLexer.TokenType;
                }
                return null;
            }
        }

        #endregion

        private void restartLexer(ITreeNode child, uint state)
        {
            _myCurrentNode = null;
            _myCurrentCommentNode = null;
            while (child != null)
            {
                _myCurrentNode = child;
                _myCurrentCommentNode = child as IDocCommentNode;
                if (_myCurrentCommentNode != null)
                {
                    LeafElementBase leaf = (LeafElementBase)_myCurrentCommentNode;

                    _myLexer = new XmlLexerGenerated(leaf.GetTextAsBuffer(), XmlTokenType);
                    //_myLexer.Start(leaf.GetTreeStartOffset().Offset + 3, leaf.GetTreeStartOffset().Offset + leaf.GetTextLength(), state);
                    _myLexer.Start(3, leaf.GetTextLength(), state);
                    if (_myLexer.TokenType == null)
                    {
                        restartLexer(_myCurrentCommentNode.NextSibling, state);
                    }
                    return;
                }

                IWhitespaceNode whitespaceNode = child as IWhitespaceNode;
                if (whitespaceNode != null && whitespaceNode.IsNewLine)
                {
                    _myLexer = new XmlLexerGenerated(new StringBuffer("\n"), XmlTokenType);
                    //_myLexer.Start(leaf.GetTreeStartOffset().Offset + 3, leaf.GetTreeStartOffset().Offset + leaf.GetTextLength(), state);
                    _myLexer.Start(0, 1, state);
                    return;
                }

                child = child.NextSibling;
            }
            _myLexer = null;
        }
    }
}