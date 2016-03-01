  * **Where are Agent Smith settings?**
    1. Agent Smith settings pages are under ReSharper->Options->C#->Agent Smith Code Style Settings'

  * **How can I import/export Agent Smith settings?**
    1. Settings can be imported along with r# code style settings.
ReSharper->Options->Common->Code Style Sharing->Import/Export

  * **Why Agent Smigh warnings are not shown in R# solution-wide analysis?**
    1. R# shows only issues that have severity 'Error' in ReSharper's Inspection Severity settings. To see Agent Smith's warnings and suggestions in solution-wide analysis windows please set corresponding inspection severity level in ReSharper->Options->Inspection Serverity to 'Error'

  * **How do I import new dictionary?**
    1. Go to http://wiki.services.openoffice.org/wiki/Dictionaries
    1. Download Spelling archive that corresponds to your language
    1. Extract ".dic" and ".aff" files from the package
    1. Go to ReSharper->Options->C#->Agent Smith Code Style->Spell Check Settings.
    1. Click on 'Import Open Office Dictionary'.
    1. Choose ".dic" and ".aff" files. Click Ok.
    1. On Spell Check Settings page configure which languages you want to you for spell checking.

  * **How do I turn off Agent Smith warnings and suggestions?**
    1. Corresponding warning level can be set in ReSharper->Options->Inspection severity

  * **How do I synchronize Agent Smith custom dictionary with FxCop custom dictionary**
    1. Go to ReSharper->Options->C#->Agent Smith Code Style->Spell Check Settings.
    1. Select custom dictionary in the User Dictionaries section
    1. Click 'import' to merge new words from a FxCop dictionary file or 'export' to       merge words into a FxCop dictionary

  * **How do I disable spell checking in certain file/file region**
    1. Spell checking is disabled after '//agentsmith spellcheck disable' comment and before '//agentsmith spellcheck restore'

  * **How do I disable spell checking of strings/comments/identifiers only**
    1. This can be disabled individually in ReSharper Inspection Severity configuration page.

  * **How do I specify default language for resx file spell checking?**
    1. [assembly: NeutralResourcesLanguage("es-ES")]