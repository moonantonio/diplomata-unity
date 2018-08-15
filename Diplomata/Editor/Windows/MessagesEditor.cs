using Diplomata.Dictionaries;
using Diplomata.Helpers;
using Diplomata.Models;
using DiplomataEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace DiplomataEditor.Windows
{
  public class MessagesEditor
  {
    private const byte HEADER_HEIGHT = GUIHelper.BUTTON_HEIGHT_SMALL + (2 * GUIHelper.MARGIN);
    private const byte LABEL_HEIGHT = HEADER_HEIGHT + 15;
    private const ushort SIDEBAR_WIDTH = 300;

    private static string[] messageList = new string[0];
    private static string[] characterList = new string[0];
    private static string[] contextList = new string[0];
    private static string[] itemList = new string[0];
    private static string[] globalFlagsList = new string[0];
    private static string[] labelsList = new string[0];
    private static string[] booleanArray = new string[] { "True", "False" };
    public static GUIStyle messagesWindowHeaderStyle = new GUIStyle(GUIHelper.windowStyle);
    public static GUIStyle messagesWindowMainStyle = new GUIStyle(GUIHelper.windowStyle);
    public static GUIStyle messagesWindowSidebarStyle = new GUIStyle(GUIHelper.windowStyle);
    public static GUIStyle textAreaStyle = new GUIStyle(GUIHelper.textAreaStyle);
    public static GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
    public static GUIStyle fakeButtonStyle = new GUIStyle(GUI.skin.button);
    public static Color inactiveColor = new Color(0, 0, 0, 0.6f);

    private static Color proColor = new Color(0.2196f, 0.2196f, 0.2196f);
    private static Color defaultColor = new Color(0.9764f, 0.9764f, 0.9764f);
    private static Vector2 scrollPosMain = new Vector2(0, 0);
    private static Vector2 scrollPosSidebar = new Vector2(0, 0);
    private static Vector2 scrollPosLabelManager = new Vector2(0, 0);
    private static Message message;

    public static void Draw()
    {
      if (TalkableMessagesManager.context != null)
      {
        messagesWindowHeaderStyle.normal.background = TalkableMessagesManager.headerBG;
        messagesWindowMainStyle.normal.background = TalkableMessagesManager.mainBG;
        messagesWindowSidebarStyle.normal.background = TalkableMessagesManager.sidebarBG;

        messagesWindowMainStyle.alignment = TextAnchor.UpperLeft;
        messagesWindowSidebarStyle.alignment = TextAnchor.UpperLeft;

        Header();
        GUILayout.BeginHorizontal();
        Main();
        Sidebar();
        GUILayout.EndHorizontal();
        LabelManager();
      }

      else
      {
        TalkableMessagesManager.Init();
      }
    }

    public static void Header()
    {
      var diplomataEditor = TalkableMessagesManager.diplomataEditor;
      var talkable = TalkableMessagesManager.talkable;
      var context = TalkableMessagesManager.context;
      string folderName = (talkable.GetType() == typeof(Character)) ? "Characters" : "Interactables";

      GUILayout.BeginHorizontal(messagesWindowHeaderStyle, GUILayout.Height(HEADER_HEIGHT));

      if (GUILayout.Button("< Back", GUILayout.Height(GUIHelper.BUTTON_HEIGHT_SMALL)))
      {
        diplomataEditor.Save(talkable, folderName);
        TalkableMessagesManager.OpenContextMenu(talkable);
      }

      EditorGUILayout.Separator();

      if (talkable.GetType() == typeof(Character))
        GUILayout.Label("Character: " + talkable.name);

      if (talkable.GetType() == typeof(Interactable))
        GUILayout.Label("Interactable: " + talkable.name);

      EditorGUILayout.Separator();

      GUILayout.Label("Column Width: ");
      context.columnWidth = (ushort) EditorGUILayout.Slider(context.columnWidth, 116, 675);

      EditorGUILayout.Separator();

      GUILayout.Label("Font Size: ");
      context.fontSize = (ushort) EditorGUILayout.Slider(context.fontSize, 8, 36);

      GUIHelper.boxStyle.fontSize = context.fontSize;
      GUIHelper.labelStyle.fontSize = context.fontSize;
      textAreaStyle.fontSize = context.fontSize;

      EditorGUILayout.Separator();

      GUILayout.Label("Filters: ");
      context.idFilter = GUILayout.Toggle(context.idFilter, "Id ");
      context.conditionsFilter = GUILayout.Toggle(context.conditionsFilter, "Conditions ");
      context.contentFilter = GUILayout.Toggle(context.contentFilter, "Content ");
      context.effectsFilter = GUILayout.Toggle(context.effectsFilter, "Effects ");

      EditorGUILayout.Separator();

      if (GUILayout.Button("Save", GUILayout.Height(GUIHelper.BUTTON_HEIGHT_SMALL)))
      {
        diplomataEditor.Save(talkable, folderName);
      }

      GUILayout.EndHorizontal();
    }

    public static void LabelManager()
    {
      var diplomataEditor = TalkableMessagesManager.diplomataEditor;
      var talkable = TalkableMessagesManager.talkable;
      var context = TalkableMessagesManager.context;
      var height = ((180 + (context.labels.Length * 240)) >= Screen.width) ? LABEL_HEIGHT : HEADER_HEIGHT;
      string folderName = (talkable.GetType() == typeof(Character)) ? "Characters" : "Interactables";

      scrollPosLabelManager = EditorGUILayout.BeginScrollView(scrollPosLabelManager,
        messagesWindowHeaderStyle, GUILayout.Width(Screen.width), GUILayout.Height(height));
      GUILayout.BeginHorizontal();

      GUILayout.Label("Labels: ", GUILayout.Width(60));

      for (int i = 0; i < context.labels.Length; i++)
      {
        var label = context.labels[i];
        label.name = EditorGUILayout.TextField(label.name, GUILayout.Width(100));
        label.color = EditorGUILayout.ColorField(label.color, GUILayout.Width(60));
        string show = (label.show) ? "hide" : "show";
        if (GUILayout.Button(show, GUILayout.Width(40), GUILayout.Height(GUIHelper.BUTTON_HEIGHT_SMALL)))
        {
          label.show = (label.show) ? false : true;
        }
        if (i > 0)
        {
          if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(GUIHelper.BUTTON_HEIGHT_SMALL)))
          {
            context.labels = ArrayHelper.Remove(context.labels, label);
            foreach (Column col in context.columns)
            {
              foreach (Message msg in col.messages)
              {
                if (msg.labelId == label.id)
                {
                  msg.labelId = context.labels[0].id;
                }
              }
            }
            diplomataEditor.Save(talkable, folderName);
          }
          GUILayout.Space(20);
        }
        else
        {
          GUILayout.Space(40);
        }
      }

      if (GUILayout.Button("Add label", GUILayout.Width(100), GUILayout.Height(GUIHelper.BUTTON_HEIGHT_SMALL)))
      {
        context.labels = ArrayHelper.Add(context.labels, new Label());
        context.labels[context.labels.Length - 1].name += " (" + (context.labels.Length - 1) + ")";
        diplomataEditor.Save(talkable, folderName);
      }

      GUILayout.EndHorizontal();
      EditorGUILayout.EndScrollView();
    }

    public static void Main()
    {
      var diplomataEditor = TalkableMessagesManager.diplomataEditor;
      var talkable = TalkableMessagesManager.talkable;
      var context = TalkableMessagesManager.context;
      var width = Screen.width - SIDEBAR_WIDTH;
      string folderName = (talkable.GetType() == typeof(Character)) ? "Characters" : "Interactables";

      ResetStyle();

      scrollPosMain = EditorGUILayout.BeginScrollView(scrollPosMain, messagesWindowMainStyle, GUILayout.Width(width));
      GUILayout.BeginHorizontal();

      for (int i = 0; i < context.columns.Length; i++)
      {

        Column column = context.columns[i];

        foreach (Column col in context.columns)
        {
          if (col.id == i)
          {
            column = col;
            break;
          }
        }

        GUILayout.BeginVertical(GUILayout.Width(context.columnWidth));

        GUILayout.Space(4);

        column.emitter = GUIHelper.Popup("Emitter: ", column.emitter, diplomataEditor.options.characterList);

        EditorGUILayout.Separator();

        for (int j = 0; j < column.messages.Length; j++)
        {

          Message currentMessage = column.messages[j];

          if (currentMessage.labelId == "")
          {
            currentMessage.labelId = context.labels[0].id;
          }

          var label = Label.Find(context.labels, currentMessage.labelId);

          foreach (Message msg in column.messages)
          {
            if (msg.id == j)
            {
              currentMessage = msg;
              break;
            }
          }

          if (label.show)
          {
            Rect boxRect = EditorGUILayout.BeginVertical(GUIHelper.boxStyle);

            var color = (EditorGUIUtility.isProSkin) ? proColor : defaultColor;

            GUIHelper.strokeWidth = 1;

            if (context.currentMessage.columnId != -1 && context.currentMessage.rowId != -1)
            {
              if (context.currentMessage.columnId == currentMessage.columnId && context.currentMessage.rowId == currentMessage.id)
              {
                color = ColorHelper.ColorAdd(color, 0.1f);
                message = currentMessage;
                GUIHelper.strokeWidth = 3;
              }
            }

            GUIHelper.labelStyle.normal.textColor = Color.black;
            textAreaStyle.normal.textColor = Color.black;

            if (color.r * color.g * color.b < 0.07f)
            {
              GUIHelper.labelStyle.normal.textColor = Color.white;
              textAreaStyle.normal.textColor = Color.white;
            }

            GUIHelper.DrawRectStroke(boxRect, color);
            EditorGUI.DrawRect(boxRect, color);

            string text = string.Empty;
            float height = 0;
            proColor = new Color(0.2196f, 0.2196f, 0.2196f);
            GUIHelper.labelStyle.alignment = TextAnchor.UpperLeft;

            if (context.idFilter)
            {
              text += "<i>[" + currentMessage.columnId + " " + currentMessage.id + "]</i>";
              GUIHelper.textContent.text = text;
              height = GUIHelper.labelStyle.CalcHeight(GUIHelper.textContent, context.columnWidth);
              GUILayout.Label(GUIHelper.textContent, GUIHelper.labelStyle, GUILayout.Width(context.columnWidth), GUILayout.Height(height));
            }

            if (context.conditionsFilter)
            {
              if (currentMessage.conditions.Length > 0)
              {
                text = "<b><i>Conditions:</i></b>\n\n";

                for (int k = 0; k < currentMessage.conditions.Length; k++)
                {
                  var condition = currentMessage.conditions[k];

                  switch (condition.type)
                  {
                    case Condition.Type.None:
                      text += condition.DisplayNone();
                      break;
                    case Condition.Type.AfterOf:
                      if (condition.afterOf.GetMessage(context) != null)
                      {
                        text += condition.DisplayAfterOf(DictionariesHelper.ContainsKey(condition.afterOf.GetMessage(context).content,
                          diplomataEditor.options.currentLanguage).value);
                      }
                      break;

                    case Condition.Type.InfluenceEqualTo:
                    case Condition.Type.InfluenceGreaterThan:
                    case Condition.Type.InfluenceLessThan:
                      text += condition.DisplayCompareInfluence();
                      break;
                    case Condition.Type.HasItem:
                      var itemName = "";
                      if (Item.Find(diplomataEditor.inventory.items, condition.itemId) != null)
                      {
                        itemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, condition.itemId).name,
                          diplomataEditor.options.currentLanguage).value;
                      }
                      text += condition.DisplayHasItem(itemName);
                      break;
                    case Condition.Type.DoesNotHaveTheItem:
                      var itemNameDont = "";
                      if (Item.Find(diplomataEditor.inventory.items, condition.itemId) != null)
                      {
                        itemNameDont = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, condition.itemId).name,
                          diplomataEditor.options.currentLanguage).value;
                      }
                      text += condition.DisplayDoesNotHaveItem(itemNameDont);
                      break;
                    case Condition.Type.ItemWasDiscarded:
                      var itemNameDiscarded = "";
                      if (Item.Find(diplomataEditor.inventory.items, condition.itemId) != null)
                      {
                        itemNameDiscarded = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, condition.itemId).name,
                          diplomataEditor.options.currentLanguage).value;
                      }
                      text += condition.DisplayItemWasDiscarded(itemNameDiscarded);
                      break;
                    case Condition.Type.ItemIsEquipped:
                      var itemNameEquipped = "";
                      if (Item.Find(diplomataEditor.inventory.items, condition.itemId) != null)
                      {
                        itemNameEquipped = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, condition.itemId).name,
                          diplomataEditor.options.currentLanguage).value;
                      }
                      text += condition.DisplayItemIsEquipped(itemNameEquipped);
                      break;
                    case Condition.Type.GlobalFlagEqualTo:
                      text += condition.DisplayGlobalFlagEqualTo();
                      break;
                  }

                  if (k < currentMessage.conditions.Length - 1)
                  {
                    text += "\n\n";
                  }
                }

                GUIHelper.labelStyle.normal.textColor = ColorHelper.ColorSub(GUIHelper.labelStyle.normal.textColor, 0, 0.4f);
                GUIHelper.textContent.text = text;
                height = GUIHelper.labelStyle.CalcHeight(GUIHelper.textContent, context.columnWidth);
                GUILayout.Label(GUIHelper.textContent, GUIHelper.labelStyle, GUILayout.Width(context.columnWidth), GUILayout.Height(height));
                GUIHelper.labelStyle.normal.textColor = ColorHelper.ColorAdd(GUIHelper.labelStyle.normal.textColor, 0, 0.4f);
              }
            }

            EditorGUI.DrawRect(new Rect(boxRect.xMin, boxRect.yMin, boxRect.width, 5), label.color);

            if (context.contentFilter)
            {
              GUIHelper.textContent.text = "<b><i>Content:</i></b>";
              height = GUIHelper.labelStyle.CalcHeight(GUIHelper.textContent, context.columnWidth);
              GUILayout.Label(GUIHelper.textContent, GUIHelper.labelStyle, GUILayout.Width(context.columnWidth), GUILayout.Height(height));

              var content = DictionariesHelper.ContainsKey(currentMessage.content, diplomataEditor.options.currentLanguage);

              if (content == null)
              {
                currentMessage.content = ArrayHelper.Add(currentMessage.content, new LanguageDictionary(diplomataEditor.options.currentLanguage, "[ Message content here ]"));
                content = DictionariesHelper.ContainsKey(currentMessage.content, diplomataEditor.options.currentLanguage);
              }

              GUIHelper.textContent.text = content.value;
              height = textAreaStyle.CalcHeight(GUIHelper.textContent, context.columnWidth);

              GUI.SetNextControlName("content" + currentMessage.id);
              content.value = EditorGUILayout.TextArea(content.value, textAreaStyle, GUILayout.Width(context.columnWidth), GUILayout.Height(height));
              EditorGUILayout.Separator();
            }

            if (context.effectsFilter)
            {
              if (currentMessage.effects.Length > 0)
              {
                text = "<b><i>Effects:</i></b>\n\n";

                for (int k = 0; k < currentMessage.effects.Length; k++)
                {
                  var effect = currentMessage.effects[k];

                  switch (effect.type)
                  {
                    case Effect.Type.None:
                      text += effect.DisplayNone();
                      break;

                    case Effect.Type.EndOfContext:
                      if (effect.endOfContext.talkableName != null)
                      {
                        text += effect.DisplayEndOfContext(DictionariesHelper.ContainsKey(effect.endOfContext.GetContext(diplomataEditor.characters, diplomataEditor.interactables).name, diplomataEditor.options.currentLanguage).value);
                      }
                      break;

                    case Effect.Type.GoTo:
                      if (effect.goTo.GetMessage(context) != null)
                      {
                        text += effect.DisplayGoTo(DictionariesHelper.ContainsKey(effect.goTo.GetMessage(context).content, diplomataEditor.options.currentLanguage).value);
                      }
                      break;
                    case Effect.Type.SetAnimatorAttribute:
                      text += effect.DisplaySetAnimatorAttribute();
                      break;
                    case Effect.Type.GetItem:
                      var itemName = "";
                      if (Item.Find(diplomataEditor.inventory.items, effect.itemId) != null)
                      {
                        itemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, effect.itemId).name,
                          diplomataEditor.options.currentLanguage).value;
                      }
                      text += effect.DisplayGetItem(itemName);
                      break;
                    case Effect.Type.DiscardItem:
                      var discardItemName = "";
                      if (Item.Find(diplomataEditor.inventory.items, effect.itemId) != null)
                      {
                        discardItemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, effect.itemId).name,
                          diplomataEditor.options.currentLanguage).value;
                      }
                      text += effect.DisplayDiscardItem(discardItemName);
                      break;
                    case Effect.Type.EquipItem:
                      var equipItemName = "";
                      if (Item.Find(diplomataEditor.inventory.items, effect.itemId) != null)
                      {
                        equipItemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, effect.itemId).name,
                          diplomataEditor.options.currentLanguage).value;
                      }
                      text += effect.DisplayEquipItem(equipItemName);
                      break;
                    case Effect.Type.SetGlobalFlag:
                      text += effect.DisplayGlobalFlagEqualTo();
                      break;
                  }

                  if (k < currentMessage.effects.Length - 1)
                  {
                    text += "\n\n";
                  }
                }

                GUIHelper.labelStyle.normal.textColor = ColorHelper.ColorSub(GUIHelper.labelStyle.normal.textColor, 0, 0.4f);
                GUIHelper.textContent.text = text;
                height = GUIHelper.labelStyle.CalcHeight(GUIHelper.textContent, context.columnWidth);
                GUILayout.Label(GUIHelper.textContent, GUIHelper.labelStyle, GUILayout.Width(context.columnWidth), GUILayout.Height(height));
                GUIHelper.labelStyle.normal.textColor = ColorHelper.ColorAdd(GUIHelper.labelStyle.normal.textColor, 0, 0.4f);
              }
            }

            if (currentMessage.disposable || currentMessage.isAChoice)
            {
              GUIHelper.labelStyle.fontSize = context.fontSize;
              GUIHelper.labelStyle.alignment = TextAnchor.UpperRight;
              GUIHelper.labelStyle.normal.textColor = ColorHelper.ColorSub(GUIHelper.labelStyle.normal.textColor, 0, 0.5f);

              text = string.Empty;

              if (currentMessage.disposable)
              {
                text += "[ disposable ] ";
              }

              if (currentMessage.isAChoice)
              {
                text += "[ is a choice ]";
              }

              GUIHelper.textContent.text = text;
              height = GUIHelper.labelStyle.CalcHeight(GUIHelper.textContent, context.columnWidth);
              GUILayout.Label(GUIHelper.textContent, GUIHelper.labelStyle, GUILayout.Width(context.columnWidth), GUILayout.Height(height));

              GUIHelper.labelStyle.fontSize = context.fontSize;
              GUIHelper.labelStyle.alignment = TextAnchor.UpperLeft;
            }

            if (GUI.Button(boxRect, "", buttonStyle))
            {
              SetMessage(currentMessage);
              EditorGUI.FocusTextInControl("");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
          }
        }

        if (GUILayout.Button("Add Message", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
        {
          column.messages = ArrayHelper.Add(column.messages, new Message(column.messages.Length, column.emitter, column.id, context.labels[0].id));

          SetMessage(null);

          diplomataEditor.Save(talkable, folderName);
        }

        EditorGUILayout.Separator();
        GUILayout.EndVertical();

        GUILayout.Space(GUIHelper.MARGIN);
      }

      if (GUILayout.Button("Add Column", GUILayout.Height(GUIHelper.BUTTON_HEIGHT), GUILayout.Width(context.columnWidth)))
      {
        context.columns = ArrayHelper.Add(context.columns, new Column(context.columns.Length));
        diplomataEditor.Save(talkable, folderName);
      }

      GUILayout.EndHorizontal();
      EditorGUILayout.EndScrollView();

      GUIHelper.labelStyle.padding = GUIHelper.zeroPadding;
    }

    public static void SetMessage(Message msg)
    {
      if (msg == null)
      {
        TalkableMessagesManager.context.messageEditorState = MessageEditorState.None;
        TalkableMessagesManager.context.currentMessage.Set(-1, -1);
      }

      else
      {
        TalkableMessagesManager.context.messageEditorState = MessageEditorState.Normal;
        TalkableMessagesManager.context.currentMessage.Set(msg.columnId, msg.id);
        message = msg;
      }
    }

    public static void ResetStyle()
    {
      textAreaStyle.normal.background = TalkableMessagesManager.textAreaBGTextureNormal;
      textAreaStyle.focused.background = TalkableMessagesManager.textAreaBGTextureFocused;
      buttonStyle.normal.background = GUIHelper.transparentTexture;
      buttonStyle.active.background = GUIHelper.transparentTexture;
      GUIHelper.labelStyle.padding = GUIHelper.padding;
    }

    public static void Sidebar()
    {
      scrollPosSidebar = EditorGUILayout.BeginScrollView(scrollPosSidebar, messagesWindowSidebarStyle, GUILayout.Width(SIDEBAR_WIDTH), GUILayout.ExpandHeight(true));

      var diplomataEditor = TalkableMessagesManager.diplomataEditor;
      var talkable = TalkableMessagesManager.talkable;
      var context = TalkableMessagesManager.context;
      string folderName = (talkable.GetType() == typeof(Character)) ? "Characters" : "Interactables";

      if (EditorGUIUtility.isProSkin)
      {
        GUIHelper.labelStyle.normal.textColor = GUIHelper.proTextColor;
      }

      else
      {
        GUIHelper.labelStyle.normal.textColor = GUIHelper.freeTextColor;
      }

      GUIHelper.labelStyle.fontSize = 12;
      GUIHelper.labelStyle.alignment = TextAnchor.UpperCenter;

      if (message != null)
      {
        switch (context.messageEditorState)
        {
          case MessageEditorState.Normal:

            GUILayout.Label("<b>Properties</b>", GUIHelper.labelStyle);
            EditorGUILayout.Separator();

            var column = Column.Find(context, message.columnId);

            var disposable = message.disposable;
            var isAChoice = message.isAChoice;

            GUILayout.BeginHorizontal();
            message.disposable = GUILayout.Toggle(message.disposable, "Disposable");
            message.isAChoice = GUILayout.Toggle(message.isAChoice, "Is a choice");
            GUILayout.EndHorizontal();

            if (message.disposable != disposable || message.isAChoice != isAChoice)
            {
              EditorGUI.FocusTextInControl("");
            }

            if (message.isAChoice)
            {

              EditorGUILayout.Separator();

              GUILayout.Label("Message attributes (most influence in): ");

              foreach (string attrName in diplomataEditor.options.attributes)
              {
                for (int i = 0; i < message.attributes.Length; i++)
                {
                  if (message.attributes[i].key == attrName)
                  {
                    break;
                  }
                  else if (i == message.attributes.Length - 1)
                  {
                    message.attributes = ArrayHelper.Add(message.attributes, new AttributeDictionary(attrName));
                  }
                }
              }

              for (int i = 0; i < message.attributes.Length; i++)
              {
                message.attributes[i].value = (byte) EditorGUILayout.Slider(message.attributes[i].key, message.attributes[i].value, 0, 100);
              }
            }

            GUIHelper.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Label: ");

            var label = Label.Find(context.labels, message.labelId);
            var labelIndex = 0;

            UpdateLabelsList(context);

            if (label != null)
            {
              for (int labelI = 0; labelI < labelsList.Length; labelI++)
              {
                if (label.name == labelsList[labelI])
                {
                  labelIndex = labelI;
                  break;
                }
              }
            }

            EditorGUI.BeginChangeCheck();

            labelIndex = EditorGUILayout.Popup(labelIndex, labelsList);

            if (EditorGUI.EndChangeCheck())
            {
              message.labelId = context.labels[labelIndex].id;
            }

            GUILayout.EndHorizontal();
            GUIHelper.Separator();

            var screenplayNotes = DictionariesHelper.ContainsKey(message.screenplayNotes, diplomataEditor.options.currentLanguage);

            if (screenplayNotes == null)
            {
              message.screenplayNotes = ArrayHelper.Add(message.screenplayNotes, new LanguageDictionary(diplomataEditor.options.currentLanguage, ""));
              screenplayNotes = DictionariesHelper.ContainsKey(message.screenplayNotes, diplomataEditor.options.currentLanguage);
            }

            GUIHelper.labelStyle.alignment = TextAnchor.UpperLeft;
            GUILayout.Label("Screenplay notes:\n<size=10>(Example: <i>whispering and gasping</i>)</size>", GUIHelper.labelStyle);
            screenplayNotes.value = EditorGUILayout.TextField(screenplayNotes.value);

            GUIHelper.Separator();

            EditorGUILayout.Separator();

            var audioClipPath = DictionariesHelper.ContainsKey(message.audioClipPath, diplomataEditor.options.currentLanguage);

            if (audioClipPath == null)
            {
              message.audioClipPath = ArrayHelper.Add(message.audioClipPath, new LanguageDictionary(diplomataEditor.options.currentLanguage, string.Empty));
              audioClipPath = DictionariesHelper.ContainsKey(message.audioClipPath, diplomataEditor.options.currentLanguage);
            }

            message.audioClip = (AudioClip) Resources.Load(audioClipPath.value);

            if (message.audioClip == null && audioClipPath.value != string.Empty)
            {
              Debug.LogWarning("Cannot find the file \"" + audioClipPath.value + "\" in Resources folder.");
            }

            GUILayout.Label("Audio to play: ");
            EditorGUI.BeginChangeCheck();

            message.audioClip = (AudioClip) EditorGUILayout.ObjectField(message.audioClip, typeof(AudioClip), false);

            if (EditorGUI.EndChangeCheck())
            {
              if (message.audioClip != null)
              {
                var str = AssetDatabase.GetAssetPath(message.audioClip).Replace("Resources/", "¬");
                var strings = str.Split('¬');
                str = strings[1].Replace(".mp3", "");
                str = str.Replace(".aif", "");
                str = str.Replace(".aiff", "");
                str = str.Replace(".ogg", "");
                str = str.Replace(".wav", "");
                audioClipPath.value = str;
              }

              else
              {
                audioClipPath.value = string.Empty;
              }
            }

            EditorGUILayout.Separator();

            message.image = (Texture2D) Resources.Load(message.imagePath);

            if (message.image == null && message.imagePath != string.Empty)
            {
              Debug.LogWarning("Cannot find the file \"" + message.imagePath + "\" in Resources folder.");
            }

            GUILayout.Label("Static image: ");
            EditorGUI.BeginChangeCheck();

            message.image = (Texture2D) EditorGUILayout.ObjectField(message.image, typeof(Texture2D), false);

            if (EditorGUI.EndChangeCheck())
            {
              if (message.image != null)
              {
                var str = AssetDatabase.GetAssetPath(message.image).Replace("Resources/", "¬");
                var strings = str.Split('¬');
                str = strings[1].Replace(".png", "");
                str = str.Replace(".jpg", "");
                str = str.Replace(".jpeg", "");
                str = str.Replace(".psd", "");
                str = str.Replace(".tga", "");
                str = str.Replace(".tiff", "");
                str = str.Replace(".gif", "");
                str = str.Replace(".bmp", "");
                message.imagePath = str;
              }

              else
              {
                message.imagePath = string.Empty;
              }
            }

            EditorGUILayout.Separator();

            EditorGUILayout.HelpBox("\nMake sure to store this audio clip, texture and animator controllers in Resources folder.\n\n" +
              "Use PlayMessageAudioContent() to play audio clip and StopMessageAudioContent() to stop.\n\n" +
              "Use SwapStaticSprite(Vector pivot) to show static image.\n", MessageType.Info);

            GUIHelper.Separator();

            GUILayout.Label("Animator Attributes Setters");

            foreach (AnimatorAttributeSetter animatorAttribute in message.animatorAttributesSetters)
            {

              EditorGUILayout.Separator();

              animatorAttribute.animator = (RuntimeAnimatorController) Resources.Load(animatorAttribute.animatorPath);

              if (animatorAttribute.animator == null && animatorAttribute.animatorPath != string.Empty)
              {
                Debug.LogWarning("Cannot find the file \"" + animatorAttribute.animatorPath + "\" in Resources folder.");
              }

              GUILayout.Label("Animator Controller: ");
              EditorGUI.BeginChangeCheck();

              animatorAttribute.animator = (RuntimeAnimatorController) EditorGUILayout.ObjectField(animatorAttribute.animator, typeof(RuntimeAnimatorController), false);

              if (EditorGUI.EndChangeCheck())
              {
                if (animatorAttribute.animator != null)
                {
                  var str = AssetDatabase.GetAssetPath(animatorAttribute.animator).Replace("Resources/", "¬");
                  var strings = str.Split('¬');
                  str = strings[1].Replace(".controller", "");
                  animatorAttribute.animatorPath = str;
                }

                else
                {
                  animatorAttribute.animatorPath = string.Empty;
                }
              }

              GUILayout.BeginHorizontal();

              GUILayout.Label("Type: ");
              animatorAttribute.type = (AnimatorControllerParameterType) EditorGUILayout.EnumPopup(animatorAttribute.type);

              EditorGUI.BeginChangeCheck();

              GUILayout.Label("Name: ");
              animatorAttribute.name = EditorGUILayout.TextField(animatorAttribute.name);

              if (EditorGUI.EndChangeCheck())
              {
                animatorAttribute.setTrigger = Animator.StringToHash(animatorAttribute.name);
              }

              GUILayout.EndHorizontal();

              GUILayout.BeginHorizontal();

              switch (animatorAttribute.type)
              {
                case AnimatorControllerParameterType.Bool:
                  string selected = animatorAttribute.setBool.ToString();

                  EditorGUI.BeginChangeCheck();

                  selected = GUIHelper.Popup("Set boolean to ", selected, booleanArray);

                  if (EditorGUI.EndChangeCheck())
                  {

                    if (selected == "True")
                    {
                      animatorAttribute.setBool = true;
                    }

                    else
                    {
                      animatorAttribute.setBool = false;
                    }

                  }

                  break;

                case AnimatorControllerParameterType.Float:
                  GUILayout.Label("Set float to ");
                  animatorAttribute.setFloat = EditorGUILayout.FloatField(animatorAttribute.setFloat);
                  break;

                case AnimatorControllerParameterType.Int:
                  GUILayout.Label("Set integer to ");
                  animatorAttribute.setInt = EditorGUILayout.IntField(animatorAttribute.setInt);
                  break;

                case AnimatorControllerParameterType.Trigger:
                  GUILayout.Label("Pull the trigger of [ " + animatorAttribute.name + " ]");
                  break;
              }

              GUILayout.EndHorizontal();
              EditorGUILayout.Separator();

              if (GUILayout.Button("Delete Animator Attribute Setter", GUILayout.Height(GUIHelper.BUTTON_HEIGHT_SMALL)))
              {
                message.animatorAttributesSetters = ArrayHelper.Remove(message.animatorAttributesSetters, animatorAttribute);
                diplomataEditor.Save(talkable, folderName);
              }

              GUIHelper.Separator();
            }

            if (GUILayout.Button("Add Animator Attribute Setter", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              message.animatorAttributesSetters = ArrayHelper.Add(message.animatorAttributesSetters, new AnimatorAttributeSetter());
              diplomataEditor.Save(talkable, folderName);
            }

            GUIHelper.Separator();

            GUILayout.Label("Edit: ");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Conditions", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              context.messageEditorState = MessageEditorState.Conditions;
            }

            if (GUILayout.Button("Effects", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              context.messageEditorState = MessageEditorState.Effects;
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            if (column.messages.Length > 1 || context.columns.Length > 1)
            {
              GUIHelper.Separator();
              GUILayout.Label("Move: ");

              fakeButtonStyle.richText = true;
              string color = "#989898";
              GUILayout.BeginHorizontal();

              if (column.id > 0)
              {
                if (GUILayout.Button("Left", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
                {
                  message.columnId -= 1;

                  var leftCol = Column.Find(context, message.columnId);

                  message.id = leftCol.messages.Length;

                  leftCol.messages = ArrayHelper.Add(leftCol.messages, message);
                  column.messages = ArrayHelper.Remove(column.messages, message);

                  column.messages = Message.ResetIDs(column.messages);
                  leftCol.messages = Message.ResetIDs(leftCol.messages);

                  SetMessage(message);
                  diplomataEditor.Save(talkable, folderName);
                }
              }

              else
              {
                GUILayout.Box("<color=" + color + ">Left</color>", fakeButtonStyle, GUILayout.Height(GUIHelper.BUTTON_HEIGHT));
              }

              if (message.id > 0)
              {
                if (GUILayout.Button("Up", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
                {
                  Message.Find(column.messages, message.id - 1).id += 1;

                  message.id -= 1;

                  SetMessage(message);
                  diplomataEditor.Save(talkable, folderName);
                }
              }

              else
              {
                GUILayout.Box("<color=" + color + ">Up</color>", fakeButtonStyle, GUILayout.Height(GUIHelper.BUTTON_HEIGHT));
              }

              if (message.id < column.messages.Length - 1)
              {
                if (GUILayout.Button("Down", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
                {
                  Message.Find(column.messages, message.id + 1).id -= 1;

                  message.id += 1;

                  SetMessage(message);
                  diplomataEditor.Save(talkable, folderName);
                }
              }

              else
              {
                GUILayout.Box("<color=" + color + ">Down</color>", fakeButtonStyle, GUILayout.Height(GUIHelper.BUTTON_HEIGHT));
              }

              if (column.id < context.columns.Length - 1)
              {
                if (GUILayout.Button("Right", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
                {
                  message.columnId += 1;

                  var rightCol = Column.Find(context, message.columnId);

                  message.id = rightCol.messages.Length;

                  rightCol.messages = ArrayHelper.Add(rightCol.messages, message);
                  column.messages = ArrayHelper.Remove(column.messages, message);

                  column.messages = Message.ResetIDs(column.messages);
                  rightCol.messages = Message.ResetIDs(rightCol.messages);

                  SetMessage(message);
                  diplomataEditor.Save(talkable, folderName);
                }
              }

              else
              {
                GUILayout.Box("<color=" + color + ">Right</color>", fakeButtonStyle, GUILayout.Height(GUIHelper.BUTTON_HEIGHT));
              }

              GUILayout.EndHorizontal();
            }

            GUIHelper.Separator();
            GUILayout.Label("Other options: ");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Duplicate", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {

              column.messages = ArrayHelper.Add(column.messages, new Message(message, column.messages.Length));

              SetMessage(null);

              diplomataEditor.Save(talkable, folderName);
            }

            if (GUILayout.Button("Delete", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              if (EditorUtility.DisplayDialog("Are you sure?", "If you agree all this message data will be lost forever.", "Yes", "No"))
              {

                column.messages = ArrayHelper.Remove(column.messages, message);

                SetMessage(null);

                column.messages = Message.ResetIDs(column.messages);

                diplomataEditor.Save(talkable, folderName);
              }
            }

            GUILayout.EndHorizontal();

            GUIHelper.Separator();

            GUILayout.Label("Columns: ");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New column at left", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              context.columns = ArrayHelper.Add(context.columns, new Column(context.columns.Length));

              MoveColumnsToRight(context, column.id);

              SetMessage(null);
              diplomataEditor.Save(talkable, folderName);
            }

            if (GUILayout.Button("New column at right", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              context.columns = ArrayHelper.Add(context.columns, new Column(context.columns.Length));

              MoveColumnsToRight(context, column.id + 1);

              SetMessage(null);
              diplomataEditor.Save(talkable, folderName);
            }

            GUILayout.EndHorizontal();

            break;

          case MessageEditorState.Conditions:

            GUILayout.Label("<b>Conditions</b>", GUIHelper.labelStyle);
            EditorGUILayout.Separator();

            if (GUILayout.Button("< Back", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              context.messageEditorState = MessageEditorState.Normal;
            }

            GUIHelper.Separator();

            var j = 0;

            foreach (Condition condition in message.conditions)
            {

              GUIHelper.labelStyle.fontSize = 11;
              GUIHelper.labelStyle.alignment = TextAnchor.UpperLeft;
              GUILayout.Label("<i>Condition " + j + "</i>\n", GUIHelper.labelStyle);
              j++;

              GUILayout.BeginHorizontal();
              GUILayout.Label("Type: ");
              condition.type = (Condition.Type) EditorGUILayout.EnumPopup(condition.type);
              GUILayout.EndHorizontal();

              switch (condition.type)
              {
                case Condition.Type.AfterOf:
                  UpdateMessagesList(context);

                  var messageName = "";

                  if (condition.afterOf.GetMessage(context) != null)
                  {
                    messageName = DictionariesHelper.ContainsKey(condition.afterOf.GetMessage(context).content, diplomataEditor.options.currentLanguage).value;
                  }

                  EditorGUI.BeginChangeCheck();

                  messageName = GUIHelper.Popup("Message: ", messageName, messageList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Column col in context.columns)
                    {
                      foreach (Message msg in col.messages)
                      {

                        if (DictionariesHelper.ContainsKey(msg.content, diplomataEditor.options.currentLanguage).value == messageName)
                        {
                          condition.afterOf.uniqueId = msg.GetUniqueId();
                          break;
                        }

                      }
                    }
                  }

                  break;

                case Condition.Type.InfluenceEqualTo:
                case Condition.Type.InfluenceGreaterThan:
                case Condition.Type.InfluenceLessThan:
                  UpdateCharacterList();

                  GUILayout.BeginHorizontal();
                  EditorGUI.BeginChangeCheck();

                  if (condition.characterInfluencedName == string.Empty && characterList.Length > 0)
                  {
                    condition.characterInfluencedName = characterList[0];
                  }

                  condition.comparedInfluence = EditorGUILayout.IntField(condition.comparedInfluence);
                  condition.characterInfluencedName = GUIHelper.Popup(" in ", condition.characterInfluencedName, characterList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    condition.DisplayCompareInfluence();
                  }
                  GUILayout.EndHorizontal();

                  break;

                case Condition.Type.HasItem:
                  GUILayout.BeginHorizontal();
                  UpdateItemList();

                  var itemName = "";

                  if (itemList.Length > 0)
                  {
                    itemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, condition.itemId).name, diplomataEditor.options.currentLanguage).value;
                  }

                  EditorGUI.BeginChangeCheck();

                  itemName = GUIHelper.Popup("Has item ", itemName, itemList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Item item in diplomataEditor.inventory.items)
                    {

                      if (DictionariesHelper.ContainsKey(item.name, diplomataEditor.options.currentLanguage).value == itemName)
                      {
                        condition.itemId = item.id;
                        break;
                      }

                    }
                  }

                  GUILayout.EndHorizontal();
                  break;

                case Condition.Type.DoesNotHaveTheItem:
                  GUILayout.BeginHorizontal();
                  UpdateItemList();

                  var itemNameDont = "";

                  if (itemList.Length > 0)
                  {
                    itemNameDont = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, condition.itemId).name, diplomataEditor.options.currentLanguage).value;
                  }

                  EditorGUI.BeginChangeCheck();

                  itemNameDont = GUIHelper.Popup("Does not have the item ", itemNameDont, itemList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Item item in diplomataEditor.inventory.items)
                    {

                      if (DictionariesHelper.ContainsKey(item.name, diplomataEditor.options.currentLanguage).value == itemNameDont)
                      {
                        condition.itemId = item.id;
                        break;
                      }

                    }
                  }

                  GUILayout.EndHorizontal();
                  break;

                case Condition.Type.ItemIsEquipped:
                  GUILayout.BeginHorizontal();
                  UpdateItemList();

                  var equippedItemName = "";

                  if (itemList.Length > 0)
                  {
                    equippedItemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, condition.itemId).name, diplomataEditor.options.currentLanguage).value;
                  }

                  EditorGUI.BeginChangeCheck();

                  equippedItemName = GUIHelper.Popup("Item is equipped ", equippedItemName, itemList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Item item in diplomataEditor.inventory.items)
                    {

                      if (DictionariesHelper.ContainsKey(item.name, diplomataEditor.options.currentLanguage).value == equippedItemName)
                      {
                        condition.itemId = item.id;
                        break;
                      }

                    }
                  }

                  GUILayout.EndHorizontal();
                  break;

                case Condition.Type.ItemWasDiscarded:
                  GUILayout.BeginHorizontal();
                  UpdateItemList();

                  var discardedItemName = "";

                  if (itemList.Length > 0)
                  {
                    discardedItemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, condition.itemId).name, diplomataEditor.options.currentLanguage).value;
                  }

                  EditorGUI.BeginChangeCheck();

                  discardedItemName = GUIHelper.Popup("Item was discarded ", discardedItemName, itemList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Item item in diplomataEditor.inventory.items)
                    {

                      if (DictionariesHelper.ContainsKey(item.name, diplomataEditor.options.currentLanguage).value == discardedItemName)
                      {
                        condition.itemId = item.id;
                        break;
                      }

                    }
                  }

                  GUILayout.EndHorizontal();
                  break;

                case Condition.Type.GlobalFlagEqualTo:
                  UpdateGlobalFlagsList();

                  condition.globalFlag.name = GUIHelper.Popup("Flag: ", condition.globalFlag.name, globalFlagsList);

                  string selected = condition.globalFlag.value.ToString();

                  EditorGUI.BeginChangeCheck();

                  selected = GUIHelper.Popup("is ", selected, booleanArray);

                  if (EditorGUI.EndChangeCheck())
                  {

                    if (selected == "True")
                    {
                      condition.globalFlag.value = true;
                    }

                    else
                    {
                      condition.globalFlag.value = false;
                    }

                  }

                  break;
              }

              if (GUILayout.Button("Delete Condition", GUILayout.Height(GUIHelper.BUTTON_HEIGHT_SMALL)))
              {
                message.conditions = ArrayHelper.Remove(message.conditions, condition);
                diplomataEditor.Save(talkable, folderName);
              }

              GUIHelper.Separator();
            }

            if (GUILayout.Button("Add Condition", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              message.conditions = ArrayHelper.Add(message.conditions, new Condition());
              diplomataEditor.Save(talkable, folderName);
            }

            break;

          case MessageEditorState.Effects:

            GUILayout.Label("<b>Effects</b>", GUIHelper.labelStyle);
            EditorGUILayout.Separator();

            if (GUILayout.Button("< Back", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              context.messageEditorState = MessageEditorState.Normal;
            }

            GUIHelper.Separator();

            var k = 0;

            foreach (Effect effect in message.effects)
            {

              GUIHelper.labelStyle.fontSize = 11;
              GUIHelper.labelStyle.alignment = TextAnchor.UpperLeft;
              GUILayout.Label("<i>Effect " + k + "</i>\n", GUIHelper.labelStyle);
              k++;

              GUILayout.BeginHorizontal();
              GUILayout.Label("Type: ");
              effect.type = (Effect.Type) EditorGUILayout.EnumPopup(effect.type);
              GUILayout.EndHorizontal();

              switch (effect.type)
              {
                case Effect.Type.EndOfContext:

                  UpdateContextList();
                  var contextName = string.Empty;

                  if (effect.endOfContext.talkableName != null)
                  {
                    Context contextToEnd = effect.endOfContext.GetContext(diplomataEditor.characters, diplomataEditor.interactables);
                    if (contextToEnd != null)
                    {
                      contextName = DictionariesHelper.ContainsKey(contextToEnd.name, diplomataEditor.options.currentLanguage).value;
                    }
                  }

                  EditorGUI.BeginChangeCheck();

                  contextName = GUIHelper.Popup("Context: ", contextName, contextList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Character tempCharacter in diplomataEditor.characters)
                    {
                      foreach (Context ctx in tempCharacter.contexts)
                      {

                        if (DictionariesHelper.ContainsKey(ctx.name, diplomataEditor.options.currentLanguage).value == contextName)
                        {
                          effect.endOfContext.Set(tempCharacter.name, ctx.id);
                          break;
                        }

                      }
                    }
                  }

                  break;

                case Effect.Type.GoTo:
                  UpdateMessagesList(context);
                  var messageContent = string.Empty;

                  if (effect.goTo.GetMessage(context) != null)
                  {
                    messageContent = DictionariesHelper.ContainsKey(effect.goTo.GetMessage(context).content, diplomataEditor.options.currentLanguage).value;
                  }

                  EditorGUI.BeginChangeCheck();

                  messageContent = GUIHelper.Popup("Message: ", messageContent, messageList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Column col in context.columns)
                    {
                      foreach (Message msg in col.messages)
                      {

                        if (DictionariesHelper.ContainsKey(msg.content, diplomataEditor.options.currentLanguage).value == messageContent)
                        {
                          effect.goTo.uniqueId = msg.GetUniqueId();
                          break;
                        }

                      }
                    }
                  }

                  break;

                case Effect.Type.SetAnimatorAttribute:

                  effect.animatorAttributeSetter.animator = (RuntimeAnimatorController) Resources.Load(effect.animatorAttributeSetter.animatorPath);

                  if (effect.animatorAttributeSetter.animator == null && effect.animatorAttributeSetter.animatorPath != string.Empty)
                  {
                    Debug.LogWarning("Cannot find the file \"" + effect.animatorAttributeSetter.animatorPath + "\" in Resources folder.");
                  }

                  GUILayout.Label("Animator Controller: ");
                  EditorGUI.BeginChangeCheck();

                  effect.animatorAttributeSetter.animator = (RuntimeAnimatorController) EditorGUILayout.ObjectField(effect.animatorAttributeSetter.animator,
                    typeof(RuntimeAnimatorController), false);

                  if (EditorGUI.EndChangeCheck())
                  {
                    if (effect.animatorAttributeSetter.animator != null)
                    {
                      var str = AssetDatabase.GetAssetPath(effect.animatorAttributeSetter.animator).Replace("Resources/", "¬");
                      var strings = str.Split('¬');
                      str = strings[1].Replace(".controller", "");
                      effect.animatorAttributeSetter.animatorPath = str;
                    }

                    else
                    {
                      effect.animatorAttributeSetter.animatorPath = string.Empty;
                    }
                  }

                  GUILayout.BeginHorizontal();

                  GUILayout.Label("Type: ");
                  effect.animatorAttributeSetter.type = (AnimatorControllerParameterType) EditorGUILayout.EnumPopup(effect.animatorAttributeSetter.type);

                  EditorGUI.BeginChangeCheck();

                  GUILayout.Label("Name: ");
                  effect.animatorAttributeSetter.name = EditorGUILayout.TextField(effect.animatorAttributeSetter.name);

                  if (EditorGUI.EndChangeCheck())
                  {
                    effect.animatorAttributeSetter.setTrigger = Animator.StringToHash(effect.animatorAttributeSetter.name);
                  }

                  GUILayout.EndHorizontal();

                  GUILayout.BeginHorizontal();

                  switch (effect.animatorAttributeSetter.type)
                  {
                    case AnimatorControllerParameterType.Bool:
                      string selected = effect.animatorAttributeSetter.setBool.ToString();

                      EditorGUI.BeginChangeCheck();

                      selected = GUIHelper.Popup("Set boolean to ", selected, booleanArray);

                      if (EditorGUI.EndChangeCheck())
                      {

                        if (selected == "True")
                        {
                          effect.animatorAttributeSetter.setBool = true;
                        }

                        else
                        {
                          effect.animatorAttributeSetter.setBool = false;
                        }

                      }

                      break;

                    case AnimatorControllerParameterType.Float:
                      GUILayout.Label("Set float to ");
                      effect.animatorAttributeSetter.setFloat = EditorGUILayout.FloatField(effect.animatorAttributeSetter.setFloat);
                      break;

                    case AnimatorControllerParameterType.Int:
                      GUILayout.Label("Set integer to ");
                      effect.animatorAttributeSetter.setInt = EditorGUILayout.IntField(effect.animatorAttributeSetter.setInt);
                      break;

                    case AnimatorControllerParameterType.Trigger:
                      GUILayout.Label("Pull the trigger of [ " + effect.animatorAttributeSetter.name + " ]");
                      break;
                  }

                  GUILayout.EndHorizontal();

                  break;

                case Effect.Type.GetItem:
                  GUILayout.BeginHorizontal();
                  UpdateItemList();

                  var itemName = "";

                  if (itemList.Length > 0)
                  {
                    itemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, effect.itemId).name, diplomataEditor.options.currentLanguage).value;
                  }

                  EditorGUI.BeginChangeCheck();

                  itemName = GUIHelper.Popup("Get item ", itemName, itemList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Item item in diplomataEditor.inventory.items)
                    {

                      if (DictionariesHelper.ContainsKey(item.name, diplomataEditor.options.currentLanguage).value == itemName)
                      {
                        effect.itemId = item.id;
                        break;
                      }

                    }
                  }

                  GUILayout.EndHorizontal();
                  break;

                case Effect.Type.DiscardItem:
                  GUILayout.BeginHorizontal();
                  UpdateItemList();

                  var discardItemName = "";

                  if (itemList.Length > 0)
                  {
                    discardItemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, effect.itemId).name, diplomataEditor.options.currentLanguage).value;
                  }

                  EditorGUI.BeginChangeCheck();

                  discardItemName = GUIHelper.Popup("Discard item ", discardItemName, itemList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Item item in diplomataEditor.inventory.items)
                    {

                      if (DictionariesHelper.ContainsKey(item.name, diplomataEditor.options.currentLanguage).value == discardItemName)
                      {
                        effect.itemId = item.id;
                        break;
                      }

                    }
                  }

                  GUILayout.EndHorizontal();
                  break;

                case Effect.Type.EquipItem:
                  GUILayout.BeginHorizontal();
                  UpdateItemList();

                  var equipItemName = "";

                  if (itemList.Length > 0)
                  {
                    equipItemName = DictionariesHelper.ContainsKey(Item.Find(diplomataEditor.inventory.items, effect.itemId).name, diplomataEditor.options.currentLanguage).value;
                  }

                  EditorGUI.BeginChangeCheck();

                  equipItemName = GUIHelper.Popup("Discard item ", equipItemName, itemList);

                  if (EditorGUI.EndChangeCheck())
                  {
                    foreach (Item item in diplomataEditor.inventory.items)
                    {

                      if (DictionariesHelper.ContainsKey(item.name, diplomataEditor.options.currentLanguage).value == equipItemName)
                      {
                        effect.itemId = item.id;
                        break;
                      }

                    }
                  }

                  GUILayout.EndHorizontal();
                  break;

                case Effect.Type.SetGlobalFlag:
                  UpdateGlobalFlagsList();

                  effect.globalFlag.name = GUIHelper.Popup("Flag: ", effect.globalFlag.name, globalFlagsList);

                  string effectSelected = effect.globalFlag.value.ToString();

                  EditorGUI.BeginChangeCheck();

                  effectSelected = GUIHelper.Popup(" set to ", effectSelected, booleanArray);

                  if (EditorGUI.EndChangeCheck())
                  {

                    if (effectSelected == "True")
                    {
                      effect.globalFlag.value = true;
                    }

                    else
                    {
                      effect.globalFlag.value = false;
                    }

                  }

                  break;
              }

              if (GUILayout.Button("Delete Effect", GUILayout.Height(GUIHelper.BUTTON_HEIGHT_SMALL)))
              {
                message.effects = ArrayHelper.Remove(message.effects, effect);
                diplomataEditor.Save(talkable, folderName);
              }

              GUIHelper.Separator();
            }

            if (GUILayout.Button("Add Effect", GUILayout.Height(GUIHelper.BUTTON_HEIGHT)))
            {
              message.effects = ArrayHelper.Add(message.effects, new Effect(talkable.name));
              diplomataEditor.Save(talkable, folderName);
            }

            break;
        }

      }

      GUILayout.Space(GUIHelper.MARGIN);

      if (GUILayout.Button("Remove Empty Columns", GUILayout.Height(GUIHelper.BUTTON_HEIGHT_SMALL)))
      {
        context.columns = Column.RemoveEmptyColumns(context.columns);
        context.messageEditorState = MessageEditorState.None;

        diplomataEditor.Save(talkable, folderName);
      }

      EditorGUILayout.EndScrollView();
    }

    public static void MoveColumnsToRight(Context context, int toIndex)
    {
      for (int i = context.columns.Length - 1; i >= toIndex; i--)
      {
        var col = Column.Find(context, i);
        var rightCol = Column.Find(context, i + 1);

        foreach (Message msg in col.messages)
        {
          msg.columnId += 1;

          msg.id = rightCol.messages.Length;

          rightCol.messages = ArrayHelper.Add(rightCol.messages, msg);
          col.messages = ArrayHelper.Remove(col.messages, msg);

          col.messages = Message.ResetIDs(col.messages);
          rightCol.messages = Message.ResetIDs(rightCol.messages);

          rightCol.emitter = col.emitter;
        }
      }
    }

    public static void UpdateCharacterList()
    {
      var diplomataEditor = TalkableMessagesManager.diplomataEditor;

      characterList = new string[0];

      foreach (string str in diplomataEditor.options.characterList)
      {
        if (str != diplomataEditor.options.playerCharacterName)
        {
          characterList = ArrayHelper.Add(characterList, str);
        }
      }
    }

    public static void UpdateItemList()
    {
      var diplomataEditor = TalkableMessagesManager.diplomataEditor;

      itemList = new string[0];

      foreach (Item item in diplomataEditor.inventory.items)
      {
        itemList = ArrayHelper.Add(itemList, DictionariesHelper.ContainsKey(item.name, diplomataEditor.options.currentLanguage).value);
      }
    }

    public static void UpdateMessagesList(Context context)
    {
      var diplomataEditor = TalkableMessagesManager.diplomataEditor;

      messageList = new string[0];

      foreach (Column col in context.columns)
      {
        foreach (Message msg in col.messages)
        {
          LanguageDictionary content = DictionariesHelper.ContainsKey(msg.content, diplomataEditor.options.currentLanguage);

          if (content == null)
          {
            msg.content = ArrayHelper.Add(msg.content, new LanguageDictionary(diplomataEditor.options.currentLanguage, ""));
            content = DictionariesHelper.ContainsKey(msg.content, diplomataEditor.options.currentLanguage);
          }

          messageList = ArrayHelper.Add(messageList, content.value);
        }
      }
    }

    public static void UpdateContextList()
    {
      var diplomataEditor = TalkableMessagesManager.diplomataEditor;

      contextList = new string[0];

      foreach (Character character in diplomataEditor.characters)
      {
        foreach (Context context in character.contexts)
        {
          LanguageDictionary contextName = DictionariesHelper.ContainsKey(context.name, diplomataEditor.options.currentLanguage);

          if (contextName == null)
          {
            context.name = ArrayHelper.Add(context.name, new LanguageDictionary(diplomataEditor.options.currentLanguage, "Name [Change clicking on Edit]"));
            contextName = DictionariesHelper.ContainsKey(context.name, diplomataEditor.options.currentLanguage);
          }

          contextList = ArrayHelper.Add(contextList, contextName.value);
        }
      }

      foreach (Interactable interactable in diplomataEditor.interactables)
      {
        foreach (Context context in interactable.contexts)
        {
          LanguageDictionary contextName = DictionariesHelper.ContainsKey(context.name, diplomataEditor.options.currentLanguage);

          if (contextName == null)
          {
            context.name = ArrayHelper.Add(context.name, new LanguageDictionary(diplomataEditor.options.currentLanguage, "Name [Change clicking on Edit]"));
            contextName = DictionariesHelper.ContainsKey(context.name, diplomataEditor.options.currentLanguage);
          }

          contextList = ArrayHelper.Add(contextList, contextName.value);
        }
      }
    }

    public static void UpdateGlobalFlagsList()
    {
      globalFlagsList = new string[0];

      foreach (Flag flag in TalkableMessagesManager.diplomataEditor.globalFlags.flags)
      {
        globalFlagsList = ArrayHelper.Add(globalFlagsList, flag.name);
      }
    }

    public static void UpdateLabelsList(Context context)
    {
      labelsList = new string[0];

      foreach (Label label in context.labels)
      {
        labelsList = ArrayHelper.Add(labelsList, label.name);
      }
    }
  }
}
