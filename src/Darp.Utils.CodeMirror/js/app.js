import mirrorsharp from './node_modules/mirrorsharp-codemirror-6-preview';
import {lineNumbers, EditorView, keymap, highlightSpecialChars, drawSelection, dropCursor, rectangularSelection, crosshairCursor, highlightActiveLineGutter} from "@codemirror/view"
import {indentOnInput, bracketMatching, foldKeymap} from "@codemirror/language"
import {defaultKeymap, historyKeymap} from "@codemirror/commands"
import {searchKeymap} from "@codemirror/search"
import {completionKeymap, closeBrackets, closeBracketsKeymap} from "@codemirror/autocomplete"
import {lintKeymap} from "@codemirror/lint"
import { EditorState, Compartment  } from "@codemirror/state";

const contentChangedListener = EditorView.updateListener.of(update => {
  if (update.docChanged) {
    const txt = update.state.doc.toString();
    // invoke the .NET callback to be registered on navigation
    if (window.msTextChanged && window.msTextChanged.invoke)
      window.msTextChanged.invoke(txt);
  }
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
  const currentText = ms.getCodeMirrorView().state.doc.toString()
  if (currentText === text) {
    return;
  }
  text = text.replaceAll("\r\n", "\n").replaceAll("\n", "\r\n")
  ms.setText(text)
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
