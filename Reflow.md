# Comments Reflowing #

Agent Smith can reflow comments for you.

This includes:
  * The comment is reflown in such way so each line takes not more than configurable number of characters. If line is less than this amount, words from next line are brought to this line.
**Original comment**
```
/// <summary>
/// Very long line. Very long line. Very long line. Very long line. Very long line.
/// Very short line. Very short line.
/// Very short line. Very short line.
/// </summary>
```

**Reflown comment**
```
/// <summary>
/// Very long line. Very long line. Very long 
/// line. Very long line. Very long line. Very
/// short line. Very short line. Very short line.
/// Very short line.
/// </summary>
```
  * Text is reflown inside paragraphs only. New paragraph starts after empty line. Paragraph lines become indented the same number of characters as its first line.

**Original comment**
```
/// <summary>
/// This text belongs to first paragraph so it won't be mixed with text from second
/// paragraph.
/// 
/// This is second paragraph which starts after blank line.
///
///      This is third paragraph and it is indented differently.
/// </summary>
```

**Reflown comment**
```
/// <summary>
/// This text belongs to first paragraph so it
/// won't be mixed with text from second paragraph.
/// 
/// This is second paragraph which starts after
/// blank line.
///
///      This is third paragraph and it is indented
///      differently.
/// </summary>
```

  * Line which starts with XML element is also considered as first line of paragraph. If a line ends with XML element then next line starts new paragraph.

**Original comment**
```
/// <summary>
/// <list>
///     <ul>Xml elements on start of
///        line start new paragraph.
///     </ul>
/// </list>
/// This is new paragraph so it won't be added to previous line.
/// </summary>
```

**Reflown comment**
```
/// <summary>
/// <list>
///     <ul>Xml elements on start of a line start
///     new paragraph.
///     </ul>
/// </list>
/// This is new paragraph so it won't be added to
/// previous line.
/// </summary>
```

  * Text inside `<code>` `</code>` and `<c>` `</c>` tags is not reflown.

**Original comment**
```
/// <summary>
/// This is line with a <c> code block which shall not be reflown although it is quite long</c>
/// </summary>
```

**Reflown comment**
```
/// <summary>
/// This is line with a
/// <c> code block which shall not be reflown although it is quite long</c>
/// </summary>
```

  * Space between XML elements is not reflown.

**Original comment**
```
/// <summary>
/// <p>something</p>                                             <list>
/// </list>
/// <summary>
```

**Reflown comment**
```
/// <summary>
/// <p>something</p>                                             <list>
/// </list>
/// <summary>
```

  * Text after `<br/>` tag is reflown to a new line.

**Original comment**
```
/// <summary>
/// This text shall <br/> take two lines.
/// <summary>
```

**Reflown comment**
```
/// <summary>
/// This text shall <br/>
/// take two lines.
/// <summary>
```


# Future Version #
  * It will be possible to specify in options number of tabs (or spaces) left-most paragraph shall start with. Relative paragraph indentation will be preserved.

**Original comment**
```
/// <summary>
/// Paragraph shall start with 3 spaces.
///
///     This is another paragraph with different indentation.
/// <summary>
```

**Reflown comment**
```
/// <summary>
///   Paragraph shall start with 3 spaces.
///
///        This is another paragraph with different
///        indentation.
/// <summary>
```

  * It will be possible to configure after/before which elements line break shall be added or removed. If line shall be inserted after open and before closing matching tags, they can be optionally aligned.

**Original comment**
```
/// <summary> Line breaks shall be added after/before 'summary' tags <c>but removed
/// inside 'c' tags </c> <summary>
```

**Reflown comment**
```
/// <summary>
/// Line breaks shall be added after/before
//  'summary' tags <c>but removed inside 'c' tags </c>
/// <summary>
```