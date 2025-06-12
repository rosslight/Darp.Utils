import mirrorsharp from './node_modules/mirrorsharp-codemirror-6-preview';
import {lineNumbers, EditorView, keymap, highlightSpecialChars, drawSelection, dropCursor, rectangularSelection, crosshairCursor, highlightActiveLineGutter} from "@codemirror/view"
import {indentOnInput, bracketMatching, foldKeymap} from "@codemirror/language"
import {defaultKeymap, historyKeymap} from "@codemirror/commands"
import {searchKeymap} from "@codemirror/search"
import {completionKeymap, closeBrackets, closeBracketsKeymap} from "@codemirror/autocomplete"
import {lintKeymap} from "@codemirror/lint"
import { EditorState, Compartment  } from "@codemirror/state";

const contentChangedListener = EditorView.updateListener.of(update => {
  if (!update.docChanged)
    return
  // drop any transaction we ourselves tagged with userEvent="setValue"
  if (update.transactions.some(tr => tr.isUserEvent("cSharpSetValue")))
    return
  const txt = update.state.doc.toString();
  // function provided by Avalonia NativeWebView
  if (window.msTextChanged && window.msTextChanged.invoke)
    window.msTextChanged.invoke(txt);
});

const readOnlyCompartment = new Compartment();

const ms = mirrorsharp(document.getElementById('editor-container'), {
  serviceUrl: window.location.href.replace(/^http(s?:\/\/[^/]+).*$/i, 'ws$1/mirrorsharp'),
  language: "C#",
  text: "",
  codeMirror: {
    extensions: [
      lineNumbers(),
      closeBrackets(),
      contentChangedListener,
      highlightActiveLineGutter(),
      highlightSpecialChars(),
      drawSelection(),
      dropCursor(),
      EditorState.allowMultipleSelections.of(true),
      indentOnInput(),
      bracketMatching(),
      rectangularSelection(),
      crosshairCursor(),
      readOnlyCompartment.of(EditorState.readOnly.of(false)),
      keymap.of([
        ...closeBracketsKeymap,
        ...defaultKeymap,
        ...searchKeymap,
        ...historyKeymap,
        ...foldKeymap,
        ...completionKeymap,
        ...lintKeymap
      ])
    ]
  }
});

// Getter and setter to be called from .Net

window.getMsText = () => ms.getText()
window.setMsText = (text) => {
  const view = ms.getCodeMirrorView()
  view.dispatch({
    changes: {
      from: 0,
      to: view.state.doc.length,
      insert: text
    },
    // tag this transaction as coming from C#
    userEvent: "cSharpSetValue"
  })
}

window.getMsLanguage = () => ms.getLanguage()
window.setMsLanguage = (language) => ms.setLanguage(language)

window.setScriptMode = (scriptMode) => ms.setServerOptions({'x-mode': scriptMode ? "script" : "regular"});

window.setMsTheme = (theme) => ms.setTheme(theme);

window.setMsIsReadOnly = (isReadOnly) => {
  const view = ms.getCodeMirrorView();
  // reconfigure the editable facet
  view.dispatch({
    effects: readOnlyCompartment.reconfigure(
      EditorState.readOnly.of(isReadOnly)
    )
  });
};
