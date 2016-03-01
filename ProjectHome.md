# Agent Smith Plugin #
Agent Smith is C# code style validation plugin for ReSharper (Visual Studio plugin).

Any additions to the plugin with features you need are welcome.

[Donations](https://www.paypal.com/cgi-bin/webscr?cmd=_xclick&business=forever%2ezet%40gmail%2ecom&item_name=Agent%20Smith&no_shipping=0&no_note=1&tax=0&currency_code=USD&lc=US&bn=PP%2dDonationsBF&charset=UTF%2d8) are also gratefully welcome :)

## Current version 1.4.3 (11/07/2010) includes following features: ##

  * Naming convention validation.
  * XML comment validation.
  * Spell checking (XML comments, string literals, identifiers and resource files).
  * Smart paste.
  * [Comment reflowing](Reflow.md) (Basic reflowing of XML comments for now)

## Naming Conventions. ##
Plugin has interface that allows you to specify what naming rules should be applied to certain declarations.

### Naming rules include ###

  * Obligatory prefixes
  * Obligatory suffixes
  * Prefixes that should be avoided.
  * Suffixes that should be avoided.
  * Casing rules: Camel, Pascal, Uppercase.
  * Regular Expression rules.

Declaration to which a naming rule applies is identified by one or more matching rules. In a Matching rule following can be specified:
  * Declaration type.
  * Visibility
  * Attribute declaration is marked with.
  * Base class of interface (for classes and interfaces).
  * For a naming rule also non matching rules can be specified in similar way.

For each kind of rule plugin highlights incorrect declaration and suggests quickfixes(except regular expressions).

## XML comment validation. ##
Agent Smith can check that certain declaration have xml comments. And have flexible configuration interface for specifying what members shall have comments.

## Spell check. ##
Agent Smith performs spell checking of
  * XML comments
  * C# string literals
  * C# identifiers. Identifiers are split by camel humps and checked against dictionary.
  * Resource files (it can automatically choose appropriate dictinary depending on  resource file extension)
and suggests quick fixes (Word suggestions, Replace with <see cref...).

Open Office dictionaries can be imported so a few  languages are virtually supported.

## Smart paste. ##
If you are smart pasting a text into XML comment Agent Smith will insert '///' characters
at line breaks and optionally escape XML reserved characters.
If you are smart pasting into a string literal Agent Smith will escape string characters
correspondingly.

## Comment reflowing ##
Automatically realigns words in comment to fit configured line width.

[Download](http://code.google.com/p/agentsmithplugin/downloads/list)

[Installation Instructions](InstallationInstructions.md)

[FAQ](FAQ.md)