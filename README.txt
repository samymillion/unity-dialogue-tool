Samuel Million Gebretsion
251172970
------------------------------------------------------------

**Custom Language Editor Window**
Use Window -> Localization to access custom language editor and key assigner windows.
You can add/remove languages in the language editor window (removes selected language).
Add keys at top toolbar. (automatically synced b/n languages)
Expand a key to rename or remove it. (click set key after typing in new name)
Type in value to set translation value (automatically saves)

**Localization Manager**
Use the prefab, have one instance in each scene.
Set the default language through the inspector on this object.

**Localized Text Prefabs**
TMP text objects with custom script attached.
Use these for text, assign the keys in the custom key assignment window

**Custom Key Assigner**
Displays all localized text game objects, set keys through this window.
Use refresh button after creating/destroying objects.

**Language Button Prefabs**
Button objects with custom script attached for changing the language.
Use for the select language menu, assign language in the inspector.
----------------------------------------------------------------
**Dialogue Editor**
Set keys in language editor, create dialogue graph and set prompts/responses to keys
Drag to connect responses to next node
**Node Parser**
Set current dialogue graph in node parser object
Set text objects, button prefabs and so on
Use prefab dialogue canvas prefab for quick setup
**Notes**
Do not attach localizedText scripts to dialogue objects, the node parser handles localization